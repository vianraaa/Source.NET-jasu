global using static Game.Server.SendProxy;

using Game.Shared;

using Source;
using Source.Common;

using System.Reflection;
namespace Game.Server;

public static class SendProxy
{
	public const int PREDICTABLE_ID_BITS = 31;

	public static SendProp SendPropPredictableId(FieldInfo field)
		=> SendPropInt(field, PREDICTABLE_ID_BITS, PropFlags.Unsigned, SendProxy_PredictableIdToInt);

	private static void SendProxy_PredictableIdToInt(SendProp prop, object instance, FieldInfo field, ref DVariant outData, int element, int objectID) {
		PredictableId? pId = field.GetValueFast<PredictableId?>(instance);
		if (pId != null)
			outData.Int = pId.GetRaw();
		else 
			outData.Int = 0;
	}

	public static SendProp SendPropBool(FieldInfo field) {
		return SendPropInt(field, 1, PropFlags.Unsigned);
	}
	public static SendProp SendPropEHandle(FieldInfo field, PropFlags flags = 0, SendVarProxyFn? proxyFn = null) {
		return SendPropInt(field, Constants.NUM_NETWORKED_EHANDLE_BITS, PropFlags.Unsigned | flags, proxyFn ?? SendProxy_EHandleToInt);
	}

	private static void SendProxy_EHandleToInt(SendProp prop, object instance, FieldInfo field, ref DVariant outData, int element, int objectID) {
		BaseHandle? handle = field.GetValueFast<BaseHandle?>(instance);
		if (handle != null && handle.Get() != null) {
			int iSerialNum = handle.GetSerialNumber() & ((1 << Constants.NUM_NETWORKED_EHANDLE_SERIAL_NUMBER_BITS) - 1);
			outData.Int = handle.GetEntryIndex() | (iSerialNum << Constants.MAX_EDICT_BITS);
		}
		else
			outData.Int = Constants.INVALID_NETWORKED_EHANDLE_VALUE;
	}
}
