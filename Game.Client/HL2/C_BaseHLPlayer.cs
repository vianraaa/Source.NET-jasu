using Game.Client.HL2MP;
using Game.Shared;

using Source.Common;

namespace Game.Client.HL2;
using FIELD = Source.FIELD<C_BaseHLPlayer>;

public partial class C_BaseHLPlayer : C_BasePlayer
{
	public static readonly RecvTable DT_HL2_Player = new(DT_BasePlayer, [
		RecvPropDataTable(nameof(HL2Local), C_HL2PlayerLocalData.DT_HL2Local),
		RecvPropBool(FIELD.OF(nameof(IsSprinting)))
	]);
	public static readonly new ClientClass ClientClass = new ClientClass("HL2_Player", null, null, DT_HL2_Player)
															.WithManualClassID(StaticClassIndices.CHL2_Player);

	public readonly C_HL2PlayerLocalData HL2Local = new();
	public bool IsSprinting;
}
