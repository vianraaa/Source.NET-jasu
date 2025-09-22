using Source.Common;
using Source.Common.Bitbuffers;
using Source.Common.Commands;
using Source.Common.Mathematics;
using Source.Engine.Server;

using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static Source.Common.Networking.svc_ClassInfo;

namespace Source.Engine;

public struct DecodeInfo
{
	public RecvProxyData RecvProxyData;
	public FieldInfo FieldInfo;
	public object Object;
	public SendProp Prop;
	public bf_read In;
}

public static class PackedEntitiesManager
{
	public static ReadOnlySpan<char> GetObjectClassName(int objectID) => "[unknown]";
}

public delegate void EncodeFn(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID);
public delegate void DecodeFn(ref DecodeInfo info);
public delegate int CompareDeltasFn(SendProp prop, bf_read p1, bf_read p2);
public delegate void FastCopyFn(SendProp prop, bf_read p1, bf_read p2);
public delegate ReadOnlySpan<char> GetTypeNameStringFn();
public delegate bool IsZeroFn(object instance, ref DVariant var, SendProp prop);
public delegate void DecodeZeroFn(ref DecodeInfo info);
public delegate bool IsEncodedZeroFn(SendProp prop, bf_read p);
public delegate void SkipPropFn(SendProp prop, bf_read p);

public struct PropTypeFns
{
	// Field members
	public EncodeFn Encode;
	public DecodeFn Decode;
	public CompareDeltasFn CompareDeltas;
	public FastCopyFn FastCopy;
	public GetTypeNameStringFn GetTypeNameString;
	public IsZeroFn IsZero;
	public DecodeZeroFn DecodeZero;
	public IsEncodedZeroFn IsEncodedZero;
	public SkipPropFn SkipProp;

	public PropTypeFns(EncodeFn encode, DecodeFn decode, CompareDeltasFn compareDeltas, FastCopyFn fastCopy, GetTypeNameStringFn getTypeNameString,
					   IsZeroFn isZero, DecodeZeroFn decodeZero, IsEncodedZeroFn isEncodedZero, SkipPropFn skipProp) {
		Encode = encode;
		Decode = decode;
		CompareDeltas = compareDeltas;
		FastCopy = fastCopy;
		GetTypeNameString = getTypeNameString;
		IsZero = isZero;
		DecodeZero = decodeZero;
		IsEncodedZero = isEncodedZero;
		SkipProp = skipProp;
	}

	public static void EncodeFloat(SendProp prop, float incoming, bf_write outBuffer, int objectID) {
		var flags = prop.GetFlags();
		if ((flags & PropFlags.Coord) != 0)
			outBuffer.WriteBitCoord(incoming);
		else if ((flags & (PropFlags.CoordMP | PropFlags.CoordMPLowPrecision | PropFlags.CoordMPIntegral)) != 0)
			outBuffer.WriteBitCoordMP(incoming, (((int)flags >> 15) & 1) != 0, (((int)flags >> 14) & 1) != 0);
		else if ((flags & PropFlags.Normal) != 0)
			outBuffer.WriteBitNormal(incoming);
		else {
			uint val;
			int bits = prop.Bits;
			if ((flags & PropFlags.NoScale) != 0) {
				val = MemoryMarshal.Cast<float, byte>(new ReadOnlySpan<float>(ref incoming))[0];
				bits = 32;
			}
			else if (incoming < prop.LowValue) {
				val = 0;

				if ((flags & PropFlags.RoundUp) == 0)
					Warning($"(class {PackedEntitiesManager.GetObjectClassName(objectID)}): Out-of-range value ({incoming} < {prop.LowValue}) in SendPropFloat '{prop.FieldInfo}', clamping.\n");
			}
			else if (incoming > prop.HighValue) {
				val = (uint)((1 << prop.Bits) - 1);

				if ((flags & PropFlags.RoundDown) == 0)
					Warning($"(class {PackedEntitiesManager.GetObjectClassName(objectID)}): Out-of-range value ({incoming} > {prop.HighValue}) in SendPropFloat '{prop.FieldInfo}', clamping.\n");
			}
			else {
				float fRangeVal = (incoming - prop.LowValue) * prop.HighLowMul;
				if (prop.Bits <= 22)
					val = (uint)MathLib.FastFloatToSmallInt(fRangeVal);
				else
					val = MathLib.RoundFloatToUnsignedLong(fRangeVal);
			}
			outBuffer.WriteUBitLong(val, bits);
		}
	}


