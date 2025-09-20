global using static Game.Server.SendProxy;

using Game.Shared;

using Source;
using Source.Common;

using System.Reflection;
namespace Game.Server;

public static class SendProxy
{
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
