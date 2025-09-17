using CommunityToolkit.HighPerformance;

using Source.Common.Engine;

using System.Buffers;
using System.Collections;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;

namespace Source.Common;


public static class SendPropHelpers
{
	public static SendProp SendPropFloat(FieldInfo field, int bits = 32, PropFlags flags = 0, float lowValue = 0, float highValue = Constants.HIGH_DEFAULT) {
		return new SendProp(field, SendPropType.Float, bits, flags, lowValue, highValue);
	}
	public static SendProp SendPropVector(FieldInfo field, int bits = 32, PropFlags flags = 0, float lowValue = 0, float highValue = Constants.HIGH_DEFAULT) {
		return new SendProp(field, SendPropType.Vector, bits, flags, lowValue, highValue);
	}
	public static SendProp SendPropInt(FieldInfo field, int bits = 32, PropFlags flags = 0, float lowValue = 0, float highValue = Constants.HIGH_DEFAULT) {
		return new SendProp(field, SendPropType.Int, bits, flags, lowValue, highValue);
	}
	public static SendProp SendPropString(FieldInfo field, int size, PropFlags flags = 0) {
		return new SendProp(field, SendPropType.String, 0);
	}
	public static SendProp SendPropStringT(FieldInfo field) {
		return SendPropString(field, Constants.DT_MAX_STRING_BUFFERSIZE, 0);
	}
	public static SendProp SendPropDataTable(string name, FieldInfo field) {
		SendProp prop = new SendProp(name, field, SendPropType.DataTable);
		prop.SetDataTable((SendTable)field.GetValue(null)!);
		return prop;
	}
}


public class SendProp : IDataTableProp
{
	public RecvProp? MatchingRecvProp;
	public SendPropType Type;
	public int Bits;
	public float LowValue;
	public float HighValue;
	public SendProp? ArrayProp;
	public int Elements;
	public string? ExcludeDTName;
	public string? ParentArrayPropName;
	public FieldInfo FieldInfo;
	public float HighLowMul;

	public SendProp() {

	}
	public SendProp(FieldInfo field, SendPropType type, int bits = 32, PropFlags flags = 0, float lowValue = 0f, float highValue = -121121.125f) {
		Type = type;
		FieldInfo = field;
		Bits = bits;
		Flags = flags;
		LowValue = lowValue;
		HighValue = highValue;
	}
	public SendProp(string? name, FieldInfo field, SendPropType type, int bits = 32, PropFlags flags = 0, float lowValue = 0f, float highValue = -121121.125f) {
		NameOverride = name;
		Type = type;
		FieldInfo = field;
		Bits = bits;
		Flags = flags;
		LowValue = lowValue;
		HighValue = highValue;
	}

	public string? NameOverride;
	PropFlags Flags;
	SendTable? DataTable;

	public T GetValue<T>(object instance) => FieldAccess<T>.Getter(FieldInfo)(instance);
	public void SetValue<T>(object instance, in T value) => FieldAccess<T>.Setter(FieldInfo)(instance, value);

	public PropType GetArrayProp<PropType>() where PropType : IDataTableProp {
		throw new NotImplementedException();
	}

	public SendTable? GetDataTable() => DataTable;

	IDataTableBase<PropType>? IDataTableProp.GetDataTable<PropType>() {
		return (IDataTableBase<PropType>?)DataTable;
	}

	public PropFlags GetFlags() => Flags;

	public ReadOnlySpan<char> GetName() => NameOverride ?? FieldInfo.Name;

	public int GetNumElements() => Elements;

	public SendPropType GetPropType() => Type;

	public bool IsExcludeProp() => (Flags & PropFlags.Exclude) != 0;
	public bool IsInsideArray() => (Flags & PropFlags.InsideArray) != 0;
	public bool IsSigned() => (Flags & PropFlags.Unsigned) == 0;
	public bool SetArrayProp<PropType>(PropType propType) where PropType : IDataTableProp {
		return (ArrayProp = propType is SendProp sp ? sp : null) != null;
	}

	public void SetFlags(PropFlags flags) => Flags = flags;
	public void SetInsideArray() => Flags |= PropFlags.InsideArray;
	public int SetNumElements(int elements) => Elements = elements;
	public ReadOnlySpan<char> GetExcludeDTName() => ExcludeDTName;

	public void SetDataTable<PropType>(IDataTableBase<PropType>? dt) where PropType : IDataTableProp
		=> DataTable = dt is SendTable st ? st : throw new InvalidCastException();

	public ReadOnlySpan<char> GetParentArrayPropName() {
		return ParentArrayPropName;
	}
	public void SetParentArrayPropName(ReadOnlySpan<char> str) {
		ParentArrayPropName = new(str);
	}