	internal static float DecodeFloat(SendProp prop, bf_read inBuffer) {
		var flags = prop.GetFlags();
		if ((flags & PropFlags.Coord) != 0)
			return inBuffer.ReadBitCoord();
		else if ((flags & (PropFlags.CoordMP | PropFlags.CoordMPLowPrecision | PropFlags.CoordMPIntegral)) != 0)
			return inBuffer.ReadBitCoordMP((((int)flags >> 15) & 1) != 0, (((int)flags >> 14) & 1) != 0);
		else if ((flags & PropFlags.NoScale) != 0)
			return inBuffer.ReadBitFloat();
		else if ((flags & PropFlags.Normal) != 0)
			return inBuffer.ReadBitNormal();
		else {
			uint dwInterp = inBuffer.ReadUBitLong(prop.Bits);
			float fVal = (float)dwInterp / ((1 << prop.Bits) - 1);
			fVal = prop.LowValue + (prop.HighValue - prop.LowValue) * fVal;
			return fVal;
		}
	}

	public static Vector3 DecodeVector(SendProp prop, bf_read inBuffer) {
		Span<float> stackFloats = stackalloc float[3];
		DecodeVector(prop, inBuffer, stackFloats);
		return new(stackFloats);
	}

	public static void DecodeVector(SendProp prop, bf_read inBuffer, Span<float> v) {
		v[0] = DecodeFloat(prop, inBuffer);
		v[1] = DecodeFloat(prop, inBuffer);

		if ((prop.GetFlags() & PropFlags.Normal) == 0)
			v[2] = DecodeFloat(prop, inBuffer);
		else {
			int signbit = inBuffer.ReadOneBit();

			float v0v0v1v1 = v[0] * v[0] +
				v[1] * v[1];
			if (v0v0v1v1 < 1.0f)
				v[2] = MathF.Sqrt(1.0f - v0v0v1v1);
			else
				v[2] = 0.0f;

			if (signbit != 0)
				v[2] *= -1.0f;
		}
	}

	/// <summary>
	/// Indices correlate to <see cref="SendPropType"/> enum values.
	/// </summary>
	static readonly PropTypeFns[] g_PropTypeFns = [
		new(Int_Encode, Int_Decode, Int_CompareDeltas, Generic_FastCopy, Int_GetTypeNameString, Int_IsZero, Int_DecodeZero, Int_IsEncodedZero, Int_SkipProp),
		new(Float_Encode, Float_Decode, Float_CompareDeltas, Generic_FastCopy, Float_GetTypeNameString, Float_IsZero, Float_DecodeZero, Float_IsEncodedZero, Float_SkipProp),
		new(Vector_Encode, Vector_Decode, Vector_CompareDeltas, Generic_FastCopy, Vector_GetTypeNameString, Vector_IsZero, Vector_DecodeZero, Vector_IsEncodedZero, Vector_SkipProp),
		new(VectorXY_Encode, VectorXY_Decode, VectorXY_CompareDeltas, Generic_FastCopy, VectorXY_GetTypeNameString, VectorXY_IsZero, VectorXY_DecodeZero, VectorXY_IsEncodedZero, VectorXY_SkipProp),
		new(String_Encode, String_Decode, String_CompareDeltas, Generic_FastCopy, String_GetTypeNameString, String_IsZero, String_DecodeZero, String_IsEncodedZero, String_SkipProp),
		new(Array_Encode, Array_Decode, Array_CompareDeltas, Generic_FastCopy, Array_GetTypeNameString, Array_IsZero, Array_DecodeZero, Array_IsEncodedZero, Array_SkipProp),
		new(DataTable_Encode, DataTable_Decode, DataTable_CompareDeltas, Generic_FastCopy, DataTable_GetTypeNameString, DataTable_IsZero, DataTable_DecodeZero, DataTable_IsEncodedZero, DataTable_SkipProp),
		new(GModTable_Encode, GModTable_Decode, GModTable_CompareDeltas, Generic_FastCopy, GModTable_GetTypeNameString, GModTable_IsZero, GModTable_DecodeZero, GModTable_IsEncodedZero, GModTable_SkipProp)
	];

