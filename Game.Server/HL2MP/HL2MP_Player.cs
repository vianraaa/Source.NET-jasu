using Game.Server.HL2;
using Game.Shared;

using Source;
using Source.Common;
using Source.Common.Mathematics;

namespace Game.Server.HL2MP;

using FIELD = FIELD<HL2MP_Player>;

public class HL2MP_Player : HL2_Player
{
	public static readonly SendTable DT_HL2MPLocalPlayerExclusive = new([
		SendPropVectorXY(FIELD.OF(nameof(Origin)), -1, PropFlags.NoScale|PropFlags.ChangesOften, 0.0f, Constants.HIGH_DEFAULT, SendProxy_OriginXY),
		SendPropFloat(FIELD.OF_VECTORELEM(nameof(Origin), 2), -1, PropFlags.NoScale|PropFlags.ChangesOften, 0.0f, Constants.HIGH_DEFAULT, SendProxy_OriginZ),

		SendPropFloat(FIELD.OF_VECTORELEM(nameof(EyeAngles), 0), 8, PropFlags.ChangesOften, -90.0f, 90.0f ),
		SendPropAngle(FIELD.OF_VECTORELEM(nameof(EyeAngles), 1), 10, PropFlags.ChangesOften ),
	]); public static readonly ServerClass SC_HL2MPLocalPlayerExclusive = new ServerClass("HL2MPLocalPlayerExclusive", DT_HL2MPLocalPlayerExclusive);

	public static readonly SendTable DT_HL2MPNonLocalPlayerExclusive = new([
		SendPropVectorXY(FIELD.OF(nameof(Origin)), -1, PropFlags.CoordMPLowPrecision|PropFlags.ChangesOften, 0.0f, Constants.HIGH_DEFAULT, SendProxy_OriginXY),
		SendPropFloat(FIELD.OF_VECTORELEM(nameof(Origin), 2), -1, PropFlags.CoordMPLowPrecision|PropFlags.ChangesOften, 0.0f, Constants.HIGH_DEFAULT, SendProxy_OriginZ),

		SendPropFloat(FIELD.OF_VECTORELEM(nameof(EyeAngles), 0), 8, PropFlags.ChangesOften, -90.0f, 90.0f),
		SendPropAngle(FIELD.OF_VECTORELEM(nameof(EyeAngles), 1), 10, PropFlags.ChangesOften),

	]); public static readonly ServerClass SC_HL2MPNonLocalPlayerExclusive = new ServerClass("HL2MPNonLocalPlayerExclusive", DT_HL2MPNonLocalPlayerExclusive);

	public static readonly SendTable DT_HL2MP_Player = new(DT_HL2_Player, [
		SendPropExclude(nameof(DT_BaseEntity), nameof(Origin)),

		SendPropDataTable("hl2mplocaldata", DT_HL2MPLocalPlayerExclusive, SendProxy_SendLocalDataTable ),
		SendPropDataTable("hl2mpnonlocaldata", DT_HL2MPNonLocalPlayerExclusive, SendProxy_SendNonLocalDataTable ),

		SendPropEHandle(FIELD.OF(nameof(Ragdoll))),
		SendPropInt(FIELD.OF(nameof(SpawnInterpCounter)), 4),
		SendPropInt(FIELD.OF(nameof(PlayerSoundType)), 3),

		SendPropExclude(nameof(DT_BaseAnimating), nameof(PoseParameter)),
		SendPropExclude(nameof(DT_BaseFlex), nameof(ViewTarget)),
		SendPropExclude(nameof(DT_ServerAnimationData), nameof(Cycle)),
		SendPropExclude(nameof(DT_AnimTimeMustBeFirst), nameof(AnimTime)),
	]);
	public static new readonly ServerClass ServerClass = new ServerClass("HL2MP_Player", DT_HL2MP_Player)
															.WithManualClassID(StaticClassIndices.CHL2MP_Player);

	public QAngle EyeAngles;
	public readonly EHANDLE Ragdoll = new();
	public int SpawnInterpCounter;
	public int PlayerSoundType;
}