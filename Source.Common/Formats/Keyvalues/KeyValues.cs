using Source.Common.Client;
using Source.Common.Filesystem;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.Formats.Keyvalues;


[DebuggerDisplay("Name = {Name}, Type = {Type}, Count = {Count}, Value = {Value}")]
public class KeyValues : IEnumerable<KeyValues>
{
	public enum Types
	{
		None = 0,
		String,
		Int,
		Double,
		Pointer,
		Color,
		Uint64,
	}

	public string Name = "";
	public Types Type;
	public object? Value;
	public string? RawValue;

	bool useEscapeSequences = false;
	bool evaluateConditionals = false;

	LinkedListNode<KeyValues> node;
	LinkedList<KeyValues> children = [];

	public KeyValues() {
		node = new(this);
	}

	public KeyValues(ReadOnlySpan<char> name) : base() {
		node = new(this);
		Name = new(name);
	}

	public bool LoadFromStream(Stream? stream) {
		// Clear();
		if (stream == null) return false;

		using StreamReader reader = new StreamReader(stream);
		return ReadKV(reader);
	}

	// Returns true if we did anything at all to skip whitespace.
	private bool SkipWhitespace(StreamReader reader) {
		bool didAnything = false;
		while (true) {
			int c = reader.Peek();
			if (c == -1)
				break;
			if (!char.IsWhiteSpace((char)c))
				break;
			reader.Read();
			didAnything = true;
		}

		return didAnything;
	}

	public override string ToString() {
		return $"{Type}<{Value}>";
	}

	// Returns true if we can read something. False if we can't.
	private bool SkipUntilParseableTextOrEOF(StreamReader reader) {
		// We read either
		//    1. A quote mark, in which case we need to read up to a quote
		//    2. Anything else, we read until whitespace

		while (true) {
			if (reader.Peek() == -1)
				return false;
			// If no whitespace was skipped and no comments were skipped, continue
			if (!SkipWhitespace(reader) && !SkipComments(reader))
				return true;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="condition"></param>
	/// <param name="match">If [$CONDITION], this value will be true, and if [!$CONDITION], this value will be false.</param>
	/// <returns></returns>
	private bool ReadConditional(StreamReader reader, Span<char> condition, out bool match) {
		// Zero out if it's existing memory
		for (int si = 0; si < condition.Length; si++)
			condition[si] = '\0';

		if (reader.Peek() != '[') {
			match = false;
			return false;
		}

		reader.Read();
		// Determine if we're inverted.
		char c = (char)reader.Read();
		match = true;
		if (c == '!') {
			match = false;
			c = (char)reader.Read();
		}

		if (c != '$') {
			Warning("ReadConditional failed.\n");
			return false;
		}

		int i = -1;
		while (++i < condition.Length) {
			char readIn = (char)reader.Read();
			if (readIn == ']')
				break;
			condition[i] = readIn;
		}
		return true;
	}

	private bool HandleConditional(ReadOnlySpan<char> condition, bool mustMatch) {
		int realStrLength = condition.IndexOf('\0');
		if(realStrLength == -1) {
			Debug.Assert(false, "String overflow!!!");
			return false;
		}
		condition = condition[..realStrLength];

		switch (condition) {
#if WIN32
			case "WIN32": return mustMatch;
			case "WINDOWS": return mustMatch;
			case "X360": return !mustMatch;
			case "OSX": return !mustMatch;
			case "POSIX": return !mustMatch;
			case "LINUX": return !mustMatch;
#elif OSX
			case "WIN32": return !mustMatch;
			case "WINDOWS": return !mustMatch;
			case "X360": return !mustMatch;
			case "OSX": return mustMatch;
			case "POSIX": return !mustMatch;
			case "LINUX": return !mustMatch;
#elif LINUX
			case "WIN32": return !mustMatch;
			case "WINDOWS": return !mustMatch;
			case "X360": return !mustMatch;
			case "OSX": return !mustMatch;
			case "POSIX": return mustMatch;
			case "LINUX": return mustMatch;
#else
#error Please define how KeyValues.HandleConditional should work on this platform.
#endif
		}
		// Other platforms are not applicable and we should just throw them away
		return !mustMatch;
	}

	private bool ReadKV(StreamReader reader) {
		SkipUntilParseableTextOrEOF(reader);

		bool quoteTerminated = (char)reader.Peek() == '"';

		string key = quoteTerminated ? ReadQuoteTerminatedString(reader, useEscapeSequences) : ReadWhitespaceTerminatedString(reader);
		Name = key;

		SkipUntilParseableTextOrEOF(reader);

		Span<char> conditional = stackalloc char[16];
		bool isBlockConditional = ReadConditional(reader, conditional, out bool mustMatch);
		bool matches = isBlockConditional ? HandleConditional(conditional, mustMatch) : true;

		SkipUntilParseableTextOrEOF(reader);

		// Determine what we're reading next.
		// If we run into a {, we read another KVObject and set our value to that.
		// If we run into a ", we read another string-based value terminated by quotes.
		// If we run into anything else, we read another string-based value, terminated by space or EOF.
		// The value will then be set based on the string. int.TryParse will try to make it an int, same for double, and Color.
		// We then will leave.

		char nextAction = (char)reader.Peek();
		string value;
		switch (nextAction) {
			case '{':
				// Ok, now we need to read every single key value until we hit a }
				ReadKVPairs(reader, matches);
				Type = Types.None;
				break;
			case '"':
				value = ReadQuoteTerminatedString(reader, useEscapeSequences);
				goto valueTypeSpecific;
			default:
				value = ReadWhitespaceTerminatedString(reader);
				goto valueTypeSpecific;
		}

		return true;

	valueTypeSpecific:

		DetermineValueType(value);
		SkipUntilParseableTextOrEOF(reader);
		bool isValueConditional = ReadConditional(reader, conditional, out bool valueMustMatch);
		bool valueMatches = isValueConditional ? HandleConditional(conditional, valueMustMatch) : true;
		return valueMatches;
	}

	private void ReadKVPairs(StreamReader reader, bool matches) {
		int rd = reader.Read();

		while (true) {
			SkipWhitespace(reader);
			if (reader.Peek() == '}') {
				reader.Read();
				break;
			}
			SkipComments(reader);
			// Start reading keyvalues.
			KeyValues kvpair = new();
			;
			if (kvpair.ReadKV(reader) && matches) // When conditional, stil need to waste time on parsing, but we throw it away after
					                              // There's definitely a better way to handle this, but it would need more testing scenarios
												  // The ReadKV call can also determine its condition and will return false if it doesnt want to be added.
				children.AddLast(kvpair.node);
		}
	}

	// Returns true if we did anything at all to skip comments.
	private bool SkipComments(StreamReader reader) {
		bool didAnything = false;
		if (reader.Peek() == '/') {
			// We need to check the stream for another /
			reader.Read();
			if (reader.Peek() == '/') { // We got //, its a comment
										// We read until the end of the line.
				didAnything = true;
				while (true) {
					char c = (char)reader.Read();
					if (c == '\n')
						break;
				}
			}
			else {
				// What...
				throw new InvalidOperationException("Expected comment");
			}
		}

		return didAnything;
	}

	private void DetermineValueType(string input) {
		RawValue = input;

		// Try Int32
		if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i32)) {
			Value = i32;
			Type = Types.Int;
		}

		// Try UInt64
		if (ulong.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong ui64)) {
			Value = ui64;
			Type = Types.Uint64;
		}

