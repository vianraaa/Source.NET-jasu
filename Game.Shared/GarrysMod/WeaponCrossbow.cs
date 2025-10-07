#if CLIENT_DLL || GAME_DLL
using Source.Common;
namespace Game.Shared.GarrysMod;
using FIELD = Source.FIELD<WeaponCrossbow>;
public class WeaponCrossbow : BaseHL2MPCombatWeapon
{
	public static readonly
#if CLIENT_DLL
		RecvTable
#else
		SendTable
#endif
		DT_WeaponCrossbow = new(DT_BaseHL2MPCombatWeapon, [
#if CLIENT_DLL

#else

#endif
		]);
#if CLIENT_DLL
	public static readonly new ClientClass ClientClass = new ClientClass("WeaponCrossbow", null, null, DT_WeaponCrossbow).WithManualClassID(StaticClassIndices.CWeaponCrossbow);
#else
	public static readonly new ServerClass ServerClass = new ServerClass("WeaponCrossbow", DT_WeaponCrossbow).WithManualClassID(StaticClassIndices.CWeaponCrossbow);
#endif
}
#endif