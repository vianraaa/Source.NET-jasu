using Game.Client.HL2MP;
using Game.Shared;

using Source.Common;

namespace Game.Client.HL2;

public partial class C_BaseHLPlayer : C_BasePlayer
{
	public static readonly RecvTable DT_HL2_Player = [

	];
	public static readonly new ClientClass ClientClass = new ClientClass("HL2_Player", null, null, DT_HL2_Player)
															.WithManualClassID(StaticClassIndices.CHL2_Player);

}
