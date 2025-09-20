using SharpCompress.Common;

using Source;
using Source.Common;

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

using static Source.Common.Networking.svc_ClassInfo;

namespace Source.Common;

public static class RecvPropHelpers
{
	public static void RecvProxy_FloatToFloat(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		field.SetValueFast<float>(instance, data.Value.Float);
	}

	public static void RecvProxy_VectorToVector(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		field.SetValueFast<Vector3>(instance, data.Value.Vector);
	}

	public static void RecvProxy_VectorToVectorXY(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		ref Vector3 vec = ref field.GetValueRefFast<Vector3>(instance);
		vec.X = data.Value.X;
		vec.Y = data.Value.Y;
	}

	public static void DataTableRecvProxy_StaticDataTable(RecvProp prop, out object? outInstance, object? instance, FieldInfo fieldInfo, int objectID) {
		outInstance = instance;
	}

	public static void RecvProxy_Int32ToInt8(ref readonly RecvProxyData data, object instance, FieldInfo field) => field.SetValueFast(instance, unchecked((sbyte)data.Value.Int));
	public static void RecvProxy_Int32ToInt16(ref readonly RecvProxyData data, object instance, FieldInfo field) => field.SetValueFast(instance, unchecked((short)data.Value.Int));
	public static void RecvProxy_Int32ToInt32(ref readonly RecvProxyData data, object instance, FieldInfo field) => field.SetValueFast(instance, unchecked(data.Value.Int));
	public static void RecvProxy_StringToString(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		if (field.FieldType == typeof(string)) {
			field.SetValueFast(instance, data.Value.String);
			return;
		}

		throw new Exception("Cannot currently do a copy...");
	}

	public static void RecvProxy_IntToEHandle(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		BaseHandle ehandle = field.GetValueFast<BaseHandle>(instance);

		if (data.Value.Int == Constants.INVALID_NETWORKED_EHANDLE_VALUE) {
			ehandle.Index = (uint)Constants.INVALID_EHANDLE_INDEX;
		}
		else {
			int entity = data.Value.Int & ((1 << Constants.MAX_EDICT_BITS) - 1);
			int serialNum = data.Value.Int >> Constants.MAX_EDICT_BITS;

			ehandle.Init(entity, serialNum);
		}
	}
	public static RecvProp RecvPropEHandle(FieldInfo field, RecvVarProxyFn? proxyFn = null) {
		proxyFn ??= RecvProxy_IntToEHandle;
		return RecvPropInt(field, 0, proxyFn);
	}
	public static RecvProp RecvPropFloat(FieldInfo field, PropFlags flags = 0, RecvVarProxyFn? proxyFn = null) {
		RecvProp ret = new();
		proxyFn ??= RecvProxy_FloatToFloat;

		ret.FieldInfo = field;
		ret.RecvType = SendPropType.Float;
		ret.Flags = flags;
		ret.SetProxyFn(proxyFn);

		return ret;
	}
	public static RecvProp RecvPropQAngles(FieldInfo field, PropFlags flags = 0, RecvVarProxyFn? proxyFn = null)
		=> RecvPropVector(field, flags, proxyFn);
	public static RecvProp RecvPropVector(FieldInfo field, PropFlags flags = 0, RecvVarProxyFn? proxyFn = null) {
		RecvProp ret = new();
		proxyFn ??= RecvProxy_VectorToVector;

		ret.FieldInfo = field;
		ret.RecvType = SendPropType.Vector;
		ret.Flags = flags;
		ret.SetProxyFn(proxyFn);

		return ret;
	}
	public static RecvProp RecvPropVectorXY(FieldInfo field, PropFlags flags = 0, RecvVarProxyFn? proxyFn = null) {
		RecvProp ret = new();
		proxyFn ??= RecvProxy_VectorToVectorXY;

		ret.FieldInfo = field;
		ret.RecvType = SendPropType.VectorXY;
		ret.Flags = flags;
		ret.SetProxyFn(proxyFn);

		return ret;
	}
	public static RecvProp RecvPropInt(FieldInfo field, PropFlags flags = 0, RecvVarProxyFn? proxyFn = null) {
		RecvProp ret = new();

		int sizeOfVar = DataTableHelpers.FieldSizes.TryGetValue(field.FieldType, out int v) ? v : -1;
		if (proxyFn == null) {
			if (sizeOfVar == 1)
				proxyFn = RecvProxy_Int32ToInt8;
			else if (sizeOfVar == 2)
				proxyFn = RecvProxy_Int32ToInt16;
			else if (sizeOfVar == 4)
				proxyFn = RecvProxy_Int32ToInt32;
			else {
				AssertMsg(false, $"RecvPropInt var has invalid size {(sizeOfVar == -1 ? "UNDEFINED" : sizeOfVar)}");
				proxyFn = RecvProxy_Int32ToInt8;
			}
		}

		ret.FieldInfo = field;
		ret.RecvType = SendPropType.Int;
		ret.Flags = flags;
		ret.SetProxyFn(proxyFn);

		return ret;
	}
	public static RecvProp RecvPropString(FieldInfo field, int bufferSize = -1, PropFlags flags = 0, RecvVarProxyFn? proxyFn = null) {
		RecvProp ret = new();
		proxyFn ??= RecvProxy_StringToString;

		ret.FieldInfo = field;
		ret.RecvType = SendPropType.String;
		ret.Flags = flags;
		ret.StringBufferSize = bufferSize;
		ret.SetProxyFn(proxyFn);

		return ret;
	}
	public static RecvProp RecvPropDataTable(string name, FieldInfo field, RecvTable table, PropFlags flags = 0, DataTableRecvVarProxyFn? proxyFn = null) {
		RecvProp ret = new();
		proxyFn ??= DataTableRecvProxy_StaticDataTable;
		ret.NameOverride = name;
		ret.FieldInfo = field;
		ret.RecvType = SendPropType.DataTable;
		ret.Flags = flags;
		ret.SetDataTableProxyFn(proxyFn);
		ret.SetDataTable(table);
		return ret;
	}
	public static RecvProp RecvPropDataTable(string name, RecvTable table, PropFlags flags = 0, DataTableRecvVarProxyFn? proxyFn = null) {
		RecvProp ret = new();
		proxyFn ??= DataTableRecvProxy_StaticDataTable;
		ret.NameOverride = name;
		ret.RecvType = SendPropType.DataTable;
		ret.Flags = flags;
		ret.SetDataTableProxyFn(proxyFn);
		ret.SetDataTable(table);
		return ret;
	}

