using System;
using System.Numerics;
using System.Runtime.InteropServices;

using static Source.Common.Networking.svc_ClassInfo;

namespace Source.Common;

public abstract class RecvProp {
	public string VarName;
	public SendPropType Type;
	public PropFlags Flags;

	public RecvProp(string varName, SendPropType type, PropFlags flags) {
		VarName = varName;
		Type = type;
		Flags = flags;
	}

	public abstract float GetFloat(object instance);
	public abstract void SetFloat(object instance, float value);
	public abstract int GetInt(object instance);
	public abstract void SetInt(object instance, int value);
	public abstract Vector3 GetVector3(object instance);
	public abstract void SetVector3(object instance, Vector3 value);
	public abstract ReadOnlySpan<char> GetString(object instance);
	public abstract void SetString(object instance, ReadOnlySpan<char> str);
}

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
		if(str.Length < writeTarget.Length)
			writeTarget[str.Length] = '\0';
	}
}


public class RecvTable : List<RecvProp>
{
	public string? NetTableName;

	public ReadOnlySpan<char> GetName() => NetTableName;
}