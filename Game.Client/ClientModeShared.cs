using Game.Client.HUD;

using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Common.Client;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Common.Input;
using Source.GUI.Controls;

namespace Game.Client;

public enum GameActionSet {
	None = -1,
	MenuControls,
	FPSControls,
	InGameHUD,
	Spectator
}



public class ClientModeShared(IServiceProvider services, ClientGlobalVariables gpGlobals, Hud Hud) : IClientMode
{
	IEngineClient engine;
	public void Init() {
		engine = services.GetRequiredService<IEngineClient>();
		ChatElement = (BaseHudChat?)Hud.FindElement("CHudChat");
		Assert(ChatElement != null);
	}
	public virtual int KeyInput(int down, ButtonCode keynum, ReadOnlySpan<char> currentBinding) {
		if (engine.Con_IsVisible())
			return 1;

		if (currentBinding != null && currentBinding.Equals("messagemode", StringComparison.Ordinal) || currentBinding.Equals("say", StringComparison.Ordinal)) {
			if (down != 0)
				StartMessageMode(MessageModeType.Say);

			return 0;
		}
		else if (currentBinding != null && currentBinding.Equals("messagemode2", StringComparison.Ordinal) || currentBinding.Equals("say_team", StringComparison.Ordinal)) {
			if (down != 0)
				StartMessageMode(MessageModeType.SayTeam);

			return 0;
		}

		// In-game spectator
		// Hud element key input
		// Weapon input

		return 1;
	}

	public void StartMessageMode(MessageModeType messageModeType) {
		if (gpGlobals.MaxClients == 1)
			return;

		ChatElement?.StartMessageMode(messageModeType);
	}
	public void StopMessageMode() {
		ChatElement?.StopMessageMode();
		
	}

	public void OverrideMouseInput(ref float mouse_x, ref float mouse_y) {
		// nothing yet
	}

	protected BaseViewport Viewport;

	public Panel GetViewport() {
		return Viewport;
	}

	public BaseHudChat? ChatElement;
}