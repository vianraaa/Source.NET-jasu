using Source.Common.Mathematics;

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
	public float X {
		get => UsingReferenceData ? default : MemoryMarshal.Cast<byte, Vector3>(ValueData)[0].X;
		set {
			MemoryMarshal.Cast<byte, Vector3>(ValueData)[0].X = value;
			UsingReferenceData = false;
			ReferenceData = null;
		}
	}
	public float Y {
		get => UsingReferenceData ? default : MemoryMarshal.Cast<byte, Vector3>(ValueData)[0].Y;
		set {
			MemoryMarshal.Cast<byte, Vector3>(ValueData)[0].Y = value;
			UsingReferenceData = false;
			ReferenceData = null;
		}
	}
	public float Z {
		get => UsingReferenceData ? default : MemoryMarshal.Cast<byte, Vector3>(ValueData)[0].Z;
		set {
			MemoryMarshal.Cast<byte, Vector3>(ValueData)[0].Z = value;
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

public struct ExcludeProp
{
	public string? TableName;
	public string? PropName;
}

public struct BuildHierarchyStruct
{
	public ExcludeProp[]? ExcludeProps;

	public InlineArrayMaxTotalSendTableProps<SendProp> DataTableProps;
	public int NumDataTableProps;

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
	void SetDataTable<PropType>(IDataTableBase<PropType>? dt) where PropType : IDataTableProp;
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
	public static readonly Dictionary<Type, int> FieldSizes = new() {
		{ typeof(sbyte), 1 }, { typeof(byte), 1 }, { typeof(bool), 1 },
		{ typeof(short), 2 }, { typeof(ushort), 2 },
		{ typeof(int), 4 }, { typeof(uint), 4 }, { typeof(float), 4 },
		{ typeof(QAngle), 12 },
		{ typeof(Vector3), 12 },
	};
	/// <summary>
	/// This is to be called on SendTables and RecvTables to setup array properties to point at their property 
	/// templates and to set the PropFlags.InsideArray flag on the properties inside arrays.
	/// </summary>
	/// <typeparam name="PropType"></typeparam>
	/// <param name="table"></param>
	public static void SetupArrayProps_R<PropType>(this IDataTableBase<PropType> table) where PropType : IDataTableProp {
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
				SetupArrayProps_R(prop.GetDataTable<PropType>()!);
		}
	}

	public static void SetDataTableProxyIndices_R(SendTablePrecalc mainTable, SendNode curTable, ref BuildHierarchyStruct bhs) {
		for (int i = 0; i < curTable.GetNumChildren(); i++) {
			SendNode node = curTable.GetChild(i);
			SendProp prop = bhs.DataTableProps[node.DataTableProp];

			if ((prop.GetFlags() & PropFlags.ProxyAlwaysYes) != 0) {
				node.SetDataTableProxyIndex(Constants.DATATABLE_PROXY_INDEX_NOPROXY);
			}
			else {
				node.SetDataTableProxyIndex((ushort)mainTable.GetNumDataTableProxies());
				mainTable.SetNumDataTableProxies(mainTable.GetNumDataTableProxies() + 1);
			}

			SetDataTableProxyIndices_R(mainTable, node, ref bhs);
		}
	}

	public static void SetRecursiveProxyIndices_R(SendTable baseTable, SendNode curTable, ref int curProxyIndex) {
		const int MAX_PROXY_RESULTS = 256;

		if (curProxyIndex >= MAX_PROXY_RESULTS)
			Error($"Too many proxies for datatable {baseTable.GetName()}.");

		curTable.SetRecursiveProxyIndex((ushort)curProxyIndex);
		curProxyIndex++;

		for (int i = 0; i < curTable.GetNumChildren(); i++) {
			SendNode pNode = curTable.GetChild(i);
			SetRecursiveProxyIndices_R(baseTable, pNode, ref curProxyIndex);
		}
	}
}

public class RenamedRecvTableInfo {
	public static RenamedRecvTableInfo? Head;
	public string OldName;
	public string NewName;
	public RenamedRecvTableInfo? Next;

	public RenamedRecvTableInfo(string oldName, string newName) {
		OldName = oldName;
		NewName = newName;

		Next = Head;
		Head = this;
	}
}