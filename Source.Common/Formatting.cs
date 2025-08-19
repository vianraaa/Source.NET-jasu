using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common;
public enum VariableType
{
	SignedDecimalInteger = 'i',
	UnsignedDecimalInteger = 'u',
	UnsignedOctal = 'o',
	UnsignedHexadecimalInteger = 'x',
	DecimalFloatingPoint = 'f',
	ScientificNotation = 'e',
	ShortestRepresentation = 'g',
	HexadecimalFloatingPoint = 'a',
	Character = 'c',
	StringOfCharacters = 's',
	PointerAddress = 'p',
	Nothing = 'n'
}

public delegate void IncomingLiteral(ReadOnlySpan<char> slice);
// Return true to prevent writing it as a literal.
public delegate bool IncomingVariable(int idx, VariableType var);

public static class Formatting
{
	public static void Parse(this ReadOnlySpan<char> str, IncomingLiteral literal, IncomingVariable variable) {
		const int MAX_LITERAL = 256;
		Span<char> stringWork = stackalloc char[MAX_LITERAL];
		int stringWriter = 0;
		int reachedVariable = 0;

		void dispatchLiteral(Span<char> s) {
			if (literalEmpty()) return;
			literal(s[..stringWriter]);
			stringWriter = 0;
		}
		bool dispatchVariable(VariableType type) {
			return variable(reachedVariable++, type);
		}
		bool literalEmpty() => stringWriter <= 0;
		bool literalOverflow() => stringWriter >= MAX_LITERAL;
		void writeToLiteral(Span<char> s, char c) {
			if (literalOverflow())
				dispatchLiteral(s);
			s[stringWriter++] = c;
		}

		for (int i = 0; i < str.Length; i++) {
			char c = str[i];

			if (c == '%') {
				i++;
				dispatchLiteral(stringWork);
				// Start reading a format
				int c2 = char.ToLowerInvariant(str[i]);
				switch (c2) {
					case '%': writeToLiteral(stringWork, (char)c2); break;
					case 'd':
						if (!dispatchVariable(VariableType.SignedDecimalInteger)) {
							writeToLiteral(stringWork, '%');
							writeToLiteral(stringWork, (char)c2);
						}
						break;
					default:
						dispatchVariable((VariableType)c2); break;
				}
			}
			else
				writeToLiteral(stringWork, c);
		}

		dispatchLiteral(stringWork);
	}

	public static void Print(this ReadOnlySpan<char> format, IncomingLiteral howToPrint, params object?[] args) =>
		Parse(format, howToPrint, (varIdx, varType) => {
			if (varIdx >= args.Length)
				return false; // just write to literal buffer, we cant read an arg
			Span<char> writeBuffer = stackalloc char[256];
			int writeSize = 0;
			bool useWriteBuffer = false;
			switch (args[varIdx]) {
				case int v when varType == VariableType.SignedDecimalInteger:
					v.TryFormat(writeBuffer, out writeSize);
					useWriteBuffer = true;
					break;
				case uint v when varType == VariableType.UnsignedDecimalInteger:
					v.TryFormat(writeBuffer, out writeSize);
					useWriteBuffer = true;
					break;
				case uint v when varType == VariableType.UnsignedOctal:
					string uoctal = Convert.ToString(v, 8);
					howToPrint(uoctal);
					break;
				case uint v when varType == VariableType.UnsignedHexadecimalInteger:
					v.TryFormat(writeBuffer, out writeSize, "x");
					useWriteBuffer = true;
					break;
				case double v when varType == VariableType.DecimalFloatingPoint:
					v.TryFormat(writeBuffer, out writeSize, "F");
					useWriteBuffer = true;
					break;
				case double v when varType == VariableType.ScientificNotation:
					v.TryFormat(writeBuffer, out writeSize, "E");
					useWriteBuffer = true;
					break;
				case double v when varType == VariableType.ShortestRepresentation:
					v.TryFormat(writeBuffer, out writeSize, "G");
					useWriteBuffer = true;
					break;
				case double v when varType == VariableType.HexadecimalFloatingPoint:
					var bits = BitConverter.DoubleToInt64Bits(v);
					writeSize = bits.TryFormat(writeBuffer, out var written, "X") ? written : 0;
					useWriteBuffer = true;
					break;
				case char v when varType == VariableType.Character:
					writeBuffer[0] = v;
					writeSize = 1;
					useWriteBuffer = true;
					break;
				case string v when varType == VariableType.StringOfCharacters:
					v.AsSpan().CopyTo(writeBuffer);
					writeSize = v.Length;
					useWriteBuffer = true;
					break;
				case object v when varType == VariableType.PointerAddress:
					var ptrStr = $"0x{v.GetHashCode():X}";
					ptrStr.AsSpan().CopyTo(writeBuffer);
					writeSize = ptrStr.Length;
					useWriteBuffer = true;
					break;
				case object _ when varType == VariableType.Nothing:
					useWriteBuffer = false;
					break;
				default:
					useWriteBuffer = false;
					break;
			}
			if (useWriteBuffer)
				howToPrint(writeBuffer[..writeSize]);
			return true; // do not write to literal buffer
		});
}