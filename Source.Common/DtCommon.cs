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

public struct DVariant
{
	// Enough space to fit an int (4 bytes), a float (4 bytes), a Vector3 (12 bytes)
	InlineArray16<byte> ValueData;
	object? ReferenceData;
	bool UsingReferenceData;

	public float Float {
		get => UsingReferenceData ? default : MemoryMarshal.Cast<byte, float>(ValueData)[0];
		set {
			MemoryMarshal.Cast<byte, float>(ValueData)[0] = value;
			UsingReferenceData = false;
			ReferenceData = null;
		}
	}
	public int Int {
		get => UsingReferenceData ? default : MemoryMarshal.Cast<byte, int>(ValueData)[0];
		set {
			MemoryMarshal.Cast<byte, int>(ValueData)[0] = value;
			UsingReferenceData = false;
			ReferenceData = null;
		}
	}
	public Vector3 Vector {
		get => UsingReferenceData ? default : MemoryMarshal.Cast<byte, Vector3>(ValueData)[0];
		set {
			MemoryMarshal.Cast<byte, Vector3>(ValueData)[0] = value;
			UsingReferenceData = false;
			ReferenceData = null;
		}
	}
	public string? String {
		get => UsingReferenceData ? ReferenceData is string str ? str : null : null;
		set {
			UsingReferenceData = true;
			ReferenceData = value;
		}
	}
	public object? Data {
		get => UsingReferenceData ? ReferenceData : null;
		set {
			UsingReferenceData = true;
			ReferenceData = value;
		}
	}

	public SendPropType Type;
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