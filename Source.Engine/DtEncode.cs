using Source.Common;
using Source.Common.Bitbuffers;
using Source.Common.Commands;

using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Source.Engine;

public struct DecodeInfo<Instance, Return> where Instance : class
{
	public RecvProxyData RecvProxyData;
	public GetRefFn<Instance, Return> Getter;
	public SendProp Prop;
	public bf_read In;
}


/// <summary>
/// Non-generic prop type functions, this is the root of all evil
/// </summary>
public abstract class BasePropTypeFns
{
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
public abstract class PropTypeFns<Return> : BasePropTypeFns
{
	public abstract void Encode<Instance>(GetRefFn<Instance, Return> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) where Instance : class;
	public abstract void Decode<Instance>(ref DecodeInfo<Instance, Return> decodeInfo) where Instance : class;
	
	public virtual void FastCopy<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, Return> sendField, GetRefFn<Instance, Return> recvField) where Instance : class {
		throw new NotImplementedException("FastCopy not implemented yet, it seems to only be used in the singleplayer engine flow, which is currently not implemented.");
		// ^^ should be a Generic_FastCopy equiv. at some point
	}

	public abstract bool IsZero<Instance>(GetRefFn<Instance, Return> fn, ref DVariant var, SendProp prop) where Instance : class;
	public abstract void DecodeZero<Instance>(ref DecodeInfo<Instance, Return> info) where Instance : class;
}

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

	public override void Decode<Instance>(ref DecodeInfo<Instance, int> decodeInfo) {
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

		decodeInfo.RecvProxyData.RecvProp?.GetProxyFn<Instance, int>()?.Invoke(ref decodeInfo.RecvProxyData, decodeInfo.Getter);
	}

	public override void DecodeZero<Instance>(ref DecodeInfo<Instance, int> info) {
		info.RecvProxyData.Value.Int = 0;

		info.RecvProxyData.RecvProp?.GetProxyFn<Instance, int>()(ref info.RecvProxyData, info.Getter);
	}

	public override void Encode<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
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
	public override bool IsZero<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop) {
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
	public override int CompareDeltas(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void Decode<Instance>(ref DecodeInfo<Instance, int> decodeInfo) {
		throw new NotImplementedException();
	}

	public override void DecodeZero<Instance>(ref DecodeInfo<Instance, int> info) {
		throw new NotImplementedException();
	}

	public override void Encode<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override void FastCopy<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, int> sendField, GetRefFn<Instance, int> recvField) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameString() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZero(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZero<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override void SkipProp(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}

public class VectorPropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltas(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void Decode<Instance>(ref DecodeInfo<Instance, int> decodeInfo) {
		throw new NotImplementedException();
	}

	public override void DecodeZero<Instance>(ref DecodeInfo<Instance, int> info) {
		throw new NotImplementedException();
	}

	public override void Encode<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override void FastCopy<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, int> sendField, GetRefFn<Instance, int> recvField) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameString() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZero(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZero<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override void SkipProp(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}

public class VectorXYPropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltas(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void Decode<Instance>(ref DecodeInfo<Instance, int> decodeInfo) {
		throw new NotImplementedException();
	}

	public override void DecodeZero<Instance>(ref DecodeInfo<Instance, int> info) {
		throw new NotImplementedException();
	}

	public override void Encode<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override void FastCopy<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, int> sendField, GetRefFn<Instance, int> recvField) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameString() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZero(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZero<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop) {
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

	public override void Decode<Instance>(ref DecodeInfo<Instance, int> decodeInfo) {
		throw new NotImplementedException();
	}

	public override void DecodeZero<Instance>(ref DecodeInfo<Instance, int> info) {
		throw new NotImplementedException();
	}

	public override void Encode<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override void FastCopy<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, int> sendField, GetRefFn<Instance, int> recvField) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameString() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZero(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZero<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop) {
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

	public override void Decode<Instance>(ref DecodeInfo<Instance, int> decodeInfo) {
		throw new NotImplementedException();
	}

	public override void DecodeZero<Instance>(ref DecodeInfo<Instance, int> info) {
		throw new NotImplementedException();
	}

	public override void Encode<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override void FastCopy<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, int> sendField, GetRefFn<Instance, int> recvField) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameString() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZero(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZero<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop) {
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

	public override void Decode<Instance>(ref DecodeInfo<Instance, int> decodeInfo) {
		throw new NotImplementedException();
	}

	public override void DecodeZero<Instance>(ref DecodeInfo<Instance, int> info) {
		throw new NotImplementedException();
	}

	public override void Encode<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override void FastCopy<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, int> sendField, GetRefFn<Instance, int> recvField) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameString() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZero(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZero<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override void SkipProp(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}
