#if CLIENT_DLL
global using BaseCombatWeapon = Game.Client.C_BaseCombatWeapon;
#else
global using BaseCombatWeapon = Game.Server.BaseCombatWeapon;
#endif

using Source.Common.Mathematics;

using System.Numerics;

#if CLIENT_DLL
namespace Game.Client;
#else
namespace Game.Server;
#endif

public partial class
#if CLIENT_DLL
	C_BaseCombatWeapon
#else
	BaseCombatWeapon
#endif
{

}

