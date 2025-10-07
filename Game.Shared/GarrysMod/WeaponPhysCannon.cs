#if CLIENT_DLL || GAME_DLL
using Source.Common;
namespace Game.Shared.GarrysMod;
using FIELD = Source.FIELD<WeaponPhysCannon>;
public class WeaponPhysCannon : BaseHL2MPCombatWeapon
{
	public static readonly
#if CLIENT_DLL
		RecvTable
#else
		SendTable
#endif
		DT_WeaponPhysCannon = new(DT_BaseHL2MPCombatWeapon, [
#if CLIENT_DLL

#else

#endif
		]);
#if CLIENT_DLL
	public static readonly new ClientClass ClientClass = new ClientClass("WeaponPhysCannon", null, null, DT_WeaponPhysCannon).WithManualClassID(StaticClassIndices.CWeaponPhysCannon);
#else
	public static readonly new ServerClass ServerClass = new ServerClass("WeaponPhysCannon", DT_WeaponPhysCannon).WithManualClassID(StaticClassIndices.CWeaponPhysCannon);
#endif
}
#endif
