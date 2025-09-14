using Game.Client.HL2;
using Game.Shared;

using Source.Common;
using Source.Common.Mathematics;

namespace Game.Client.HL2MP;

[DeclareClientClass]
[LinkEntityToClass(LocalName = "player")]
public partial class C_HL2MP_Player : C_BaseHLPlayer
{
	public static IClientNetworkable CreateObject(int entNum, int serialNum) {
		C_HL2MP_Player ret = new C_HL2MP_Player();
		ret.Init(entNum, serialNum);
		return ret;
	}
	public static readonly RecvTable DT_HL2MP_Player = [
		new RecvPropFloat<C_HL2MP_Player>((instance) => ref instance.EyeAngles.X),
		new RecvPropFloat<C_HL2MP_Player>((instance) => ref instance.EyeAngles.Y),
		new RecvPropEHandle<C_HL2MP_Player, EHANDLE>((instance) => ref instance.Ragdoll),
		new RecvPropInt<C_HL2MP_Player>((instance) => ref instance.SpawnInterpCounter),
		new RecvPropInt<C_HL2MP_Player>((instance) => ref instance.PlayerSoundType),
		new RecvPropBool<C_HL2MP_Player>((instance) => ref instance.IsWalking),
	];
	public static readonly ClientClass ClientClass = new ClientClass("HL2MP_Player", CreateObject, null, DT_HL2MP_Player)
															.WithManualClassID(StaticClassIndices.CHL2MP_Player);
	public override ClientClass GetClientClass() => ClientClass;

	public QAngle EyeAngles;
	public EHANDLE Ragdoll = new();
	public int SpawnInterpCounter;
	public int PlayerSoundType;
	public bool IsWalking;

	public override void PostDataUpdate(DataUpdateType updateType) {
		base.PostDataUpdate(updateType);
	}
}