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

/// <summary>
/// Non-generic prop type functions, this is the root of all evil
/// </summary>
public abstract class BasePropTypeFns
{
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

	public readonly static BasePropTypeFns[] PropTypeFns = [
			new IntPropTypeFns(),
		new FloatPropTypeFns(),
		new VectorPropTypeFns(),
		new VectorXYPropTypeFns(),
		new StringPropTypeFns(),
		new ArrayPropTypeFns(),
		new DataTablePropTypeFns(),
	];

	public static BasePropTypeFns Get(SendPropType propType) => PropTypeFns[(int)propType];
	public static PropTypeFns<T> Get<T>(SendPropType propType) => PropTypeFns[(int)propType] is PropTypeFns<T> ptfnsT ? ptfnsT : throw new InvalidCastException("Bad type cast");

	public abstract ReadOnlySpan<char> GetTypeNameString();
	public abstract int CompareDeltas(SendProp prop, bf_read p1, bf_read p2);
	public abstract bool IsEncodedZero(SendProp prop, bf_read p);
	public abstract void SkipProp(SendProp prop, bf_read p);
	public abstract void Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID);
	public abstract void Decode(ref DecodeInfo decodeInfo);
	public abstract bool IsZero(object instance, ref DVariant var, SendProp prop);
	public abstract void DecodeZero(ref DecodeInfo info);
}

/// <summary>
/// Generic abstract class which inherits <see cref="BasePropTypeFns"/>, using <typeparamref name="Return"/> to specify the field type
/// it intends to modify. To avoid the Instance generic creeping into <see cref="BasePropTypeFns"/>, the Instance generic is instead placed in the 
/// individual abstract methods. The Instance generic must be a class type and is passed into <see cref="DecodeInfo{Instance, Return}"/>
/// and <see cref="GetRefFn{InstanceType, ReturnType}"/>'s InstanceType generic parameters, which is only really used to say "cast this <see cref="object"/>
/// to InstanceType" in those functions (ie. pass around object references for the most part, then cast where applicable for the sake of field ref grabbing, etc).
/// Inheritors of this class specify their <typeparamref name="Return"/> type, which allows them to encode/decode that specific type.
/// </summary>
/// <typeparam name="Return"></typeparam>
public abstract class PropTypeFns<Return> : BasePropTypeFns;

public class IntPropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltas(SendProp prop, bf_read p1, bf_read p2) {
		if ((prop.GetFlags() & PropFlags.VarInt) != 0) {
			if ((prop.GetFlags() & PropFlags.Unsigned) != 0)
				return (p1.ReadVarInt32() != p2.ReadVarInt32()) ? 1 : 0;

			return (p1.ReadSignedVarInt32() != p2.ReadSignedVarInt32()) ? 1 : 0;
		}