	public static ref readonly PropTypeFns Get(SendPropType propType) => ref g_PropTypeFns[(int)propType];

	// Implementations for prop type fns.
	private static void Generic_FastCopy(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	#region SendPropType.Int
	public static int Int_CompareDeltas(SendProp prop, bf_read p1, bf_read p2) {
		if ((prop.GetFlags() & PropFlags.VarInt) != 0) {
			if ((prop.GetFlags() & PropFlags.Unsigned) != 0)
				return (p1.ReadVarInt32() != p2.ReadVarInt32()) ? 1 : 0;

			return (p1.ReadSignedVarInt32() != p2.ReadSignedVarInt32()) ? 1 : 0;
		}

		return p1.CompareBits(p2, prop.Bits) ? 1 : 0;
	}
	public static void Int_Decode(ref DecodeInfo decodeInfo) {
		SendProp prop = decodeInfo.Prop;
		PropFlags flags = prop.GetFlags();

		if ((flags & PropFlags.VarInt) != 0) {
			if ((flags & PropFlags.Unsigned) != 0)
				decodeInfo.RecvProxyData.Value.Int = (int)decodeInfo.In.ReadVarInt32();
			else
				decodeInfo.RecvProxyData.Value.Int = decodeInfo.In.ReadSignedVarInt32();
		}
		else {
			int bits = prop.Bits;
			decodeInfo.RecvProxyData.Value.Int = (int)decodeInfo.In.ReadUBitLong(bits);

			if (bits != 32 && (flags & PropFlags.Unsigned) == 0) {
				uint highbit = (uint)(1 << (prop.Bits - 1));
				if ((decodeInfo.RecvProxyData.Value.Int & highbit) != 0) {
					decodeInfo.RecvProxyData.Value.Int -= (int)highbit;
					decodeInfo.RecvProxyData.Value.Int -= (int)highbit;
				}
			}
		}

		decodeInfo.RecvProxyData.RecvProp?.GetProxyFn()(ref decodeInfo.RecvProxyData, decodeInfo.Object, decodeInfo.FieldInfo);
	}
	public static void Int_DecodeZero(ref DecodeInfo info) {
		info.RecvProxyData.Value.Int = 0;

		info.RecvProxyData.RecvProp?.GetProxyFn()(ref info.RecvProxyData, info.Object, info.FieldInfo);
	}
	public static void Int_Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		int value = var.Int;

		if ((prop.GetFlags() & PropFlags.VarInt) != 0) {
			if ((prop.GetFlags() & PropFlags.Unsigned) != 0)
				writeOut.WriteVarInt32((uint)value);
			else
				writeOut.WriteSignedVarInt32(value);
		}
		else {
			int nPreserveBits = (0x7FFFFFFF >> (32 - prop.Bits));
			nPreserveBits |= (int)(((prop.GetFlags() & PropFlags.Unsigned) != 0) ? 0xFFFFFFFF : 0);
			int nSignExtension = (value >> 31) & ~nPreserveBits;

			value &= nPreserveBits;
			value |= nSignExtension;

			writeOut.WriteUBitLong((uint)value, prop.Bits, false);
		}
	}
	public static ReadOnlySpan<char> Int_GetTypeNameString() => "DPT_Int";
	public static bool Int_IsEncodedZero(SendProp prop, bf_read p) {
		if ((prop.GetFlags() & PropFlags.VarInt) != 0) {
			if ((prop.GetFlags() & PropFlags.Unsigned) != 0)
				return p.ReadVarInt32() == 0;

			return p.ReadSignedVarInt32() == 0;
		}

		return p.ReadUBitLong(prop.Bits) == 0;
	}
	public static bool Int_IsZero(object instance, ref DVariant var, SendProp prop) => var.Int == 0;
	public static void Int_SkipProp(SendProp prop, bf_read p) {
		if ((prop.GetFlags() & PropFlags.VarInt) != 0) {
			if ((prop.GetFlags() & PropFlags.Unsigned) != 0)
				p.ReadVarInt32();
			else
				p.ReadSignedVarInt32();
		}
		else {
			p.SeekRelative(prop.Bits);
		}
	}
	#endregion
	#region SendPropType.Float
	public static void Float_SkipProp(SendProp prop, bf_read p) { 
		if ((prop.GetFlags() & PropFlags.Coord) != 0) {
			uint val = p.ReadUBitLong(2);
			if (val != 0) {
				int seekDist = 1;

				if ((val & 1) != 0)
					seekDist += (int)BitBuffer.COORD_INTEGER_BITS;
				if ((val & 2) != 0)
					seekDist += (int)BitBuffer.COORD_FRACTIONAL_BITS;

				p.SeekRelative(seekDist);
			}
		}
		else if ((prop.GetFlags() & PropFlags.CoordMP) != 0)
			p.ReadBitCoordMP(false, false);
		else if ((prop.GetFlags() & PropFlags.CoordMPLowPrecision) != 0)
			p.ReadBitCoordMP(false, true);
		else if ((prop.GetFlags() & PropFlags.CoordMPIntegral) != 0)
			p.ReadBitCoordMP(true, false);
		else if ((prop.GetFlags() & PropFlags.NoScale) != 0)
			p.SeekRelative(32);
		else if ((prop.GetFlags() & PropFlags.Normal) != 0)
			p.SeekRelative(BitBuffer.NORMAL_FRACTIONAL_BITS + 1);
		else
			p.SeekRelative(prop.Bits);
	}
	public static void Float_Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}
	public static void Float_Decode(ref DecodeInfo decodeInfo) {
		decodeInfo.RecvProxyData.Value.Float = DecodeFloat(decodeInfo.Prop, decodeInfo.In);

		if (decodeInfo.RecvProxyData.RecvProp != null)
			decodeInfo.RecvProxyData.RecvProp.GetProxyFn()(in decodeInfo.RecvProxyData, decodeInfo.Object, decodeInfo.FieldInfo);
	}
	public static bool Float_IsZero(object instance, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}
	public static void Float_DecodeZero(ref DecodeInfo info) {
		throw new NotImplementedException();
	}
	public static ReadOnlySpan<char> Float_GetTypeNameString() {
		throw new NotImplementedException();
	}
	public static int Float_CompareDeltas(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}
	public static bool Float_IsEncodedZero(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
	#endregion
	#region SendPropType.Vector
	public static int Vector_CompareDeltas(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}
	public static void Vector_Decode(ref DecodeInfo decodeInfo) {
		Vector3 vec = DecodeVector(decodeInfo.Prop, decodeInfo.In);
		decodeInfo.RecvProxyData.Value.Vector = vec;
		decodeInfo.RecvProxyData.RecvProp?.GetProxyFn()(ref decodeInfo.RecvProxyData, decodeInfo.Object, decodeInfo.FieldInfo);
	}
	public static void Vector_DecodeZero(ref DecodeInfo info) {
		throw new NotImplementedException();
	}
	public static void Vector_Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}
	public static ReadOnlySpan<char> Vector_GetTypeNameString() {
		throw new NotImplementedException();
	}
	public static bool Vector_IsEncodedZero(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
	public static bool Vector_IsZero(object instance, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}
	public static void Vector_SkipProp(SendProp prop, bf_read p) {
		Float_SkipProp(prop, p);
		Float_SkipProp(prop, p);
		if ((prop.GetFlags() & PropFlags.Normal) != 0)
			p.SeekRelative(1);
		else
			Float_SkipProp(prop, p);
	}
	#endregion
	#region SendPropType.VectorXY
	public static int VectorXY_CompareDeltas(SendProp prop, bf_read p1, bf_read p2) => throw new NotImplementedException();
	public static void VectorXY_Decode(ref DecodeInfo decodeInfo) => throw new NotImplementedException();
	public static void VectorXY_DecodeZero(ref DecodeInfo info) => throw new NotImplementedException();
	public static void VectorXY_Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) => throw new NotImplementedException();
	public static ReadOnlySpan<char> VectorXY_GetTypeNameString() => throw new NotImplementedException();
	public static bool VectorXY_IsEncodedZero(SendProp prop, bf_read p) => throw new NotImplementedException();
	public static bool VectorXY_IsZero(object instance, ref DVariant var, SendProp prop) => throw new NotImplementedException();
	public static void VectorXY_SkipProp(SendProp prop, bf_read p) => throw new NotImplementedException();
	#endregion
	#region SendPropType.String
	public static int String_CompareDeltas(SendProp prop, bf_read p1, bf_read p2) => throw new NotImplementedException();
	public static void String_Decode(ref DecodeInfo decodeInfo) => throw new NotImplementedException();
	public static void String_DecodeZero(ref DecodeInfo info) => throw new NotImplementedException();
	public static void String_Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) => throw new NotImplementedException();
	public static ReadOnlySpan<char> String_GetTypeNameString() => throw new NotImplementedException();
	public static bool String_IsEncodedZero(SendProp prop, bf_read p) => throw new NotImplementedException();
	public static bool String_IsZero(object instance, ref DVariant var, SendProp prop) => throw new NotImplementedException();
	public static void String_SkipProp(SendProp prop, bf_read p) {
		int len = (int)p.ReadUBitLong(Constants.DT_MAX_STRING_BITS);
		p.SeekRelative(len * 8);
	}
	#endregion
	#region SendPropType.Array
	public static int Array_CompareDeltas(SendProp prop, bf_read p1, bf_read p2) => throw new NotImplementedException();
	public static void Array_Decode(ref DecodeInfo decodeInfo) => throw new NotImplementedException();
	public static void Array_DecodeZero(ref DecodeInfo info) => throw new NotImplementedException();
	public static void Array_Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) => throw new NotImplementedException();
	public static ReadOnlySpan<char> Array_GetTypeNameString() => throw new NotImplementedException();
	public static bool Array_IsEncodedZero(SendProp prop, bf_read p) => throw new NotImplementedException();
	public static bool Array_IsZero(object instance, ref DVariant var, SendProp prop) => throw new NotImplementedException();
	public static void Array_SkipProp(SendProp prop, bf_read p) => throw new NotImplementedException();
	#endregion
	#region SendPropType.DataTable
	public static int DataTable_CompareDeltas(SendProp prop, bf_read p1, bf_read p2) => throw new NotImplementedException();
	public static void DataTable_Decode(ref DecodeInfo decodeInfo) => throw new NotImplementedException();
	public static void DataTable_DecodeZero(ref DecodeInfo info) => throw new NotImplementedException();
	public static void DataTable_Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) => throw new NotImplementedException();
	public static ReadOnlySpan<char> DataTable_GetTypeNameString() => throw new NotImplementedException();
	public static bool DataTable_IsEncodedZero(SendProp prop, bf_read p) => throw new NotImplementedException();
	public static bool DataTable_IsZero(object instance, ref DVariant var, SendProp prop) => throw new NotImplementedException();
	public static void DataTable_SkipProp(SendProp prop, bf_read p) => throw new NotImplementedException();
	#endregion
	#region SendPropType.GModTable
	public static int GModTable_CompareDeltas(SendProp prop, bf_read p1, bf_read p2) => throw new NotImplementedException();
	public static void GModTable_Decode(ref DecodeInfo decodeInfo) => throw new NotImplementedException();
	public static void GModTable_DecodeZero(ref DecodeInfo info) => throw new NotImplementedException();
	public static void GModTable_Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) => throw new NotImplementedException();
	public static ReadOnlySpan<char> GModTable_GetTypeNameString() => throw new NotImplementedException();
	public static bool GModTable_IsEncodedZero(SendProp prop, bf_read p) => throw new NotImplementedException();
	public static bool GModTable_IsZero(object instance, ref DVariant var, SendProp prop) => throw new NotImplementedException();
	public static void GModTable_SkipProp(SendProp prop, bf_read p) => throw new NotImplementedException();
	#endregion
}
