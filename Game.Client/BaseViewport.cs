using Game.Client.HUD;

using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Engine;
using Source.GUI.Controls;

namespace Game.Client;

public interface IViewPortPanel
{
	ReadOnlySpan<char> GetName();
	void SetData(KeyValues? data);
	void Reset();
	void Update();
	bool NeedsUpdate();
	bool HasInputElements();
	void ShowPanel(bool state);
	GameActionSet GetPreferredActionSet();
	bool IsVisible();
	void SetParent(IPanel? parent);
}

public interface IViewPort
{
	void UpdateAllPanels();
	void ShowPanel(ReadOnlySpan<char> name, bool state);
	void ShowPanel(IViewPortPanel? panel, bool state);
	void ShowBackGround(bool show);
	IViewPortPanel? FindPanelByName(ReadOnlySpan<char> panelName);
	IViewPortPanel? GetActivePanel();
	void PostMessageToPanel(ReadOnlySpan<char> name, KeyValues? keyValues);
}

public class BaseViewport : EditablePanel, IViewPort {
	IViewPortPanel? ActivePanel;
	IViewPortPanel? LastActivePanel;
	IPanel? LastPanel;

	readonly Hud HUD = Singleton<Hud>();
	readonly IEngineVGui EngineVGui = Singleton<IEngineVGui>();
	bool Initialized;
	bool HasParent;

	public BaseViewport() : base(null, "BaseViewport") {
		SetSize(10, 10);
		Initialized = false;
		SetKeyboardInputEnabled(false);
		SetMouseInputEnabled(false);
		IScheme? scheme = SchemeManager.LoadSchemeFromFileEx(EngineVGui.GetPanel(VGuiPanelType.ClientDll), "resource/ClientScheme.res", "ClientScheme");
		SetScheme(scheme!);
		SetProportional(true);

		AnimController = new AnimationController(this);
		AnimController.SetScheme(scheme!);
		AnimController.SetProportional(true);

		 // TODO: Hud animations
	}

	AnimationController AnimController;

	public IViewPortPanel? FindPanelByName(ReadOnlySpan<char> panelName) {
		throw new NotImplementedException();
	}

	public IViewPortPanel? GetActivePanel() {
		return ActivePanel;
	}

	public void PostMessageToPanel(ReadOnlySpan<char> name, KeyValues? keyValues) {
		throw new NotImplementedException();
	}

	public void ShowBackGround(bool show) {
		throw new NotImplementedException();
	}

	public void ShowPanel(ReadOnlySpan<char> name, bool state) {
		throw new NotImplementedException();
	}

	public void ShowPanel(IViewPortPanel? panel, bool state) {
		throw new NotImplementedException();
	}

	public void UpdateAllPanels() {
		throw new NotImplementedException();
	}

	internal void ReloadScheme(ReadOnlySpan<char> from) {
		if (from != null) {
			IScheme scheme = SchemeManager.LoadSchemeFromFileEx(EngineVGui.GetPanel(VGuiPanelType.ClientDll), from, "HudScheme")!;

			SetScheme(scheme);
			SetProportional(true);
			AnimController.SetScheme(scheme);
		}

		if (LoadHudAnimations() == false) 
			if (!AnimController.SetScriptFile(this, "scripts/HudAnimations.txt", true)) 
				Assert(false);
			

		SetProportional(true);
		// todo: conditions
		// reload the .res file from disk
		LoadControlSettings("scripts/HudLayout.res", null, null, null);

		HUD.RefreshHudTextures();
		InvalidateLayout(true, true);
		HUD.ResetHUD();
	}

	private bool LoadHudAnimations() {
		return true; // lie for now
	}
}
