using Game.Client.GarrysMod;
using Game.Client.HL2MP;

using Source.Common;
using Source.Common.Client;

namespace Game.Client;

public partial class C_BasePlayer : C_BaseCombatCharacter, IGameEventListener2
{
	public static IClientNetworkable CreateObject(int entNum, int serialNum) {
		C_GMOD_Player ret = new C_GMOD_Player();
		ret.Init(entNum, serialNum);
		return ret;
	}
	public static readonly RecvTable DT_BasePlayer = new([

	]);
	public static readonly new ClientClass ClientClass = new ClientClass("BasePlayer", CreateObject, null, DT_BasePlayer);


	static C_BasePlayer? localPlayer;
	internal static C_BasePlayer? GetLocalPlayer() => localPlayer;

	public void FireGameEvent(IGameEvent ev) {
		throw new NotImplementedException();
	}

	public override void Dispose() {
		base.Dispose();
		if (this == localPlayer) {
			localPlayer = null;
		}
	}

	public override void PostDataUpdate(DataUpdateType updateType) {
		if(updateType == DataUpdateType.Created) {
			int localPlayerIndex = engine.GetLocalPlayer();

			if(localPlayerIndex == Index) {
				localPlayer = this;
			}
		}

		base.PostDataUpdate(updateType);
	}
}