	public static DataTableRecvVarProxyFn RECV_GET_OBJECT_AT_FIELD(FieldInfo field) {
		return (RecvProp prop, out object? outInstance, object? instance, FieldInfo fieldInfo, int objectID) => {
			outInstance = field.GetValueFast<object>(instance);
		};
	}
}

/// <summary>
/// This is the base receive property abstract class. We differentiate from Source here by using 
/// generic-types that inherit from RecvProp for specific type access, where get/set goes through
/// overloaded abstract/virtual methods (replicating the RecvPropFloat/whatever calls that normally
/// get done via the implement clientclass etc. macros in Source). The way Source does it is with raw
/// pointer access, we semi-replicate that behavior in <see cref="GetRefFn{InstanceType, ReturnType}"/>
/// since it returns a by-reference var to the field on <c>InstanceType</c>.
/// </summary>
public class RecvProp : IDataTableProp
{
	public FieldInfo FieldInfo;
	public SendPropType RecvType;
	public PropFlags Flags;
	public int StringBufferSize;

	public RecvProp() {

	}
	public string? NameOverride;
	bool InsideArray;
	RecvProp? ArrayProp;
	RecvVarProxyFn ProxyFn;
	DataTableRecvVarProxyFn DataTableProxyFn;
	RecvTable? DataTable;
	int Offset;
	int Elements;

	public T GetValue<T>(object instance) => FieldAccess<T>.Getter(FieldInfo)(instance);
	public void SetValue<T>(object instance, in T value) => FieldAccess<T>.Setter(FieldInfo)(instance, value);

	public RecvVarProxyFn GetProxyFn() => ProxyFn;
	public void SetProxyFn(RecvVarProxyFn fn) => ProxyFn = fn;
	public DataTableRecvVarProxyFn GetDataTableProxyFn() => DataTableProxyFn;
	public void SetDataTableProxyFn(DataTableRecvVarProxyFn fn) => DataTableProxyFn = fn;

