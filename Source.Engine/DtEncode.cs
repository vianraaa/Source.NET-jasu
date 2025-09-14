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

public delegate void EncodeFn<Instance, Return>(GetRefFn<Instance, Return> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) where Instance : class;
public delegate void DecodeFn<Instance, Return>(ref DecodeInfo<Instance, Return> decodeInfo) where Instance : class;
public delegate int CompareDeltasFn(SendProp prop, bf_read p1, bf_read p2);
public delegate void FastCopyFn<Instance, Return>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, Return> sendField, GetRefFn<Instance, Return> recvField) where Instance : class;
public delegate ReadOnlySpan<char> GetTypeNameStringFn();
public delegate bool IsZeroFn<Instance, Return>(GetRefFn<Instance, Return> fn, ref DVariant var, SendProp prop) where Instance : class;
public delegate bool DecodeZeroFn<Instance, Return>(ref DecodeInfo<Instance, Return> info) where Instance : class;
public delegate bool IsEncodedZeroFn(SendProp prop, bf_read p);
public delegate bool SkipPropFn(SendProp prop, bf_read p);

public abstract class BasePropTypeFns<Instance> where Instance : class
{
	public static void Int_Encode<Return>(GetRefFn<Instance, Return> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
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
	public static void Int_Decode<Return>(ref DecodeInfo<Instance, Return> info) {
		SendProp prop = info.Prop;
		PropFlags flags = prop.GetFlags();

		if ((flags & PropFlags.VarInt) != 0) {
			if ((flags & PropFlags.Unsigned) != 0)
				info.RecvProxyData.Value.Int = (int)info.In.ReadVarInt32();
			else
				info.RecvProxyData.Value.Int = info.In.ReadSignedVarInt32();
		}
		else {
			int bits = prop.Bits;
			info.RecvProxyData.Value.Int = (int)info.In.ReadUBitLong(bits);

			if (bits != 32 && (flags & PropFlags.Unsigned) == 0) {
				uint highbit = (uint)(1 << (prop.Bits - 1));
				if ((info.RecvProxyData.Value.Int & highbit) != 0) {
					info.RecvProxyData.Value.Int -= (int)highbit;
					info.RecvProxyData.Value.Int -= (int)highbit;
				}
			}
		}

		info.RecvProxyData.RecvProp?.GetProxyFn<Instance, Return>()?.Invoke(ref info.RecvProxyData, info.Getter);
	}
	public static int Int_CompareDeltas(SendProp prop, bf_read p1, bf_read p2) {
		if ((prop.GetFlags() & PropFlags.VarInt) != 0) {
			if ((prop.GetFlags() & PropFlags.Unsigned) != 0)
				return (p1.ReadVarInt32() != p2.ReadVarInt32()) ? 1 : 0;

			return (p1.ReadSignedVarInt32() != p2.ReadSignedVarInt32()) ? 1 : 0;
		}

		return p1.CompareBits(p2, prop.Bits) ? 1 : 0;
	}
	public static void Int_FastCopy<Return>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, Return> sendField, GetRefFn<Instance, Return> recvField) => throw new NotImplementedException();
	public static ReadOnlySpan<char> Int_GetTypeNameString() => throw new NotImplementedException();
	public static bool Int_IsZero<Return>(GetRefFn<Instance, Return> fn, ref DVariant var, SendProp prop) 
		=> 
	public static bool Int_DecodeZero<Return>(ref DecodeInfo<Instance, Return> info) => throw new NotImplementedException();
	public static bool Int_IsEncodedZero(SendProp prop, bf_read p) => throw new NotImplementedException();
	public static bool Int_SkipProp(SendProp prop, bf_read p) => throw new NotImplementedException();

	public static readonly BasePropTypeFns<Instance>[] Globals = [
		new PropTypeFns<Instance, int>(Int_Encode, Int_Decode, Int_CompareDeltas, null, Int_GetTypeNameString, Int_IsZero, Int_DecodeZero, Int_IsEncodedZero, Int_SkipProp),
	];

	public static BasePropTypeFns<Instance> Get(SendPropType type) => Globals[(int)type];
	public static PropTypeFns<Instance, Return> Get<Instance, Return>(SendPropType type) where Instance : class
		=> Globals[(int)type] is PropTypeFns<Instance, Return> retFns ? retFns : throw new Exception("Bad generic type for send prop!");
}
public class PropTypeFns<Instance, Return> : BasePropTypeFns<Instance> where Instance : class
{
	public EncodeFn<Instance, Return> Encode;
	public DecodeFn<Instance, Return> Decode;
	public CompareDeltasFn CompareDeltas;
	public FastCopyFn<Instance, Return> FastCopy;
	public GetTypeNameStringFn? GetTypeNameString;
	public IsZeroFn<Instance, Return> IsZero;
	public DecodeZeroFn<Instance, Return> DecodeZero;
	public IsEncodedZeroFn IsEncodedZero;
	public SkipPropFn SkipProp;
	public PropTypeFns(EncodeFn<Instance, Return> encode,
		DecodeFn<Instance, Return> decode, CompareDeltasFn compareDeltas,
		FastCopyFn<Instance, Return> fastCopy, GetTypeNameStringFn? getTypeNameString,
		IsZeroFn<Instance, Return> isZero, DecodeZeroFn<Instance, Return> decodeZero,
		IsEncodedZeroFn isEncodedZero, SkipPropFn skipProp) {
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
}
