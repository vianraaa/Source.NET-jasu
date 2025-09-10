using Game.Client.HUD;

using Source.Common;
using Source.Common.Client;

namespace Game.Client.HL2;

public class HudViewport : BaseViewport {

}

public class ClientModeHL2MPNormal : ClientModeShared
{
	public ClientModeHL2MPNormal(IServiceProvider services, ClientGlobalVariables gpGlobals, Hud Hud) : base(services, gpGlobals, Hud) {
		Viewport = new HudViewport();
	}
}