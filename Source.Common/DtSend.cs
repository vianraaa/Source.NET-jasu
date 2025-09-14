using Source.Common.Engine;

using System.Collections;

namespace Source.Common;

public delegate Span<GetRefFn<InstanceType, ReturnType>> SendTableProxyFn<InstanceType, ReturnType>
	(ReadOnlySpan<SendProp> props, GetRefFn<InstanceType, SendTable> baseT, int objectID) where InstanceType : class;

public abstract class SendProp : IDataTableProp
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
	public string? VarName;

	PropFlags Flags;
	SendTable? DataTable;

	public PropType GetArrayProp<PropType>() where PropType : IDataTableProp {
		throw new NotImplementedException();
	}

	public SendTable? GetDataTable() => DataTable;

	IDataTableBase<PropType>? IDataTableProp.GetDataTable<PropType>() {
		return (IDataTableBase<PropType>?)DataTable;
	}

	public PropFlags GetFlags() => Flags;

	public ReadOnlySpan<char> GetName() => VarName;

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

	public static void BuildHierarchy(SendNode sendNode, SendTable? table, ref BuildHierarchyStruct bhs) {
		throw new NotImplementedException();
	}

	public static void SortByPriority(ref BuildHierarchyStruct bhs) {
		throw new NotImplementedException();
	}

	public bool IsInitialized() => Initialized;
	public void SetInitialized(bool initialized) => Initialized = initialized;
	public int GetNumProps() => Props.Length;

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

	public static void GenerateProxyPaths(SendTablePrecalc sendTablePrecalc, int proxyIndices) {
		throw new NotImplementedException();
	}

	public IEnumerator<SendProp> GetEnumerator() {
		return ((IEnumerable<SendProp>)Props).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return Props.GetEnumerator();
	}
}