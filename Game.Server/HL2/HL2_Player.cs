using Game.Shared;

using Source.Common;

namespace Game.Server.HL2;

public class HL2_Player : BasePlayer {
	public static readonly SendTable DT_HL2_Player = [
		// We gotta redo this later
		// Just focusing on the world for now.
	];
	public static readonly ServerClass ServerClass = new ServerClass("HL2_Player", DT_HL2_Player)
															.WithManualClassID(StaticClassIndices.CHL2_Player);
}