using Source.Common.Mathematics;

using System.Numerics;

#if CLIENT_DLL
namespace Game.Client;
#else
namespace Game.Server;
#endif

public partial class
#if CLIENT_DLL
	C_BasePlayer
#else
	BasePlayer
#endif
{
	public virtual void CalcView(ref Vector3 eyeOrigin, ref  QAngle eyeAngles, ref float zNear, ref float zFar, ref float fov) {
		CalcPlayerView(ref eyeOrigin, ref eyeAngles, ref fov); // << TODO: There is a lot more logic here for observers, vehicles, etc!
	}

	public BaseCombatWeapon? GetActiveWeapon() {
		throw new NotImplementedException();
	}

	private void CalcPlayerView(ref Vector3 eyeOrigin, ref QAngle eyeAngles, ref float fov) {
		throw new NotImplementedException();
	}
}

