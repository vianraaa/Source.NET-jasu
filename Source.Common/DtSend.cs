using Source.Common.Engine;

using System.Collections;
using System.Numerics;

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

	public SendProp() {

	}
	public SendProp(string varName, SendPropType type, int bits = 32, PropFlags flags = 0, float lowValue = 0f, float highValue = -121121.125f) {
		Type = type;
		VarName = varName;
		Bits = bits;
		Flags = flags;
		LowValue = lowValue;
		HighValue = highValue;
	}

	PropFlags Flags;
	SendTable? DataTable;

	public abstract float GetFloat(object instance);
	public abstract void SetFloat(object instance, float value);
	public abstract int GetInt(object instance);
	public abstract void SetInt(object instance, int value);
	public abstract Vector3 GetVector3(object instance);
	public abstract void SetVector3(object instance, Vector3 value);
	public abstract ReadOnlySpan<char> GetString(object instance);
	public abstract void SetString(object instance, ReadOnlySpan<char> str);
	public virtual SendTable GetSendTable(object instance) => throw new NotImplementedException();

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

public class SendPropFloat<T>(string varName, GetRefFn<T, float> refToField, int bits = 32, PropFlags flags = 0, float lowValue = 0f, float highValue = -121121.125f) : SendProp(varName, SendPropType.Float, bits, flags, lowValue, highValue) where T : class
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
public class SendPropInt<T>(string varName, GetRefFn<T, int> refToField, int bits = 32, PropFlags flags = 0, float lowValue = 0f, float highValue = -121121.125f) : SendProp(varName, SendPropType.Int, bits, flags, lowValue, highValue) where T : class
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
public class SendPropVector<T>(string varName, GetRefFn<T, Vector3> refToField, int bits = 32, PropFlags flags = 0, float lowValue = 0f, float highValue = -121121.125f) : SendProp(varName, SendPropType.Vector, bits, flags, lowValue, highValue) where T : class
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
public class SendPropDataTable<T>(string varName, GetRefFn<T, SendTable> refToField) : SendProp(varName, SendPropType.DataTable) where T : class
{
	public override float GetFloat(object instance) => throw new NotImplementedException();
	public override int GetInt(object instance) => throw new NotImplementedException();
	public override Vector3 GetVector3(object instance) => throw new NotImplementedException();
	public override void SetFloat(object instance, float value) => throw new NotImplementedException();
	public override void SetInt(object instance, int value) => throw new NotImplementedException();
	public override void SetVector3(object instance, Vector3 value) => throw new NotImplementedException();
	public override ReadOnlySpan<char> GetString(object instance) => throw new NotSupportedException();
	public override void SetString(object instance, ReadOnlySpan<char> str) => throw new NotSupportedException();

	public override SendTable GetSendTable(object instance) => refToField((T)instance);
}
public class SendPropBool<T>(string varName, GetRefFn<T, bool> refToField, int bits = 32, PropFlags flags = 0, float lowValue = 0f, float highValue = -121121.125f) : SendProp(varName, SendPropType.Int, bits, flags, lowValue, highValue) where T : class
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
public class SendPropEHandle<T, BHT>(string varName, GetRefFn<T, BHT> refToField, int bits = 32, PropFlags flags = 0, float lowValue = 0f, float highValue = -121121.125f) : SendProp(varName, SendPropType.Int, bits, flags, lowValue, highValue) where T : class where BHT : BaseHandle
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
public class SendPropString<T>(string varName, GetRefFn<T, string?> refToField, int bits = 32, PropFlags flags = 0, float lowValue = 0f, float highValue = -121121.125f) : SendProp(varName, SendPropType.Int, bits, flags, lowValue, highValue) where T : class
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