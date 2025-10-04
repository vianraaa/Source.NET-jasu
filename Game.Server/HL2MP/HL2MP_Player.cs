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
		SendPropVector(FIELD.OF(nameof(Origin)), 0, PropFlags.NoScale|PropFlags.ChangesOften, 0.0f, Constants.HIGH_DEFAULT),

		SendPropFloat(FIELD.OF_VECTORELEM(nameof(AngEyeAngles), 0), 11, PropFlags.ChangesOften | PropFlags.RoundDown, 0, 360f ),
		SendPropAngle(FIELD.OF_VECTORELEM(nameof(AngEyeAngles), 1), 11, PropFlags.ChangesOften | PropFlags.RoundDown, 0, 360f ),
	]); public static readonly ServerClass SC_HL2MPLocalPlayerExclusive = new ServerClass("HL2MPLocalPlayerExclusive", DT_HL2MPLocalPlayerExclusive);

	public static readonly SendTable DT_HL2MPNonLocalPlayerExclusive = new([
		SendPropVector(FIELD.OF(nameof(Origin)), 0, PropFlags.CoordMPLowPrecision|PropFlags.ChangesOften, 0.0f, Constants.HIGH_DEFAULT),

		SendPropFloat(FIELD.OF_VECTORELEM(nameof(AngEyeAngles), 0), 11, PropFlags.ChangesOften | PropFlags.RoundDown, 0, 360f),
		SendPropAngle(FIELD.OF_VECTORELEM(nameof(AngEyeAngles), 1), 11, PropFlags.ChangesOften | PropFlags.RoundDown, 0, 360f),

	]); public static readonly ServerClass SC_HL2MPNonLocalPlayerExclusive = new ServerClass("HL2MPNonLocalPlayerExclusive", DT_HL2MPNonLocalPlayerExclusive);

	public static readonly SendTable DT_HL2MP_Player = new(DT_HL2_Player, [
		SendPropExclude(nameof(DT_BaseAnimating), nameof(PoseParameter)),
		SendPropExclude(nameof(DT_BaseAnimating), nameof(PlaybackRate)),
		SendPropExclude(nameof(DT_BaseAnimating), nameof(Sequence)),
		SendPropExclude(nameof(DT_BaseEntity), nameof(Rotation)),
		SendPropExclude(nameof(DT_BaseAnimatingOverlay), "overlay_vars"),

		SendPropExclude(nameof(DT_BaseEntity), nameof(Origin)),
		SendPropExclude(nameof(DT_ServerAnimationData), nameof(Cycle)),
		SendPropExclude(nameof(DT_AnimTimeMustBeFirst), nameof(AnimTime)),
		SendPropExclude(nameof(DT_BaseFlex), nameof(FlexWeight)),
		SendPropExclude(nameof(DT_BaseFlex), nameof(BlinkToggle)),
		SendPropExclude(nameof(DT_BaseFlex), nameof(ViewTarget)),

		SendPropDataTable("hl2mplocaldata", DT_HL2MPLocalPlayerExclusive, SendProxy_SendLocalDataTable ),
		SendPropDataTable("hl2mpnonlocaldata", DT_HL2MPNonLocalPlayerExclusive, SendProxy_SendNonLocalDataTable ),

		SendPropEHandle(FIELD.OF(nameof(Ragdoll))),
		SendPropInt(FIELD.OF(nameof(SpawnInterpCounter)), 4),
		SendPropInt(FIELD.OF(nameof(PlayerSoundType)), 3),
		SendPropBool(FIELD.OF(nameof(IsWalking))),

		SendPropExclude(nameof(DT_BaseAnimating), nameof(PoseParameter)),
		SendPropExclude(nameof(DT_BaseFlex), nameof(ViewTarget)),
	]);
	public static new readonly ServerClass ServerClass = new ServerClass("HL2MP_Player", DT_HL2MP_Player)
															.WithManualClassID(StaticClassIndices.CHL2MP_Player);

	public QAngle AngEyeAngles;
	public readonly EHANDLE Ragdoll = new();
	public int SpawnInterpCounter;
	public int PlayerSoundType;
	public bool IsWalking;
}