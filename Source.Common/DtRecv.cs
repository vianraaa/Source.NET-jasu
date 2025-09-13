namespace Source.Common;

public abstract class RecvProp {
	public abstract float GetFloat(object instance);
	public abstract void SetFloat(object instance, float value);
	public abstract int GetInt(object instance);
	public abstract void SetInt(object instance, int value);
}

public class RecvPropFloat<T>(GetRefFn<T, float> refToField) : RecvProp where T : class
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
}

public class RecvPropInt<T>(GetRefFn<T, int> refToField) : RecvProp where T : class
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
}

public class RecvPropBool<T>(GetRefFn<T, bool> refToField) : RecvProp where T : class
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
}

public class RecvPropEHandle<T, BHT>(GetRefFn<T, BHT> refToField) : RecvProp where T : class where BHT : BaseHandle
{
	public override float GetFloat(object instance) => refToField((T)instance).Index;
	public override void SetFloat(object instance, float value) => SetInt(instance, (int)value);

	public override int GetInt(object instance) => (int)refToField((T)instance).Index;
	public override void SetInt(object instance, int value) {
		ref BHT fl = ref refToField((T)instance);
		fl.Index = (uint)value;
	}
}

public class RecvTable : List<RecvProp>
{

}