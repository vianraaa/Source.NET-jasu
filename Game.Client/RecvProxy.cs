global using static Game.Client.RecvProxy;
using Source;
using Source.Common;
using Source.Common.Networking;

using System.Reflection;

namespace Game.Client;

public static class RecvProxy
{
	public static void RecvProxy_IntToModelIndex16_BackCompatible(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		int modelIndex = data.Value.Int;
		if (modelIndex < -1 && engine.GetProtocolVersion() <= Protocol.PROTOCOL_VERSION_20) {
			Assert(modelIndex > -20000);
			modelIndex = -2 - ((-2 - modelIndex) << 1);
		}
		field.SetValueFast<short>(instance, unchecked((short)modelIndex));
	}
}
