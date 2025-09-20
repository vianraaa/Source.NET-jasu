using Game.Shared;

using Source.Common;

namespace Game.Server.HL2;

public class HL2_Player : BasePlayer {
	public static readonly SendTable DT_HL2_Player = new(DT_BasePlayer, [

	]);
	public static new readonly ServerClass ServerClass = new ServerClass("HL2_Player", DT_HL2_Player)
															.WithManualClassID(StaticClassIndices.CHL2_Player);
}