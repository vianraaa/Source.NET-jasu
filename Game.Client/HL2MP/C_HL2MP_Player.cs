using Game.Client.HL2;
using Game.Shared;

using Source;
using Source.Common;
using Source.Common.Mathematics;

namespace Game.Client.HL2MP;
using FIELD = FIELD<C_HL2MP_Player>;

public partial class C_HL2MP_Player : C_BaseHLPlayer
{
	public static readonly RecvTable DT_HL2MPLocalPlayerExclusive = new([
		RecvPropVector(FIELD.OF(nameof(Origin))),

		RecvPropFloat(FIELD.OF_VECTORELEM(nameof(EyeAngles), 0)),
		RecvPropFloat(FIELD.OF_VECTORELEM(nameof(EyeAngles), 1)),
	]); public static readonly ClientClass CC_HL2MPLocalPlayerExclusive = new ClientClass("HL2MPLocalPlayerExclusive", null, null, DT_HL2MPLocalPlayerExclusive);

	public static readonly RecvTable DT_HL2MPNonLocalPlayerExclusive = new([
		RecvPropVector(FIELD.OF(nameof(Origin))),

		RecvPropFloat(FIELD.OF_VECTORELEM(nameof(EyeAngles), 0)),
		RecvPropFloat(FIELD.OF_VECTORELEM(nameof(EyeAngles), 1)),
	]); public static readonly ClientClass CC_HL2MPNonLocalPlayerExclusive = new ClientClass("HL2MPNonLocalPlayerExclusive", null, null, DT_HL2MPNonLocalPlayerExclusive);


	public static readonly RecvTable DT_HL2MP_Player = new(DT_BasePlayer, [
		RecvPropDataTable("hl2mplocaldata", DT_HL2MPLocalPlayerExclusive),
		RecvPropDataTable("hl2mpnonlocaldata", DT_HL2MPNonLocalPlayerExclusive),
		RecvPropEHandle(FIELD.OF(nameof(Ragdoll))),
		RecvPropInt(FIELD.OF(nameof(SpawnInterpCounter))),
		RecvPropInt(FIELD.OF(nameof(PlayerSoundType))),
		RecvPropBool(FIELD.OF(nameof(IsWalking)))
	]);
	public static readonly new ClientClass ClientClass = new ClientClass("HL2MP_Player", null, null, DT_HL2MP_Player)
															.WithManualClassID(StaticClassIndices.CHL2MP_Player);

	public QAngle EyeAngles;
	public EHANDLE Ragdoll = new();
	public int SpawnInterpCounter;
	public int PlayerSoundType;
	public bool IsWalking;

	public override void PostDataUpdate(DataUpdateType updateType) {
		base.PostDataUpdate(updateType);
	}
}