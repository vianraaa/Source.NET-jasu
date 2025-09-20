
using Game.Server.HL2MP;
using Game.Shared;

using Source.Common;

namespace Game.Server.GarrysMod;

public class GMOD_Player : HL2MP_Player
{
	public static readonly SendTable DT_GMOD_Player = new(DT_HL2MP_Player, [

	]);
	public static readonly new ServerClass ServerClass = new ServerClass("GMOD_Player", DT_GMOD_Player)
															.WithManualClassID(StaticClassIndices.CGMOD_Player);
}