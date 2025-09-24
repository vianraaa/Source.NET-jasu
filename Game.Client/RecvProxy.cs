global using static Game.Client.RecvProxy;

using Game.Shared;

using Source;
using Source.Common;
using Source.Common.Networking;

using System.Reflection;

namespace Game.Client;

public static class RecvProxy
{
	public static void RecvProxy_IntToPredictableId(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		PredictableId pId = field.GetValueFast<PredictableId>(instance);
		pId.SetRaw(data.Value.Int);
	}
	public static RecvProp RecvPropPredictableId(FieldInfo field) {
		return RecvPropInt(field, 0, RecvProxy_IntToPredictableId);
	}

	public static void RecvProxy_IntToModelIndex16_BackCompatible(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		int modelIndex = data.Value.Int;
		if (modelIndex < -1 && engine.GetProtocolVersion() <= Protocol.PROTOCOL_VERSION_20) {
			Assert(modelIndex > -20000);
			modelIndex = -2 - ((-2 - modelIndex) << 1);
		}
		field.SetValueFast<short>(instance, unchecked((short)modelIndex));
	}
	public static RecvProp RecvPropTime(FieldInfo field) {
		return RecvPropFloat(field);
	}
	public static RecvProp RecvPropBool(FieldInfo field) => RecvPropInt(field);

	public static void RecvProxy_IntToMoveParent(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		EHANDLE handle = field.GetValueFast<EHANDLE>(instance);
		RecvProxy_IntToEHandle(in data, instance, field);
	}
}
