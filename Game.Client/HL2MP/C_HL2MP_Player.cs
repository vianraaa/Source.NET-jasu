using Game.Client.HL2;
using Game.Shared;

using Source.Common;
using Source.Common.Mathematics;

namespace Game.Client.HL2MP;

[LinkEntityToClass(LocalName = "player")]
[ManualClassIndex(Index = StaticClassIndices.CHL2MP_Player)]
[ImplementClientClassDT(ClientClassName = nameof(C_HL2MP_Player), DataTable = nameof(DT_HL2MP_Player), ServerClassName = "HL2MP_Player")] // TODO: nameof(HL2MP_Player))]
public partial class C_HL2MP_Player : C_BaseHLPlayer
{
	public static readonly RecvTable DT_HL2MP_Player = new() {
		new RecvPropFloat<C_HL2MP_Player>((instance) => ref instance.EyeAngles.X),
		new RecvPropFloat<C_HL2MP_Player>((instance) => ref instance.EyeAngles.Y),
		new RecvPropEHandle<C_HL2MP_Player, EHANDLE>((instance) => ref instance.Ragdoll),
		new RecvPropInt<C_HL2MP_Player>((instance) => ref instance.SpawnInterpCounter),
		new RecvPropInt<C_HL2MP_Player>((instance) => ref instance.PlayerSoundType),
		new RecvPropBool<C_HL2MP_Player>((instance) => ref instance.IsWalking),
	};

	public QAngle EyeAngles;
	public EHANDLE Ragdoll = new();
	public int SpawnInterpCounter;
	public int PlayerSoundType;
	public bool IsWalking;

	public override void PostDataUpdate(DataUpdateType updateType) {
		base.PostDataUpdate(updateType);
	}
}