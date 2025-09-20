using Game.Server.HL2;
using Game.Shared;

using Source.Common;

namespace Game.Server.HL2MP;

public class HL2MP_Player : HL2_Player {
	public static readonly SendTable DT_HL2MP_Player = [

	];
	public static new readonly ServerClass ServerClass = new ServerClass("HL2MP_Player", DT_HL2MP_Player)
															.WithManualClassID(StaticClassIndices.CHL2MP_Player);
}