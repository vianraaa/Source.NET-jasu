using Source.Common.Client;
using Source.Common.Filesystem;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.Formats.Keyvalues;


public class KeyValues : LinkedList<KeyValues>
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
	public string StringValue;
	
	bool useEscapeSequences;

	public KeyValues() {

	}
	public KeyValues(string name) {
		Name = name;
	}

	public bool LoadFromStream(Stream? stream) {
		if (stream == null) return false;

		using StreamReader reader = new StreamReader(stream);
		return ReadKV(reader);
	}

	private void SkipWhitespace(StreamReader reader) {
		while (true) {
			int c = reader.Peek();
			if (c == -1) 
				return;
			if (!char.IsWhiteSpace((char)c))
				return;
			reader.Read();
		}
	}

	private bool ReadKV(StreamReader reader) {
		// We read either
		//    1. A quote mark, in which case we need to read up to a quote
		//    2. Anything else, we read until whitespace
		SkipWhitespace(reader);
		SkipComments(reader);

		string key = (char)reader.Peek() == '"' ? ReadQuoteTerminatedString(reader, useEscapeSequences) : ReadWhitespaceTerminatedString(reader);
		Name = key;
		// Determine what we're reading next.
		// If we run into a {, we read another KVObject and set our value to that.
		// If we run into a ", we read another string-based value terminated by quotes.
		// If we run into anything else, we read another string-based value, terminated by space or EOF.
		// The value will then be set based on the string. int.TryParse will try to make it an int, same for double, and Color.
		// We then will leave.
		SkipWhitespace(reader);
		SkipComments(reader);

		char nextAction = (char)reader.Peek();
		switch (nextAction) {
			case '{':
				// Ok, now we need to read every single key value until we hit a }
				ReadKVPairs(reader);
				Type = Types.None;
				break;
			case '"':
				string value1 = ReadQuoteTerminatedString(reader, useEscapeSequences);
				DetermineValueType(value1);
				break;
			default:
				string value2 = ReadWhitespaceTerminatedString(reader);
				DetermineValueType(value2);
				break;
		}

		return true;
	}

	private void ReadKVPairs(StreamReader reader) {
		int rd = reader.Read();
		Dbg.Assert(rd == '"', "invalid kvpairs");

		while (true) {
			SkipWhitespace(reader);
			if (reader.Peek() == '}') {
				reader.Read();
				break;
			}
			SkipComments(reader);
			// Start reading keyvalues.
			KeyValues kvpair = new();
			kvpair.ReadKV(reader);
			AddLast(kvpair);
		}
	}

	private void SkipComments(StreamReader reader) {
		if (reader.Peek() == '/') {
			// We need to check the stream for another /
			reader.BaseStream.Seek(1, SeekOrigin.Current);
			if (reader.Peek() == '/') { // We got //, its a comment
										// We read until the end of the line.
				while (true) {
					char c = (char)reader.Read();
					if (c == '\n')
						break;
				}
			}
			else {
				// Go back, the / is fine.
				reader.BaseStream.Seek(-1, SeekOrigin.Current);
			}
		}
	}

	private void DetermineValueType(string input) {
		StringValue = input;

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
		Dbg.Assert(rd == '"', "invalid quote-terminated string");
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

	public KeyValues? FindKey(string v) {
		foreach(var child in this) {
			if (child.Name == v)
				return child;
		}

		return null;
	}

	public string GetString() => StringValue ?? "";
	public string? GetString(string key) => FindKey(key)?.GetString();

	public bool LoadFromFile(IFileSystem fileSystem, ReadOnlySpan<char> path, ReadOnlySpan<char> pathID) {
		return LoadFromStream(fileSystem.Open(path, FileOpenOptions.Read, pathID)?.Stream);
	}
	public bool LoadFromFile(IFileSystem fileSystem, ReadOnlySpan<char> path) {
		return LoadFromStream(fileSystem.Open(path, FileOpenOptions.Read, null)?.Stream);
	}
}
