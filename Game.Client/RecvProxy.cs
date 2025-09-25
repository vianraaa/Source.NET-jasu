global using static Game.Client.RecvProxy;

using Game.Shared;

using Source;
using Source.Common;
using Source.Common.Networking;

using System.Reflection;

namespace Game.Client;

public static class RecvProxy
{
	public static void RecvProxy_IntToPredictableId(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
		PredictableId pId = field.GetValue<PredictableId>(instance);
		pId.SetRaw(data.Value.Int);
	}
	public static RecvProp RecvPropPredictableId(IFieldAccessor field) {
		return RecvPropInt(field, 0, RecvProxy_IntToPredictableId);
	}

	public static void RecvProxy_IntToModelIndex16_BackCompatible(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
		int modelIndex = data.Value.Int;
		if (modelIndex < -1 && engine.GetProtocolVersion() <= Protocol.PROTOCOL_VERSION_20) {
			Assert(modelIndex > -20000);
			modelIndex = -2 - ((-2 - modelIndex) << 1);
		}
		field.SetValue<short>(instance, unchecked((short)modelIndex));
	}
	public static RecvProp RecvPropTime(IFieldAccessor field) {
		return RecvPropFloat(field);
	}
	public static RecvProp RecvPropBool(IFieldAccessor field) => RecvPropInt(field);

	public static void RecvProxy_IntToMoveParent(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
		EHANDLE handle = field.GetValue<EHANDLE>(instance);
		RecvProxy_IntToEHandle(in data, instance, field);
	}
}
