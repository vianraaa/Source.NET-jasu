#if CLIENT_DLL || GAME_DLL
using Source.Common;
namespace Game.Shared.GarrysMod;
using FIELD = Source.FIELD<WeaponSLAM>;
public class WeaponSLAM : BaseHL2MPCombatWeapon
{
	public static readonly
#if CLIENT_DLL
		RecvTable
#else
		SendTable
#endif
		DT_WeaponSLAM = new(DT_BaseHL2MPCombatWeapon, [
#if CLIENT_DLL

#else

#endif
		]);
#if CLIENT_DLL
	public static readonly new ClientClass ClientClass = new ClientClass("WeaponSLAM", null, null, DT_WeaponSLAM).WithManualClassID(StaticClassIndices.CWeapon_SLAM);
#else
	public static readonly new ServerClass ServerClass = new ServerClass("WeaponSLAM", DT_WeaponSLAM).WithManualClassID(StaticClassIndices.CWeapon_SLAM);
#endif
}
#endif