	public ReadOnlySpan<char> GetName() => NameOverride ?? FieldInfo.Name;

	public bool IsSigned() => (Flags & PropFlags.Unsigned) == 0;
	public bool IsExcludeProp() => (Flags & PropFlags.Exclude) != 0;
	public bool IsInsideArray() => InsideArray;
	public void SetInsideArray() => InsideArray = true;
	public bool SetArrayProp<PropType>(PropType propType) where PropType : IDataTableProp => (ArrayProp = (propType is RecvProp rp) ? rp : throw new InvalidCastException()) != null;
	public PropType GetArrayProp<PropType>() where PropType : IDataTableProp => ArrayProp is PropType pt ? pt : throw new InvalidCastException();
	public int GetNumElements() => Elements;
	public int SetNumElements(int elements) => Elements = elements;
	public SendPropType GetPropType() => RecvType;
	public PropFlags GetFlags() => Flags;
	public void SetFlags(PropFlags flags) => Flags = flags;

	public IDataTableBase<PropType>? GetDataTable<PropType>() where PropType : IDataTableProp
		=> DataTable == null ? null : DataTable is IDataTableBase<PropType> dt ? dt : throw new InvalidCastException();

	public void SetDataTable<PropType>(IDataTableBase<PropType>? dt) where PropType : IDataTableProp
		=> DataTable = dt == null ? null : dt is RecvTable rt ? rt : throw new InvalidCastException();
}

public class RecvDecoder
{
	public RecvDecoder() {

	}

	public ReadOnlySpan<char> GetName() {
		throw new NotImplementedException();
	}
	public SendTable? GetSendTable() => Precalc.GetSendTable();
	public RecvTable? GetRecvTable() => Table;
	public int GetNumProps() => Props.Count;
	public RecvProp? GetProp(int i) => (uint)i < (uint)GetNumProps() ? Props[i] : null;
	public SendProp GetSendProp(int i) => Precalc.GetProp(i);

	public int GetNumDatatableProps() => DataTableProps.Count;
	public RecvProp GetDatatableProp(int i) => DataTableProps[i];


	public RecvTable Table;
	public ClientSendTable ClientSendTable;

	public readonly SendTablePrecalc Precalc = new();

	public readonly List<RecvProp> Props = [];
	public readonly List<RecvProp> DataTableProps = [];
}

public class SendTablePrecalc
{
	public SendTablePrecalc() {

	}

	public bool SetupFlatPropertyArray() {
		SendTable? table = GetSendTable();

		table!.SetupArrayProps_R();

		ExcludeProp[] excludeProps = new ExcludeProp[Constants.MAX_EXCLUDE_PROPS];
		int numExcludeProps = 0;
		if (!SendTable.GetPropsExcluded(table!, excludeProps.AsSpan(), ref numExcludeProps, Constants.MAX_EXCLUDE_PROPS))
			return false;

		// Now build the hierarchy.
		BuildHierarchyStruct bhs = default;
		bhs.ExcludeProps = excludeProps;
		bhs.ExcludeProps = excludeProps;
		bhs.NumProps = bhs.NumDataTableProps = 0;
		bhs.PropProxies = 0;
		SendTable.BuildHierarchy(GetRootNode(), table, ref bhs);

		SendTable.SortByPriority(ref bhs);
		Props.Clear(); Props.AddRange(bhs.Props);
		DataTableProps.Clear(); DataTableProps.AddRange(bhs.DataTableProps[..bhs.NumDataTableProps]);
		PropProxyIndices.Clear(); PropProxyIndices.AddRange(bhs.PropProxyIndices[..bhs.NumProps]);

		SetNumDataTableProxies(0);
		DataTableHelpers.SetDataTableProxyIndices_R(this, GetRootNode(), ref bhs);

		int proxyIndices = 0;
		DataTableHelpers.SetRecursiveProxyIndices_R(table, GetRootNode(), ref proxyIndices);

		SendTable.GenerateProxyPaths(this, proxyIndices);
		return true;
	}

	public int GetNumProps() => Props.Count;
	public SendProp GetProp(int i) => Props[i];
	public int GetNumDatatableProps() => DataTableProps.Count;

	public SendProp GetDatatableProp(int i) => DataTableProps[i];

