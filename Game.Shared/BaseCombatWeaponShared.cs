#if CLIENT_DLL
global using BaseCombatWeapon = Game.Client.C_BaseCombatWeapon;
#else
global using BaseCombatWeapon = Game.Server.BaseCombatWeapon;
#endif

using Source.Common.Mathematics;
using Source.Common;
using System.Numerics;
using Game.Shared;

#if CLIENT_DLL
namespace Game.Client;
#else
namespace Game.Server;
#endif

using Table =
#if CLIENT_DLL
    RecvTable;
#else
	SendTable;
#endif

using Class =
#if CLIENT_DLL
    ClientClass;
#else
	ServerClass;
#endif

using FIELD = Source.FIELD<BaseCombatWeapon>;

public partial class
#if CLIENT_DLL
    C_BaseCombatWeapon : C_BaseAnimating
#else
	BaseCombatWeapon : BaseAnimating
#endif
{
	public static readonly Table DT_LocalWeaponData = new([
#if CLIENT_DLL
		RecvPropIntWithMinusOneFlag(FIELD.OF(nameof(Clip1))),
		RecvPropIntWithMinusOneFlag(FIELD.OF(nameof(Clip1))),
		RecvPropInt(FIELD.OF(nameof(PrimaryAmmoType))),
		RecvPropInt(FIELD.OF(nameof(SecondaryAmmoType))),
		RecvPropInt(FIELD.OF(nameof(ViewModelIndex))),
		RecvPropInt(FIELD.OF(nameof(FlipViewModel))),
#else
		SendPropIntWithMinusOneFlag(FIELD.OF(nameof(Clip1)), 16),
		SendPropIntWithMinusOneFlag(FIELD.OF(nameof(Clip1)), 16),
		SendPropInt(FIELD.OF(nameof(PrimaryAmmoType)), 8),
		SendPropInt(FIELD.OF(nameof(SecondaryAmmoType)), 8),
		SendPropInt(FIELD.OF(nameof(ViewModelIndex)), BaseViewModel.VIEWMODEL_INDEX_BITS, PropFlags.Unsigned),
		SendPropInt(FIELD.OF(nameof(FlipViewModel)), 8),
#endif
	]); public static readonly Class SC_LocalWeaponData = new Class("LocalWeaponData", DT_LocalWeaponData);

	public static readonly Table DT_LocalActiveWeaponData = new([
#if CLIENT_DLL

#else

#endif
	]); public static readonly Class SC_LocalActiveWeaponData = new Class("LocalActiveWeaponData", DT_LocalActiveWeaponData);

	public static readonly Table DT_BaseCombatWeapon = new(DT_BaseAnimating, [
#if CLIENT_DLL

#else

#endif
	]); public static readonly new Class ServerClass = new Class("BaseCombatWeapon", DT_BaseCombatWeapon).WithManualClassID(StaticClassIndices.CBaseCombatWeapon);

	public int Clip1;
	public int Clip2;
	public int PrimaryAmmoType;
	public int SecondaryAmmoType;
	public int ViewModelIndex;
	public bool FlipViewModel;

	public TimeUnit_t NextPrimaryAttack;
	public TimeUnit_t NextSecondaryAttack;
	public TimeUnit_t TimeWeaponIdle;
}