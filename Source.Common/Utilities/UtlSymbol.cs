using CommunityToolkit.HighPerformance;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.Utilities;

public struct UtlSymbol
{
	public static readonly UtlSymId_t UTL_INVAL_SYMBOL = UtlSymId_t.MaxValue;
	// Instance fields
	private static UtlSymbolTableMT? SymbolTable;
	private readonly UtlSymId_t Id;
	private readonly bool ValidId; // Trick because Id may equal 0 when uninitialized.

	// Instance methods
	public UtlSymbol() {
		Id = UTL_INVAL_SYMBOL;
		ValidId = false;
	}
	public UtlSymbol(ReadOnlySpan<char> str) => Id = CurrTable().AddString(str);
	public UtlSymbol(string str) {
		Id = CurrTable().AddString(str);
		ValidId = Id != UTL_INVAL_SYMBOL;
	}
	public UtlSymbol(in UtlSymbol symbol) {
		Id = symbol.Id;
		ValidId = Id != UTL_INVAL_SYMBOL;
	}
	public readonly ReadOnlySpan<char> String() => CurrTable().String(Id);
	public readonly bool IsValid() => ValidId && Id != UTL_INVAL_SYMBOL;

	// Static members
	static bool symbolsInitialized = false;

	// Static methods
	[MemberNotNull(nameof(SymbolTable))]
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
	static void Initialize() {
		if (!symbolsInitialized) {
			SymbolTable = new UtlSymbolTableMT();
			symbolsInitialized = true;
		}
	}
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

	public static UtlSymbolTableMT CurrTable() {
		Initialize();
		return SymbolTable;
	}

	// Operators
	public static bool operator ==(UtlSymbol symbol, ReadOnlySpan<char> str) 
		=> symbol.Id == UTL_INVAL_SYMBOL 
			? false 
			: str == null 
				? symbol.Id == 0 
				: str.Hash() == symbol.Id;
	public static bool operator !=(UtlSymbol symbol, ReadOnlySpan<char> str) 
		=> symbol.Id == UTL_INVAL_SYMBOL 
			? false 
			: str == null 
				? symbol.Id == 0 
				: str.Hash() != symbol.Id;
	public static implicit operator UtlSymId_t(UtlSymbol symbol) => symbol.Id;

	public override readonly bool Equals(object? obj) => obj is UtlSymbol sym && sym.Id == Id;
	public override readonly int GetHashCode() => Id.GetHashCode();
}
