using SharpCompress.Common;

using Source;
using Source.Common;

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Source.Common;

/// <summary>
/// This is the base receive property abstract class. We differentiate from Source here by using 
/// generic-types that inherit from RecvProp for specific type access, where get/set goes through
/// overloaded abstract/virtual methods (replicating the RecvPropFloat/whatever calls that normally
/// get done via the implement clientclass etc. macros in Source). The way Source does it is with raw
/// pointer access, we semi-replicate that behavior in <see cref="GetRefFn{InstanceType, ReturnType}"/>
/// since it returns a by-reference var to the field on <c>InstanceType</c>.
/// </summary>
public abstract class RecvProp : IDataTableProp
{
	public string VarName;
	public SendPropType Type;
	public PropFlags Flags;
	public int StringBufferSize;

	public RecvProp(string varName, SendPropType type) {
		VarName = varName;
		Type = type;
	}

	bool InsideArray;
	RecvProp? ArrayProp;
	object? ProxyFn;
	RecvTable? DataTable;
	int Offset;
	int Elements;

	public abstract float GetFloat(object instance);
	public abstract void SetFloat(object instance, float value);
	public abstract int GetInt(object instance);
	public abstract void SetInt(object instance, int value);
	public abstract Vector3 GetVector3(object instance);
	public abstract void SetVector3(object instance, Vector3 value);
	public abstract ReadOnlySpan<char> GetString(object instance);
	public abstract void SetString(object instance, ReadOnlySpan<char> str);
	public virtual RecvTable GetRecvTable(object instance) => throw new NotImplementedException();

	public RecvVarProxyFn<Instance, FieldType> GetProxyFn<Instance, FieldType>() where Instance : class
		=> ProxyFn is RecvVarProxyFn<Instance, FieldType> fn ? fn : throw new InvalidCastException();
	public void SetProxyFn<Instance, FieldType>(RecvVarProxyFn<Instance, FieldType> fn) where Instance : class
		=> ProxyFn = fn;

	public ReadOnlySpan<char> GetName() => VarName;

	public bool IsSigned() => (Flags & PropFlags.Unsigned) == 0;
	public bool IsExcludeProp() => (Flags & PropFlags.Exclude) != 0;
	public bool IsInsideArray() => InsideArray;
	public void SetInsideArray() => InsideArray = true;
	public bool SetArrayProp<PropType>(PropType propType) where PropType : IDataTableProp => (ArrayProp = (propType is RecvProp rp) ? rp : throw new InvalidCastException()) != null;
	public PropType GetArrayProp<PropType>() where PropType : IDataTableProp => ArrayProp is PropType pt ? pt : throw new InvalidCastException();
	public int GetNumElements() => Elements;
	public int SetNumElements(int elements) => Elements = elements;
	public SendPropType GetPropType() => Type;
	public PropFlags GetFlags() => Flags;
	public void SetFlags(PropFlags flags) => Flags = flags;

	public IDataTableBase<PropType>? GetDataTable<PropType>() where PropType : IDataTableProp
		=> DataTable == null ? null : DataTable is IDataTableBase<PropType> dt ? dt : throw new InvalidCastException();

	public void SetDataTable<PropType>(IDataTableBase<PropType>? dt) where PropType : IDataTableProp
		=> DataTable = dt == null ? null : dt is RecvTable rt ? rt : throw new InvalidCastException();

	public object GetFn() {
		return ProxyFn;
	}

	public void SetFn(object fn) {
		ProxyFn = fn;
	}
}

public delegate void RecvVarProxyFn<Instance, FieldType>(ref RecvProxyData data, GetRefFn<Instance, FieldType> fieldFn) where Instance : class;

public class RecvPropFloat<T>(string varName, GetRefFn<T, float> refToField) : RecvProp(varName, SendPropType.Float) where T : class
{
	public override float GetFloat(object instance) => refToField((T)instance);
	public override void SetFloat(object instance, float value) {
		ref float fl = ref refToField((T)instance);
		fl = value;
	}
	public override int GetInt(object instance) => (int)refToField((T)instance);
	public override void SetInt(object instance, int value) {
		ref float fl = ref refToField((T)instance);
		fl = value;
	}
	public override Vector3 GetVector3(object instance) => new(refToField((T)instance));
	public override void SetVector3(object instance, Vector3 value) {
		ref float fl = ref refToField((T)instance);
		fl = value.X;
	}

	public override ReadOnlySpan<char> GetString(object instance) => throw new NotSupportedException();
	public override void SetString(object instance, ReadOnlySpan<char> str) => throw new NotSupportedException();
}
public class RecvPropInt<T>(string varName, GetRefFn<T, int> refToField) : RecvProp(varName, SendPropType.Int) where T : class
{
	public override float GetFloat(object instance) => refToField((T)instance);
	public override void SetFloat(object instance, float value) {
		ref int fl = ref refToField((T)instance);
		fl = (int)value;
	}
	public override int GetInt(object instance) => (int)refToField((T)instance);
	public override void SetInt(object instance, int value) {
		ref int fl = ref refToField((T)instance);
		fl = value;
	}
	public override Vector3 GetVector3(object instance) => new(refToField((T)instance));
	public override void SetVector3(object instance, Vector3 value) {
		ref int fl = ref refToField((T)instance);
		fl = (int)value.X;
	}

