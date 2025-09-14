namespace Source.Common;

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
	internal ReadOnlySpan<char> GetExcludeDTName() => ExcludeDTName;
}

public class SendTable : List<SendProp>, IDataTableBase<SendProp>
{
	public string? NetTableName;

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
	public int GetNumProps() => Count;

	public static bool GetPropsExcluded(SendTable table, Span<ExcludeProp> excludeProps, ref int numExcludeProps, int maxExcludeProps) {
		for (int i = 0; i < table.Count; i++) {
			SendProp prop = table[i];

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
		return this[index];
	}

	public static void GenerateProxyPaths(SendTablePrecalc sendTablePrecalc, int proxyIndices) {
		throw new NotImplementedException();
	}
}