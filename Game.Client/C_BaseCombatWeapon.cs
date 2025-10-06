namespace Game.Client;

public partial class C_BaseCombatWeapon : C_BaseAnimating
{
	public static C_BaseCombatWeapon? GetActiveWeapon() {
		C_BasePlayer? player = C_BasePlayer.GetLocalPlayer();
		return player?.GetActiveWeapon();
	}
}