	public override ReadOnlySpan<char> GetString(object instance) => throw new NotSupportedException();
	public override void SetString(object instance, ReadOnlySpan<char> str) => throw new NotSupportedException();
}
public class RecvPropVector<T>(string varName, GetRefFn<T, Vector3> refToField) : RecvProp(varName, SendPropType.Vector) where T : class
{
	public override float GetFloat(object instance) => refToField((T)instance).X;
	public override void SetFloat(object instance, float value) {
		ref Vector3 fl = ref refToField((T)instance);
		fl = new(value, value, value);
	}
	public override int GetInt(object instance) => (int)refToField((T)instance).X;
	public override void SetInt(object instance, int value) {
		ref Vector3 fl = ref refToField((T)instance);
		fl = new(value, value, value);
	}
	public override Vector3 GetVector3(object instance) => refToField((T)instance);
	public override void SetVector3(object instance, Vector3 value) {
		ref Vector3 fl = ref refToField((T)instance);
		fl = value;
	}

	public override ReadOnlySpan<char> GetString(object instance) => throw new NotSupportedException();
	public override void SetString(object instance, ReadOnlySpan<char> str) => throw new NotSupportedException();
}
public class RecvPropDataTable<T> : RecvProp where T : class
{
	public RecvPropDataTable(string varName, RecvTable basetable) : base(varName, SendPropType.DataTable) {
		SetDataTable(basetable);
	}
	public override float GetFloat(object instance) => throw new NotImplementedException();
	public override int GetInt(object instance) => throw new NotImplementedException();
	public override Vector3 GetVector3(object instance) => throw new NotImplementedException();
	public override void SetFloat(object instance, float value) => throw new NotImplementedException();
	public override void SetInt(object instance, int value) => throw new NotImplementedException();
	public override void SetVector3(object instance, Vector3 value) => throw new NotImplementedException();
	public override ReadOnlySpan<char> GetString(object instance) => throw new NotSupportedException();
	public override void SetString(object instance, ReadOnlySpan<char> str) => throw new NotSupportedException();
}
public class RecvPropBool<T>(string varName, GetRefFn<T, bool> refToField) : RecvProp(varName, SendPropType.Int) where T : class
{
	public override float GetFloat(object instance) => refToField((T)instance) ? 1 : 0;
	public override void SetFloat(object instance, float value) {
		ref bool fl = ref refToField((T)instance);
		fl = value != 0;
	}
	public override int GetInt(object instance) => refToField((T)instance) ? 1 : 0;
	public override void SetInt(object instance, int value) {
		ref bool fl = ref refToField((T)instance);
		fl = value != 0;
	}
	public override Vector3 GetVector3(object instance) => new(refToField((T)instance) ? 1 : 0);
	public override void SetVector3(object instance, Vector3 value) {
		ref bool fl = ref refToField((T)instance);
		fl = value.X != 0;
	}

	public override ReadOnlySpan<char> GetString(object instance) => throw new NotSupportedException();
	public override void SetString(object instance, ReadOnlySpan<char> str) => throw new NotSupportedException();
}
public class RecvPropEHandle<T, BHT>(string varName, GetRefFn<T, BHT> refToField) : RecvProp(varName, SendPropType.Int) where T : class where BHT : BaseHandle
{
	public override float GetFloat(object instance) => refToField((T)instance).Index;
	public override void SetFloat(object instance, float value) => SetInt(instance, (int)value);

	public override int GetInt(object instance) => (int)refToField((T)instance).Index;
	public override void SetInt(object instance, int value) {
		ref BHT fl = ref refToField((T)instance);
		fl.Index = (uint)value;
	}

	public override Vector3 GetVector3(object instance) => new(GetFloat(instance));
	public override void SetVector3(object instance, Vector3 value) => SetFloat(instance, value.X);

	public override ReadOnlySpan<char> GetString(object instance) => throw new NotSupportedException();
	public override void SetString(object instance, ReadOnlySpan<char> str) => throw new NotSupportedException();
}
public class RecvPropString<T>(string varName, GetRefFn<T, string?> refToField) : RecvProp(varName, SendPropType.Int) where T : class
{
	public override float GetFloat(object instance) => float.TryParse(refToField((T)instance), out var i) ? i : default;
	public override void SetFloat(object instance, float value) {
		ref string? fl = ref refToField((T)instance);
		fl = value.ToString();
	}

	public override int GetInt(object instance) => int.TryParse(refToField((T)instance), out var i) ? i : default;
	public override void SetInt(object instance, int value) {
		ref string? fl = ref refToField((T)instance);
		fl = value.ToString();
	}

	public override Vector3 GetVector3(object instance) => throw new NotImplementedException();
	public override void SetVector3(object instance, Vector3 value) => throw new NotImplementedException();

	public override ReadOnlySpan<char> GetString(object instance) => refToField((T)instance);
	public override void SetString(object instance, ReadOnlySpan<char> str) {
		ref string? fl = ref refToField((T)instance);
		fl = new(str);
	}
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
		if (!SendTable.GetPropsExcluded(table!, excludeProps.AsSpan(), ref  numExcludeProps, Constants.MAX_EXCLUDE_PROPS))
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