		return p1.CompareBits(p2, prop.Bits) ? 1 : 0;
	}

	public override void Decode(ref DecodeInfo decodeInfo) {
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

	public override void DecodeZero(ref DecodeInfo info) {
		info.RecvProxyData.Value.Int = 0;

		info.RecvProxyData.RecvProp?.GetProxyFn()(ref info.RecvProxyData, info.Object, info.FieldInfo);
	}

	public override void Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
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

	public override ReadOnlySpan<char> GetTypeNameString() => "DPT_Int";

	public override bool IsEncodedZero(SendProp prop, bf_read p) {
		if ((prop.GetFlags() & PropFlags.VarInt) != 0) {
			if ((prop.GetFlags() & PropFlags.Unsigned) != 0)
				return p.ReadVarInt32() == 0;

			return p.ReadSignedVarInt32() == 0;
		}

		return p.ReadUBitLong(prop.Bits) == 0;
	}
	public override bool IsZero(object instance, ref DVariant var, SendProp prop) {
		return var.Int == 0;
	}

	public override void SkipProp(SendProp prop, bf_read p) {
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
}
public class FloatPropTypeFns : PropTypeFns<int>
{
	public override void SkipProp(SendProp prop, bf_read p) => _SkipProp(prop, p);

	public static void _SkipProp(SendProp prop, bf_read inBuffer) {
		if ((prop.GetFlags() & PropFlags.Coord) != 0) {
			uint val = inBuffer.ReadUBitLong(2);
			if (val != 0) {
				int seekDist = 1;

				if ((val & 1) != 0)
					seekDist += (int)BitBuffer.COORD_INTEGER_BITS;
				if ((val & 2) != 0)
					seekDist += (int)BitBuffer.COORD_FRACTIONAL_BITS;

				inBuffer.SeekRelative(seekDist);
			}
		}
		else if ((prop.GetFlags() & PropFlags.CoordMP) != 0)
			inBuffer.ReadBitCoordMP(false, false);
		else if ((prop.GetFlags() & PropFlags.CoordMPLowPrecision) != 0)
			inBuffer.ReadBitCoordMP(false, true);
		else if ((prop.GetFlags() & PropFlags.CoordMPIntegral) != 0)
			inBuffer.ReadBitCoordMP(true, false);
		else if ((prop.GetFlags() & PropFlags.NoScale) != 0)
			inBuffer.SeekRelative(32);
		else if( (prop.GetFlags() & PropFlags.Normal) != 0 )
			inBuffer.SeekRelative(BitBuffer.NORMAL_FRACTIONAL_BITS + 1);
		else 
			inBuffer.SeekRelative(prop.Bits);
	}

	public override void Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override void Decode(ref DecodeInfo decodeInfo) {
		decodeInfo.RecvProxyData.Value.Float = DecodeFloat(decodeInfo.Prop, decodeInfo.In);

		if (decodeInfo.RecvProxyData.RecvProp != null)
			decodeInfo.RecvProxyData.RecvProp.GetProxyFn()(in decodeInfo.RecvProxyData, decodeInfo.Object, decodeInfo.FieldInfo);
	}

	public override bool IsZero(object instance, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override void DecodeZero(ref DecodeInfo info) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameString() {
		throw new NotImplementedException();
	}

	public override int CompareDeltas(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZero(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}

public class VectorPropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltas(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void Decode(ref DecodeInfo decodeInfo) {
		Vector3 vec = DecodeVector(decodeInfo.Prop, decodeInfo.In);
		decodeInfo.RecvProxyData.Value.Vector = vec;
		decodeInfo.RecvProxyData.RecvProp?.GetProxyFn()(ref decodeInfo.RecvProxyData, decodeInfo.Object, decodeInfo.FieldInfo);
	}

	public override void DecodeZero(ref DecodeInfo info) {
		throw new NotImplementedException();
	}

	public override void Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameString() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZero(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZero(object instance, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override void SkipProp(SendProp prop, bf_read p) {
		FloatPropTypeFns._SkipProp(prop, p);
		FloatPropTypeFns._SkipProp(prop, p);
		if ((prop.GetFlags() & PropFlags.Normal) != 0)
			p.SeekRelative(1);
		else
			FloatPropTypeFns._SkipProp(prop, p);
	}
}

public class VectorXYPropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltas(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void Decode(ref DecodeInfo decodeInfo) {
		throw new NotImplementedException();
	}

	public override void DecodeZero(ref DecodeInfo info) {
		throw new NotImplementedException();
	}

	public override void Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameString() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZero(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZero(object instance, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override void SkipProp(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}

public class StringPropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltas(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void Decode(ref DecodeInfo decodeInfo) {
		throw new NotImplementedException();
	}

	public override void DecodeZero(ref DecodeInfo info) {
		throw new NotImplementedException();
	}

	public override void Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameString() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZero(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZero(object instance, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override void SkipProp(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}

public class ArrayPropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltas(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void Decode(ref DecodeInfo decodeInfo) {
		throw new NotImplementedException();
	}

	public override void DecodeZero(ref DecodeInfo info) {
		throw new NotImplementedException();
	}

	public override void Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameString() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZero(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZero(object instance, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override void SkipProp(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}

public class DataTablePropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltas(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void Decode(ref DecodeInfo decodeInfo) {
		throw new NotImplementedException();
	}

	public override void DecodeZero(ref DecodeInfo info) {
		throw new NotImplementedException();
	}

	public override void Encode(object instance, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameString() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZero(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZero(object instance, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override void SkipProp(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}