	object Fn;
	object DtFn;

	public object GetFn() {
		return Fn;
	}

	public void SetFn(object fn) {
		Fn = fn;
	}

	public void SetDataTableProxyFn(object value) {
		DtFn = value;
	}

	public object GetDataTableProxyFn() {
		return DtFn;
	}
}

public class SendTable : IEnumerable<SendProp>, IDataTableBase<SendProp>
{
	public SendTable() { }
	public SendTable(SendProp[] props) {
		Props = props;
	}

	public string? NetTableName;
	public SendTablePrecalc? Precalc;

	public SendProp[]? Props;

	protected bool Initialized;
	protected bool HasBeenWritten;
	protected bool HasPropsEncodedAgainstCurrentTickCount;

	public ReadOnlySpan<char> GetName() => NetTableName;

	public static void BuildHierarchy(SendNode node, SendTable? table, ref BuildHierarchyStruct bhs) {
		node.Table = table;
		node.FirstRecursiveProp = (ushort)bhs.NumProps;

		Assert(bhs.PropProxies < 255);
		byte curPropProxy = bhs.PropProxies;
		++bhs.PropProxies;

		SendProp[] nonDatatableProps = ArrayPool<SendProp>.Shared.Rent(Constants.MAX_TOTAL_SENDTABLE_PROPS);
		int numNonDatatableProps = 0;

		BuildHierarchy_IterateProps(node, table, ref bhs, nonDatatableProps, ref numNonDatatableProps);

		ErrorIfNot(bhs.NumProps + numNonDatatableProps < ((Span<SendProp>)bhs.Props).Length, "SendTable_BuildHierarchy: overflowed prop buffer.");

		for (int i = 0; i < numNonDatatableProps; i++) {
			bhs.Props[bhs.NumProps] = nonDatatableProps[i];
			bhs.PropProxyIndices[bhs.NumProps] = curPropProxy;
			++bhs.NumProps;
		}

		node.RecursiveProps = (ushort)(bhs.NumProps - node.FirstRecursiveProp);
		ArrayPool<SendProp>.Shared.Return(nonDatatableProps, true);
	}

	private static void BuildHierarchy_IterateProps(SendNode node, SendTable? table, ref BuildHierarchyStruct bhs, SendProp[] nonDatatableProps, ref int numNonDatatableProps) {
		int i;
		for (i = 0; i < (table?.Props?.Length ?? 0); i++) {
			SendProp prop = table.Props![i];

			if (prop.IsExcludeProp() || prop.IsInsideArray() || FindExcludeProp(table.GetName(), prop.GetName(), bhs.ExcludeProps!, out _)) {
				continue;
			}

			if (prop.GetPropType() == SendPropType.DataTable) {
				if ((prop.GetFlags() & PropFlags.Collapsible) != 0) {
					BuildHierarchy_IterateProps(
						node,
						prop.GetDataTable(),
						ref bhs,
						nonDatatableProps,
						ref numNonDatatableProps);
				}
				else {
					SendNode child = new();

					if (bhs.NumDataTableProps >= ((Span<SendProp>)bhs.DataTableProps).Length)
						Error($"Overflowed datatable prop list in SendTable '{table.GetName()}'.");

					bhs.DataTableProps[bhs.NumDataTableProps] = prop;
					child.DataTableProp = (short)bhs.NumDataTableProps;
					++bhs.NumDataTableProps;

					node.Children.Add(child);

					BuildHierarchy(child, prop.GetDataTable(), ref bhs);
				}
			}
			else {
				if (numNonDatatableProps >= Constants.MAX_TOTAL_SENDTABLE_PROPS)
					Error($"SendTable_BuildHierarchy: overflowed non-datatable props with '{prop.GetName()}'.");

				nonDatatableProps[numNonDatatableProps++] = prop;
			}
		}
	}

	private static bool FindExcludeProp(ReadOnlySpan<char> tableName, ReadOnlySpan<char> propName, ExcludeProp[]? excludeProps, out ExcludeProp excludeProp) {
		for (int i = 0; i < excludeProps?.Length; i++) {
			if (tableName.Equals(excludeProps[i].TableName!, StringComparison.OrdinalIgnoreCase) && propName.Equals(excludeProps[i].PropName!, StringComparison.OrdinalIgnoreCase)) {
				excludeProp = excludeProps[i];
				return true;
			}
		}

		excludeProp = default;
		return false;
	}

