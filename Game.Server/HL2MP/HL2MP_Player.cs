using Game.Server.HL2;
using Game.Shared;

using Source;
using Source.Common;
using Source.Common.Mathematics;

namespace Game.Server.HL2MP;

using FIELD = FIELD<HL2MP_Player>;

public class HL2MP_Player : HL2_Player {
	public static readonly SendTable DT_HL2MP_Player = new(DT_HL2_Player, [
		SendPropAngle(FIELD.OF_VECTORELEM(nameof(EyeAngles), 0), 11, PropFlags.ChangesOften),
		SendPropAngle(FIELD.OF_VECTORELEM(nameof(EyeAngles), 1), 11, PropFlags.ChangesOften),
		SendPropEHandle(FIELD.OF(nameof(Ragdoll))),
		SendPropInt(FIELD.OF(nameof(SpawnInterpCounter)), 4),
		SendPropInt(FIELD.OF(nameof(PlayerSoundType)), 3),

		SendPropExclude(nameof(DT_BaseAnimating), nameof(PoseParameter)),
		SendPropExclude(nameof(DT_BaseFlex), nameof(ViewTarget)),
	]);
	public static new readonly ServerClass ServerClass = new ServerClass("HL2MP_Player", DT_HL2MP_Player)
															.WithManualClassID(StaticClassIndices.CHL2MP_Player);

	public QAngle EyeAngles;
	public readonly EHANDLE Ragdoll = new();
	public int SpawnInterpCounter;
	public int PlayerSoundType;
}