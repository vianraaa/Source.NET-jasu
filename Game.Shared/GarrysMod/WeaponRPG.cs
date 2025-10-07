#if CLIENT_DLL || GAME_DLL
using Source.Common;
namespace Game.Shared.GarrysMod;
using FIELD = Source.FIELD<WeaponRPG>;
public class WeaponRPG : BaseHL2MPCombatWeapon
{
	public static readonly
#if CLIENT_DLL
		RecvTable
#else
		SendTable
#endif
		DT_WeaponRPG = new(DT_BaseHL2MPCombatWeapon, [
#if CLIENT_DLL

#else

#endif
		]);
#if CLIENT_DLL
	public static readonly new ClientClass ClientClass = new ClientClass("WeaponRPG", null, null, DT_WeaponRPG).WithManualClassID(StaticClassIndices.CWeaponRPG);
#else
	public static readonly new ServerClass ServerClass = new ServerClass("WeaponRPG", DT_WeaponRPG).WithManualClassID(StaticClassIndices.CWeaponRPG);
#endif
}
#endif