	public static void SortByPriority(ref BuildHierarchyStruct bhs) {
		int i, start = 0;

		while (true) {
			for (i = start; i < bhs.NumProps; i++) {
				SendProp p = bhs.Props[i];
				byte c = bhs.PropProxyIndices[i];

				if ((p.GetFlags() & PropFlags.ChangesOften) != 0) {
					bhs.Props[i] = bhs.Props[start];
					bhs.PropProxyIndices[i] = bhs.PropProxyIndices[start];
					bhs.Props[start] = p;
					bhs.PropProxyIndices[start] = c;
					start++;
					break;
				}
			}

			if (i == bhs.NumProps)
				return;
		}
	}

	public bool IsInitialized() => Initialized;
	public void SetInitialized(bool initialized) => Initialized = initialized;
	public int GetNumProps() => Props == null ? 0 : Props.Length;

	public static bool GetPropsExcluded(SendTable table, Span<ExcludeProp> excludeProps, ref int numExcludeProps, int maxExcludeProps) {
		for (int i = 0; i < table.Props!.Length; i++) {
			SendProp prop = table.Props[i];

			if (prop.IsExcludeProp()) {
				ReadOnlySpan<char> pName = prop.GetExcludeDTName();

				ErrorIfNot(pName != null, "Found an exclude prop missing a name.");
				ErrorIfNot(numExcludeProps < maxExcludeProps, $"SendTable_GetPropsExcluded: Overflowed max exclude props with {pName}.");

				excludeProps[numExcludeProps].TableName = new(pName);
				excludeProps[numExcludeProps].PropName = new(prop.GetName());
				numExcludeProps++;
			}
			else if (prop.GetDataTable() != null) {
				if (!GetPropsExcluded(prop.GetDataTable()!, excludeProps, ref numExcludeProps, maxExcludeProps))
					return false;
			}
		}

		return true;
	}

	public SendProp GetProp(int index) {
		return Props![index];
	}

	public static void GenerateProxyPaths(SendTablePrecalc precalc, int proxyIndices) {
		precalc.ProxyPaths.SetSize(proxyIndices);

		Span<SendTablePrecalc.ProxyPath> precalcs = precalc.ProxyPaths.AsSpan();
		for (int i = 0; i < proxyIndices; i++)
			precalcs[i].FirstEntry = precalcs[i].Entries = 0xFFFF;

		int totalPathLengths = 0;
		List<int> pathLengths = [];
		pathLengths.SetSize(proxyIndices);
		CalcPathLengths_R(precalc.GetRootNode(), pathLengths, 0, ref totalPathLengths);

		int curEntry = 0;
		precalc.ProxyPathEntries.SetSize(totalPathLengths);
		FillPathEntries_R(precalc, precalc.GetRootNode(), null, ref curEntry);
	}

	private static void CalcPathLengths_R(SendNode node, List<int> pathLengths, int curPathLength, ref int totalPathLengths) {
		pathLengths[node.GetRecursiveProxyIndex()] = curPathLength;
		totalPathLengths += curPathLength;

		for (int i = 0; i < node.GetNumChildren(); i++) {
			CalcPathLengths_R(node.GetChild(i), pathLengths, curPathLength + 1, ref totalPathLengths);
		}
	}

	private static void FillPathEntries_R(SendTablePrecalc precalc, SendNode node, SendNode? parent, ref int curEntry) {
		ref SendTablePrecalc.ProxyPath outProxyPath = ref precalc.ProxyPaths.AsSpan()[node.GetRecursiveProxyIndex()];
		outProxyPath.FirstEntry = (ushort)curEntry;

		if (parent != null) {
			ref SendTablePrecalc.ProxyPath parentProxyPath = ref precalc.ProxyPaths.AsSpan()[parent.GetRecursiveProxyIndex()];

			for (int i = 0; i < parentProxyPath.Entries; i++)
				precalc.ProxyPathEntries[curEntry++] = precalc.ProxyPathEntries[parentProxyPath.FirstEntry + i];

			precalc.ProxyPathEntries.AsSpan()[curEntry].Proxy = node.GetRecursiveProxyIndex();
			precalc.ProxyPathEntries.AsSpan()[curEntry].DataTableProp = (ushort)node.DataTableProp;
			++curEntry;
		}
		else
			outProxyPath.Entries = 0;

		for (int i = 0; i < node.GetNumChildren(); i++)
			FillPathEntries_R(precalc, node.GetChild(i), node, ref curEntry);
	}

	public IEnumerator<SendProp> GetEnumerator() {
		return ((IEnumerable<SendProp>)Props).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return Props.GetEnumerator();
	}
}

public delegate void SendVarProxyFn<TypeFrom, TypeTo>(SendProp prop, in TypeFrom input, out TypeTo output, int objectID);