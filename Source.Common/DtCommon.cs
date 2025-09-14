using System.Numerics;
using System.Runtime.InteropServices;

namespace Source.Common;


public delegate ref ReturnType GetRefFn<InstanceType, ReturnType>(InstanceType type) where InstanceType : class;
public delegate Span<ReturnType> GetSpanFn<InstanceType, ReturnType>(InstanceType type) where InstanceType : class;


public enum SendPropType {
	Int,
	Float,
	Vector,
	VectorXY,
	String,
	Array,
	DataTable,
	Max
}

[StructLayout(LayoutKind.Explicit)]
public struct DVariant
{
	[FieldOffset(0)] public float Float;
	[FieldOffset(0)] public int Int;
	[FieldOffset(0)] public Vector3 Vector;
	[FieldOffset(0)] public string? String;
	[FieldOffset(0)] public object? Data;
	[FieldOffset(1)] public SendPropType Type;
}

public struct RecvProxyData
{
	public RecvProp RecvProp;
	public DVariant Value;
	public int Element;
	public int ObjectID;
}

public enum PropFlags
{
	Unsigned = 1 << 0,
	Coord = 1 << 1,
	NoScale = 1 << 2,
	RoundDown = 1 << 3,
	RoundUp = 1 << 4,
	Normal = 1 << 5,
	Exclude = 1 << 6,
	XYZExponent = 1 << 7,
	InsideArray = 1 << 8,
	ProxyAlwaysYes = 1 << 9,
	ChangesOften = 1 << 10,
	IsAVectorElem = 1 << 11,
	Collapsible = 1 << 12,
	CoordMP = 1 << 13,
	CoordMPLowPrecision = 1 << 14,
	CoordMPIntegral = 1 << 15,
	VarInt = Normal,
	NumFlagBitsNetworked = 16,
	EncodedAgainstTickCount = 1 << 16,
	NumFlagBits = 17,
}