using Game.Client.HL2MP;
using Game.Shared;

using Source.Common;

namespace Game.Client.GarrysMod;

[DeclareClientClass]
[LinkEntityToClass(LocalName = "player")]
public class C_GMOD_Player : C_HL2MP_Player
{
	public static IClientNetworkable CreateObject(int entNum, int serialNum) {
		C_GMOD_Player ret = new C_GMOD_Player();
		ret.Init(entNum, serialNum);
		return ret;
	}
	public static readonly RecvTable DT_GMOD_Player = new([
		RecvPropDataTable("baseclass", DT_HL2MP_Player),
	]);
	public static readonly new ClientClass ClientClass = new ClientClass("GMOD_Player", CreateObject, null, DT_GMOD_Player)
															.WithManualClassID(StaticClassIndices.CGMOD_Player);
}