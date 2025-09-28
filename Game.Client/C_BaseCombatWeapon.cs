using Game.Shared;

using Source.Common;
using FIELD = Source.FIELD<Game.Client.C_BaseAnimating>;

namespace Game.Client;

public partial class C_BaseCombatWeapon : C_BaseAnimating
{
	public static C_BaseCombatWeapon? GetActiveWeapon() {
		C_BasePlayer player = C_BasePlayer.GetLocalPlayer();
	}

	public static readonly RecvTable DT_BaseCombatWeapon = new(DT_BaseAnimating, [
		
	]);

	public static readonly new ClientClass ClientClass = new ClientClass("BaseCombatWeapon", null, null, DT_BaseCombatWeapon).WithManualClassID(StaticClassIndices.CBaseCombatWeapon);
}
