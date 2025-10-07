#if CLIENT_DLL || GAME_DLL
using Source.Common;
namespace Game.Shared.GarrysMod;
using FIELD = Source.FIELD<WeaponPistol>;
public class WeaponPistol : BaseHL2MPCombatWeapon
{
	public static readonly
#if CLIENT_DLL
		RecvTable
#else
		SendTable
#endif
		DT_WeaponPistol = new(DT_BaseHL2MPCombatWeapon, [
#if CLIENT_DLL

#else

#endif
		]);
#if CLIENT_DLL
	public static readonly new ClientClass ClientClass = new ClientClass("WeaponPistol", null, null, DT_WeaponPistol).WithManualClassID(StaticClassIndices.CWeaponPistol);
#else
	public static readonly new ServerClass ServerClass = new ServerClass("WeaponPistol", DT_WeaponPistol).WithManualClassID(StaticClassIndices.CWeaponPistol);
#endif
}
#endif
