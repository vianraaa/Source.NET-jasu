#if CLIENT_DLL || GAME_DLL
using Source.Common;
namespace Game.Shared.GarrysMod;
using FIELD = Source.FIELD<WeaponShotgun>;
public class WeaponShotgun : BaseHL2MPCombatWeapon
{
	public static readonly
#if CLIENT_DLL
		RecvTable
#else
		SendTable
#endif
		DT_WeaponShotgun = new(DT_BaseHL2MPCombatWeapon, [
#if CLIENT_DLL

#else

#endif
		]);
#if CLIENT_DLL
	public static readonly new ClientClass ClientClass = new ClientClass("WeaponShotgun", null, null, DT_WeaponShotgun).WithManualClassID(StaticClassIndices.CWeaponShotgun);
#else
	public static readonly new ServerClass ServerClass = new ServerClass("WeaponShotgun", DT_WeaponShotgun).WithManualClassID(StaticClassIndices.CWeaponShotgun);
#endif
}
#endif
