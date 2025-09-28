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


public partial class
#if CLIENT_DLL
	C_BaseCombatWeapon
#else
	BaseCombatWeapon
#endif
{
	public static readonly Table DT_LocalWeaponData = new([

	]); public static readonly Class SC_LocalWeaponData = Class.New(DT_LocalWeaponData);

	public static readonly Table DT_LocalActiveWeaponData = new([

	]); public static readonly Class SC_LocalActiveWeaponData = Class.New(DT_LocalActiveWeaponData);


	public static readonly Table DT_BaseCombatWeapon = new(DT_BaseAnimating, [

	]); public static readonly new Class ServerClass = Class.New(DT_BaseCombatWeapon).WithManualClassID(StaticClassIndices.CBaseCombatWeapon);
}

