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


public class BasePropTypeFns
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
}

public abstract class PropTypeFns<Return> : BasePropTypeFns
{
	public abstract void EncodeFn<Instance>(GetRefFn<Instance, Return> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) where Instance : class;
	public abstract void DecodeFn<Instance>(ref DecodeInfo<Instance, Return> decodeInfo) where Instance : class;
	public abstract int CompareDeltasFn(SendProp prop, bf_read p1, bf_read p2);
	public abstract void FastCopyFn<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, Return> sendField, GetRefFn<Instance, Return> recvField) where Instance : class;
	public abstract ReadOnlySpan<char> GetTypeNameStringFn();
	public abstract bool IsZeroFn<Instance>(GetRefFn<Instance, Return> fn, ref DVariant var, SendProp prop) where Instance : class;
	public abstract bool DecodeZeroFn<Instance>(ref DecodeInfo<Instance, Return> info) where Instance : class;
	public abstract bool IsEncodedZeroFn(SendProp prop, bf_read p);
	public abstract bool SkipPropFn(SendProp prop, bf_read p);
}

public class IntPropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltasFn(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void DecodeFn<Instance>(ref DecodeInfo<Instance, int> decodeInfo) {
		throw new NotImplementedException();
	}

	public override bool DecodeZeroFn<Instance>(ref DecodeInfo<Instance, int> info) {
		throw new NotImplementedException();
	}

	public override void EncodeFn<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override void FastCopyFn<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, int> sendField, GetRefFn<Instance, int> recvField) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameStringFn() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZeroFn(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZeroFn<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override bool SkipPropFn(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}
public class FloatPropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltasFn(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void DecodeFn<Instance>(ref DecodeInfo<Instance, int> decodeInfo) {
		throw new NotImplementedException();
	}

	public override bool DecodeZeroFn<Instance>(ref DecodeInfo<Instance, int> info) {
		throw new NotImplementedException();
	}

	public override void EncodeFn<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override void FastCopyFn<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, int> sendField, GetRefFn<Instance, int> recvField) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameStringFn() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZeroFn(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZeroFn<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override bool SkipPropFn(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}

public class VectorPropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltasFn(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void DecodeFn<Instance>(ref DecodeInfo<Instance, int> decodeInfo) {
		throw new NotImplementedException();
	}

	public override bool DecodeZeroFn<Instance>(ref DecodeInfo<Instance, int> info) {
		throw new NotImplementedException();
	}

	public override void EncodeFn<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override void FastCopyFn<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, int> sendField, GetRefFn<Instance, int> recvField) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameStringFn() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZeroFn(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZeroFn<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override bool SkipPropFn(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}

public class VectorXYPropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltasFn(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void DecodeFn<Instance>(ref DecodeInfo<Instance, int> decodeInfo) {
		throw new NotImplementedException();
	}

	public override bool DecodeZeroFn<Instance>(ref DecodeInfo<Instance, int> info) {
		throw new NotImplementedException();
	}

	public override void EncodeFn<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override void FastCopyFn<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, int> sendField, GetRefFn<Instance, int> recvField) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameStringFn() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZeroFn(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZeroFn<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override bool SkipPropFn(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}

public class StringPropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltasFn(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void DecodeFn<Instance>(ref DecodeInfo<Instance, int> decodeInfo) {
		throw new NotImplementedException();
	}

	public override bool DecodeZeroFn<Instance>(ref DecodeInfo<Instance, int> info) {
		throw new NotImplementedException();
	}

	public override void EncodeFn<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override void FastCopyFn<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, int> sendField, GetRefFn<Instance, int> recvField) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameStringFn() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZeroFn(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZeroFn<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override bool SkipPropFn(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}

public class ArrayPropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltasFn(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void DecodeFn<Instance>(ref DecodeInfo<Instance, int> decodeInfo) {
		throw new NotImplementedException();
	}

	public override bool DecodeZeroFn<Instance>(ref DecodeInfo<Instance, int> info) {
		throw new NotImplementedException();
	}

	public override void EncodeFn<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override void FastCopyFn<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, int> sendField, GetRefFn<Instance, int> recvField) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameStringFn() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZeroFn(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZeroFn<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override bool SkipPropFn(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}

public class DataTablePropTypeFns : PropTypeFns<int>
{
	public override int CompareDeltasFn(SendProp prop, bf_read p1, bf_read p2) {
		throw new NotImplementedException();
	}

	public override void DecodeFn<Instance>(ref DecodeInfo<Instance, int> decodeInfo) {
		throw new NotImplementedException();
	}

	public override bool DecodeZeroFn<Instance>(ref DecodeInfo<Instance, int> info) {
		throw new NotImplementedException();
	}

	public override void EncodeFn<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop, bf_write writeOut, int objectID) {
		throw new NotImplementedException();
	}

	public override void FastCopyFn<Instance>(SendProp sendProp, RecvProp recvProp, GetRefFn<Instance, int> sendField, GetRefFn<Instance, int> recvField) {
		throw new NotImplementedException();
	}

	public override ReadOnlySpan<char> GetTypeNameStringFn() {
		throw new NotImplementedException();
	}

	public override bool IsEncodedZeroFn(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}

	public override bool IsZeroFn<Instance>(GetRefFn<Instance, int> fn, ref DVariant var, SendProp prop) {
		throw new NotImplementedException();
	}

	public override bool SkipPropFn(SendProp prop, bf_read p) {
		throw new NotImplementedException();
	}
}