	public SendTable? GetSendTable() => SendTable;
	public SendNode GetRootNode() => Root;

	public int GetNumDataTableProxies() => DataTableProxies;

	public void SetNumDataTableProxies(int count) {
		DataTableProxies = count;
	}

	public struct ProxyPathEntry
	{
		public ushort DataTableProp;
		public ushort Proxy;
	}

	public struct ProxyPath
	{
		public ushort FirstEntry;
		public ushort Entries;
	}


	public readonly List<ProxyPathEntry> ProxyPathEntries = [];
	public readonly List<ProxyPath> ProxyPaths = [];
	public readonly List<SendProp> Props = [];
	public readonly List<byte> PropProxyIndices = [];
	public readonly List<SendProp> DataTableProps = [];
	public readonly SendNode Root = new();
	public SendTable SendTable;
	public int DataTableProxies;
	public readonly Dictionary<ushort, ushort> PropOffsetToIndexMap = [];
}

public class ClientSendProp
{
	private string? TableName;

	public ReadOnlySpan<char> GetTableName() => TableName;
	public void SetTableName(ReadOnlySpan<char> str) => TableName = new(str);
}
public class ClientSendTable
{
	public int GetNumProps() => SendTable?.Props?.Length ?? 0;
	public ClientSendProp GetClientProp(int i) => Props[i];
	public ReadOnlySpan<char> GetName() => SendTable == null ? null : SendTable.GetName();
	public SendTable? GetSendTable() => SendTable;

	public SendTable SendTable = new();
	public readonly List<ClientSendProp> Props = [];
}
public class SendNode
{
	public SendNode() {
		DataTableProp = -1;
		Table = null;

		FirstRecursiveProp = RecursiveProps = 0;
		DataTableProxyIndex = Constants.DATATABLE_PROXY_INDEX_INVALID;
	}

	public int GetNumChildren() => Children.Count;
	public SendNode GetChild(int i) => Children[i];
	public bool IsPropInRecursiveProps(int i) {
		int index = i - (int)FirstRecursiveProp;
		return index >= 0 && index < RecursiveProps;
	}
	public ushort GetDataTableProxyIndex() {
		Assert(DataTableProxyIndex != Constants.DATATABLE_PROXY_INDEX_INVALID);
		return DataTableProxyIndex;
	}
	public void SetDataTableProxyIndex(ushort val) {
		DataTableProxyIndex = val;
	}
	public ushort GetRecursiveProxyIndex() {
		return RecursiveProxyIndex;
	}
	public void SetRecursiveProxyIndex(ushort val) {
		RecursiveProxyIndex = val;
	}

	public readonly List<SendNode> Children = [];
	public short DataTableProp;

	public SendTable? Table;
	public ushort FirstRecursiveProp;
	public ushort RecursiveProps;
	public ushort DataTableProxyIndex;
	public ushort RecursiveProxyIndex;
}

public class RecvTable : IEnumerable<RecvProp>, IDataTableBase<RecvProp>
{
	public RecvTable() { }
	public RecvTable(RecvProp[] props) {
		Props = props;
	}
	public RecvTable(string name, RecvProp[] props) {
		NetTableName = name;
		Props = props;
	}
	public RecvProp[]? Props;
	public RecvDecoder? Decoder;
	public string? NetTableName;

	bool Initialized;
	bool InMainList;

	public IEnumerator<RecvProp> GetEnumerator() {
		return ((IEnumerable<RecvProp>)Props).GetEnumerator();
	}

	public ReadOnlySpan<char> GetName() => NetTableName;

	public int GetNumProps() => Props?.Length ?? 0;

	public RecvProp GetProp(int index) => Props![index];

	public bool IsInitialized() => Initialized;

	public void SetInitialized(bool initialized) => Initialized = initialized;

	IEnumerator IEnumerable.GetEnumerator() {
		return Props.GetEnumerator();
	}

	public bool IsInMainList() {
		return InMainList;
	}
	public void SetInMainList(bool inList) {
		InMainList = inList;
	}
}

public delegate void RecvVarProxyFn(ref readonly RecvProxyData data, object instance, FieldInfo field);
public delegate void DataTableRecvVarProxyFn(RecvProp prop, out object? outInstance, object? instance, FieldInfo fieldInfo, int objectID);
