using Source;
using Source.Common;

using System;
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
public abstract class RecvProp
{
	public string VarName;
	public SendPropType Type;
	public PropFlags Flags;

	public RecvProp(string varName, SendPropType type, PropFlags flags) {
		VarName = varName;
		Type = type;
		Flags = flags;
	}

	object? recvFn;

	public abstract float GetFloat(object instance);
	public abstract void SetFloat(object instance, float value);
	public abstract int GetInt(object instance);
	public abstract void SetInt(object instance, int value);
	public abstract Vector3 GetVector3(object instance);
	public abstract void SetVector3(object instance, Vector3 value);
	public abstract ReadOnlySpan<char> GetString(object instance);
	public abstract void SetString(object instance, ReadOnlySpan<char> str);

	// ONLY used for datatable and array types (TODO: confirm the latter.)
	public virtual RecvTable GetRecvTable(object instance) => throw new NotImplementedException();

	public RecvVarProxyFn<Instance, FieldType> GetProxyFn<Instance, FieldType>() where Instance : class
		=> recvFn is RecvVarProxyFn<Instance, FieldType> fn ? fn : throw new InvalidCastException();
	public void SetProxyFn<Instance, FieldType>(RecvVarProxyFn<Instance, FieldType> fn) where Instance : class
		=> recvFn = fn;
}

public delegate void RecvVarProxyFn<Instance, FieldType>(ref RecvProxyData data, GetRefFn<Instance, FieldType> fieldFn) where Instance : class;

public class RecvPropFloat<T>(string varName, GetRefFn<T, float> refToField, PropFlags flags = 0) : RecvProp(varName, SendPropType.Float, flags) where T : class
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

public class RecvPropInt<T>(string varName, GetRefFn<T, int> refToField, PropFlags flags = 0) : RecvProp(varName, SendPropType.Int, flags) where T : class
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

public class RecvPropVector<T>(string varName, GetRefFn<T, Vector3> refToField, PropFlags flags = 0) : RecvProp(varName, SendPropType.Vector, flags) where T : class
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

public class RecvPropDataTable<T>(string varName, GetRefFn<T, RecvTable> refToField, PropFlags flags = 0) : RecvProp(varName, SendPropType.DataTable, flags) where T : class
{
	public override float GetFloat(object instance) => throw new NotImplementedException();
	public override int GetInt(object instance) => throw new NotImplementedException();
	public override Vector3 GetVector3(object instance) => throw new NotImplementedException();
	public override void SetFloat(object instance, float value) => throw new NotImplementedException();
	public override void SetInt(object instance, int value) => throw new NotImplementedException();
	public override void SetVector3(object instance, Vector3 value) => throw new NotImplementedException();
	public override ReadOnlySpan<char> GetString(object instance) => throw new NotSupportedException();
	public override void SetString(object instance, ReadOnlySpan<char> str) => throw new NotSupportedException();

	public override RecvTable GetRecvTable(object instance) => refToField((T)instance);
}

public class RecvPropBool<T>(string varName, GetRefFn<T, bool> refToField, PropFlags flags = 0) : RecvProp(varName, SendPropType.Int, flags) where T : class
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

public class RecvPropEHandle<T, BHT>(string varName, GetRefFn<T, BHT> refToField, PropFlags flags = 0) : RecvProp(varName, SendPropType.Int, flags) where T : class where BHT : BaseHandle
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


public class RecvPropSpan<T, ST>(string varName, GetSpanFn<T, ST> spanField, PropFlags flags = 0) : RecvProp(varName, SendPropType.String, flags) where T : class where ST : unmanaged
{
	public override float GetFloat(object instance) => throw new NotSupportedException();
	public override void SetFloat(object instance, float value) => throw new NotSupportedException();

	public override int GetInt(object instance) => throw new NotSupportedException();
	public override void SetInt(object instance, int value) => throw new NotSupportedException();

	public override Vector3 GetVector3(object instance) => throw new NotSupportedException();
	public override void SetVector3(object instance, Vector3 value) => throw new NotSupportedException();

	public override ReadOnlySpan<char> GetString(object instance) {
		Span<ST> span = spanField((T)instance);
		return MemoryMarshal.Cast<ST, char>(span);
	}
	public override void SetString(object instance, ReadOnlySpan<char> str) {
		Span<ST> span = spanField((T)instance);
		Span<char> writeTarget = MemoryMarshal.Cast<ST, char>(span);
		str.ClampedCopyTo(writeTarget);
		if (str.Length < writeTarget.Length)
			writeTarget[str.Length] = '\0';
	}
}


public class RecvDecoder
{
	public RecvDecoder() {

	}

	public ReadOnlySpan<char> GetName() {
		throw new NotImplementedException();
	}
	public SendTable GetSendTable() {
		throw new NotImplementedException();
	}
	public RecvTable GetRecvTable() {
		throw new NotImplementedException();
	}

	public int GetNumProps() {
		throw new NotImplementedException();
	}
	public RecvProp GetProp(int i) {
		throw new NotImplementedException();
	}
	public SendProp GetSendProp(int i) {
		throw new NotImplementedException();
	}

	public int GetNumDatatableProps() {
		throw new NotImplementedException();
	}
	public RecvProp GetDatatableProp(int i) {
		throw new NotImplementedException();
	}


	public RecvTable Table;
	public ClientSendTable ClientSendTable;

	public SendTablePrecalc m_Precalc;

	public readonly List<RecvProp> Props = [];
	public readonly List<RecvProp> DatatableProps = [];
}

public class SendTablePrecalc
{
	public SendTablePrecalc() {
		throw new NotImplementedException();
	}

	public bool SetupFlatPropertyArray() {
		SendTable? table = GetSendTable();

		table!.SetupArrayProps();

		ExcludeProp[] excludeProps = new ExcludeProp[Constants.MAX_EXCLUDE_PROPS];
		int numExcludeProps = 0;
		if (!SendTable.GetPropsExcluded(table!, excludeProps.AsSpan(), ref  numExcludeProps, Constants.MAX_EXCLUDE_PROPS))
			return false;

		// Now build the hierarchy.
		BuildHierarchyStruct bhs = default;
		bhs.ExcludeProps = excludeProps;
		bhs.ExcludeProps = excludeProps;
		bhs.NumProps = bhs.NumDatatableProps = 0;
		bhs.PropProxies = 0;
		SendTable.BuildHierarchy(GetRootNode(), table, ref bhs);

		SendTable.SortByPriority(ref bhs);
		Props.Clear(); Props.AddRange(bhs.Props);
		DataTableProps.Clear(); DataTableProps.AddRange(bhs.DatatableProps[..bhs.NumDatatableProps]);
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
	public SendNode Root;
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
	public int GetNumProps() => SendTable?.Count ?? 0;
	public ClientSendProp GetClientProp(int i) => Props[i];
	public ReadOnlySpan<char> GetName() => SendTable == null ? null : SendTable.GetName();
	public SendTable? GetSendTable() => SendTable;

	public SendTable? SendTable;
	public readonly List<ClientSendProp> Props = [];
}
public class SendNode
{
	public SendNode() {
		DatatableProp = -1;
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
	public short DatatableProp;

	public SendTable? Table;
	public ushort FirstRecursiveProp;
	public ushort RecursiveProps;
	public ushort DataTableProxyIndex;
	public ushort RecursiveProxyIndex;
}

public class RecvTable : List<RecvProp>
{
	public RecvDecoder? Decoder;
	public string? NetTableName;

	public ReadOnlySpan<char> GetName() => NetTableName;
}