using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common;
public enum VariableType
{
	NotApplicable = '\0',
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

public ref struct CFormatReader
{
	int formatReader = 0;
	bool haltedAtVariable;
	int variableIdx;
	readonly ReadOnlySpan<char> format;

	public CFormatReader(ReadOnlySpan<char> format) {
		this.format = format;
	}

	public bool Overflowed() => formatReader >= format.Length;

	/// <summary>
	/// Reads one literal into a read target buffer.
	/// </summary>
	/// <param name="readTarget">A target buffer</param>
	/// <returns>How much was written to the target. 0 if any literal was read at all. 0 if a halt break occured due to an incoming variable or end-of-stream.</returns>
	public int ReadLiteral(Span<char> readTarget) {
		if (haltedAtVariable)
			return 0;

		int chars = 0;
		for (int i = 0; i < readTarget.Length; i++) {
			if (Overflowed())
				return i;

			char c = format[formatReader];
			if (c == '%') {
				i++;
				if (format[formatReader + 1] != '%') {
					// We have to stop, a variable was reached
					formatReader++;
					haltedAtVariable = true;
					break; // We wrote something if i > 0, even if we guarded here
				}
			}
			// Otherwise, write
			readTarget[chars++] = c;
			formatReader++;
		}

		return chars;
	}
	/// <summary>
	/// Tries to read one variable type. ReadLiteral must have triggered a halt before this function will return true.
	/// </summary>
	/// <param name="type">Out variable type</param>
	/// <param name="variableIdx">Out variable index</param>
	/// <returns>True if a variable could be read. False if a halt break hasn't occured, or if the format pointer overflowed</returns>
	public bool ReadVariable(out char type, out int variableIdx) {
		type = (char)VariableType.NotApplicable;
		variableIdx = -1;
		if (!haltedAtVariable)
			return false;
		if (Overflowed())
			return false;

		type = char.ToLowerInvariant(format[formatReader++]);
		variableIdx = this.variableIdx;
		this.variableIdx++;
		haltedAtVariable = false;
		return true;
	}
}

public static class CFormatUtils
{
	public static int WriteFormattedObject(Span<char> writeBuffer, char type, object? arg) {
		int writeSize = 0;
		bool useWriteBuffer = false;

		VariableType varType;
		if (type == 'd')
			varType = VariableType.SignedDecimalInteger;
		else
			varType = (VariableType)type;

		switch (arg) {
			case int v when varType == VariableType.SignedDecimalInteger:
				v.TryFormat(writeBuffer, out writeSize);
				useWriteBuffer = true;
				break;
			case uint v when varType == VariableType.UnsignedDecimalInteger:
				v.TryFormat(writeBuffer, out writeSize);
				useWriteBuffer = true;
				break;
			case uint v when varType == VariableType.UnsignedOctal:
				string uoctal = Convert.ToString(v, 8); // ugh
				uoctal.CopyTo(writeBuffer);
				writeSize = uoctal.Length;
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
		return writeSize;
	}
}

public static class CFormatting
{
	public static unsafe int sprintf(Span<char> target, ReadOnlySpan<char> format, params object?[] args) {
		CFormatReader reader = new(format);
		return sprintf(target, ref reader, args);
	}
	public static unsafe int sprintf(Span<char> target, ref CFormatReader reader, params object?[] args) {
		int originalSize = target.Length;

		Span<char> buffer = stackalloc char[256];
		while (!reader.Overflowed()) {
			// Try reading literal
			Span<char> read = buffer[0..reader.ReadLiteral(buffer)];
			if (read.Length > 0) {
				read.CopyTo(target);
				target = target[read.Length..];
			}
			// Try reading a variable?
			else if (reader.ReadVariable(out char type, out int idx)) {
				if (idx < args.Length) {
					int writtenVar = CFormatUtils.WriteFormattedObject(target, type, args[idx]);
					target = target[writtenVar..];
				}
				else {
					// Skip it
					target[0] = '%';
					target[1] = type;
					target = target[2..];
				}
			}
			else {
				if (!reader.Overflowed())
					throw new FormatException("Unexpected non-overflow, non-variable, and non-literal condition met...");
			}
		}

		return originalSize - target.Length; // Should return the delta length
	}
}