using Game.Client.HUD;

using Source.Common;
using Source.Common.GUI;
using Source.Engine;

namespace Game.Client.HL2;

public class HudViewport : BaseViewport {

}

public class ClientModeHL2MPNormal : ClientModeShared
{
	public ClientModeHL2MPNormal(IServiceProvider services, ClientGlobalVariables gpGlobals, Hud Hud, IEngineVGui enginevgui, ISurface surface) : base(services, gpGlobals, Hud, enginevgui, surface) {
		Viewport = new HudViewport();
		Viewport.Start();
	}
}