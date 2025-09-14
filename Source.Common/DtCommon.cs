using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Source.Common;


public delegate ref ReturnType GetRefFn<InstanceType, ReturnType>(InstanceType type) where InstanceType : class;
public delegate Span<ReturnType> GetSpanFn<InstanceType, ReturnType>(InstanceType type) where InstanceType : class;


public enum SendPropType
{
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

public struct ExcludeProp
{
	public string? TableName;
	public string? PropName;
}

public struct BuildHierarchyStruct
{
	public ExcludeProp[]? ExcludeProps;

	public InlineArrayMaxTotalSendTableProps<SendProp> DatatableProps;
	public int NumDatatableProps;

	public InlineArrayMaxTotalSendTableProps<SendProp> Props;
	public InlineArrayMaxTotalSendTableProps<byte> PropProxyIndices;
	public int NumProps;

	public byte PropProxies;
}

public interface IDataTableProp
{
	ReadOnlySpan<char> GetName();

	bool IsSigned();
	bool IsExcludeProp();
	bool IsInsideArray();
	void SetInsideArray();
	bool SetArrayProp<PropType>(PropType propType) where PropType : IDataTableProp;
	PropType GetArrayProp<PropType>() where PropType : IDataTableProp;
	int GetNumElements();
	int SetNumElements(int elements);
	SendPropType GetPropType();
	PropFlags GetFlags();
	void SetFlags(PropFlags flags);
	IDataTableBase<PropType>? GetDataTable<PropType>() where PropType : IDataTableProp;
}

public interface IDataTableBase<PropType> where PropType : IDataTableProp
{
	int GetNumProps();
	bool IsInitialized();
	void SetInitialized(bool v);
	PropType GetProp(int index);

}

public static class DataTableHelpers
{
	/// <summary>
	/// This is to be called on SendTables and RecvTables to setup array properties to point at their property 
	/// templates and to set the PropFlags.InsideArray flag on the properties inside arrays.
	/// </summary>
	/// <typeparam name="PropType"></typeparam>
	/// <param name="table"></param>
	public static void SetupArrayProps<PropType>(this IDataTableBase<PropType> table) where PropType : IDataTableProp {
		if (table.IsInitialized())
			return;

		table.SetInitialized(true);

		for (int i = 0; i < table.GetNumProps(); i++) {
			PropType prop = table.GetProp(i);

			if (prop.GetPropType() == SendPropType.Array) {
				ErrorIfNot(i >= 1, $"SetupArrayProps_R: array prop '{prop.GetName()}' is at index zero.");

				PropType arrayProp = table.GetProp(i - 1);
				arrayProp.SetInsideArray();
				prop.SetArrayProp(arrayProp);
			}
			else if (prop.GetPropType() == SendPropType.DataTable)
				SetupArrayProps(prop.GetDataTable<PropType>());
		}
	}

	public static void SetDataTableProxyIndices_R(SendTablePrecalc sendTablePrecalc, SendNode sendNode, ref BuildHierarchyStruct bhs) {
		throw new NotImplementedException();
	}

	public static void SetRecursiveProxyIndices_R(SendTable? table, SendNode sendNode, ref int proxyIndices) {
		throw new NotImplementedException();
	}
}