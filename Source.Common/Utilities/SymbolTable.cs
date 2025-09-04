using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.Utilities;

public class SymbolTable(bool caseInsensitive = false)
{
	Dictionary<ulong, string> Symbols = [];
	public ulong AddString(ReadOnlySpan<char> str) {
		ulong hash = str.Hash(invariant: caseInsensitive);
		if (!Symbols.ContainsKey(hash))
			Symbols[hash] = new(str);
		return hash;
	}
	public ulong Find(ReadOnlySpan<char> str) {
		ulong hash = str.Hash(invariant: caseInsensitive);
		if (Symbols.ContainsKey(hash))
			return hash;
		return 0;
	}
	public ReadOnlySpan<char> String(ulong symbol) {
		if (Symbols.TryGetValue(symbol, out string? str))
			return str;
		return null;
	}
	public nint GetNumStrings() => Symbols.Count;
	public void RemoveAll() => Symbols.Clear();
}