		// Try Double
		if (double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double d64)) {
			Value = d64;
			Type = Types.Double;
		}

		Value = input;
		Type = Types.String;
	}

	private string ReadWhitespaceTerminatedString(StreamReader reader) {
		Span<char> work = stackalloc char[1024];
		int i, len;
		for (i = 0, len = work.Length; i < len; i++) {
			char c = (char)reader.Peek();
			if (c == -1) break;
			if (char.IsWhiteSpace(c)) break;
			work[i] = c;
			reader.Read();
		}

		if (i >= len)
			Dbg.Warning("KeyValues: string overflow, ignoring (should we allocate more space?)\n");

		return new(work[..i]);
	}

	private string ReadQuoteTerminatedString(StreamReader reader, bool useEscapeSequences) {
		int rd = reader.Read();
		Debug.Assert(rd == '"', "invalid quote-terminated string");
		Span<char> work = stackalloc char[1024];
		int i, len;
		bool lastCharacterWasEscape = false;
		for (i = 0, len = work.Length; i < len; i++) {
			char c = (char)reader.Peek();
			if (c == -1) break;
			if (lastCharacterWasEscape) {
				lastCharacterWasEscape = false;
			}
			else {
				if (c == '"') {
					reader.Read();
					break;
				}
				else if (c == '\\' && useEscapeSequences) {
					lastCharacterWasEscape = true;
					continue;
				}
			}
			work[i] = c;
			reader.Read();
		}

		if (i >= len)
			Dbg.Warning("KeyValues: string overflow, ignoring (should we allocate more space?)\n");

		return new(work[..i]);
	}

	public bool LoadFromFile(string? filepath) {
		if (filepath == null) return false;

		FileInfo info = new(filepath);
		FileStream stream;
		try { stream = info.OpenRead(); }
		catch { return false; }

		bool ok = LoadFromStream(stream);
		stream.Dispose();
		return ok;
	}


	public KeyValues? FindKey(string searchStr, bool create = false) {
		foreach (var child in this.children) {
			if (child.Name == searchStr)
				return child;
		}

		if (create) {
			KeyValues newKey = new(searchStr);
			newKey.useEscapeSequences = useEscapeSequences;

			children.AddLast(newKey.node);
			return newKey;
		}

		return null;
	}

	public string GetString() => Value is string str ? (str ?? "") : "";
	public string? GetString(string key) => FindKey(key)?.GetString();

	public void SetString(string keyName, ReadOnlySpan<char> value) {
		KeyValues? dat = FindKey(keyName, true);
		if (dat != null) {
			if (dat.Type == Types.String && value.Equals(dat.Value?.ToString(), StringComparison.Ordinal))
				return;

			if (value == null)
				value = "";
			dat.Value = new string(value);
			dat.Type = Types.String;
		}
	}
	public void SetInt(string keyName, int value) {
		KeyValues? dat = FindKey(keyName, true);
		if (dat != null) {
			dat.Value = value;
			dat.Type = Types.Int;
		}
	}

	public bool LoadFromFile(IFileSystem fileSystem, ReadOnlySpan<char> path, ReadOnlySpan<char> pathID) {
		return LoadFromStream(fileSystem.Open(path, FileOpenOptions.Read, pathID)?.Stream);
	}
	public bool LoadFromFile(IFileSystem fileSystem, ReadOnlySpan<char> path) {
		return LoadFromStream(fileSystem.Open(path, FileOpenOptions.Read, null)?.Stream);
	}

	public KeyValues? GetFirstSubKey() => children.First?.Value;

	public int GetInt() {
		return Convert.ToInt32(Value);
	}
	public float GetFloat() {
		return Convert.ToSingle(Value);
	}
	public double GetDouble() {
		return Convert.ToDouble(Value);
	}

	public void UsesEscapeSequences(bool value) {
		useEscapeSequences = value;
	}

	public KeyValues? GetNextKey() {
		return node.Next?.Value;
	}

	public IEnumerator<KeyValues> GetEnumerator() {
		foreach(var key in children)
			yield return key;
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
