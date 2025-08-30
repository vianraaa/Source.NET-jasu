using Microsoft.Extensions.DependencyInjection;

using Source.Common.Engine;
using Source.Common.GameUI;
using Source.Common.GUI;
using Source.Engine;

namespace Game.UI;

public class GameUI : IGameUI
{
	public bool IsMainMenuVisible() {
		throw new NotImplementedException();
	}

	public void OnConfirmQuit() {
		throw new NotImplementedException();
	}

	public void OnConnectToServer(ReadOnlySpan<char> game, int ip, int connectionPort, int queryPort) {
		throw new NotImplementedException();
	}

	public void OnDisconnectFromServer(byte steamLoginFailure) {
		throw new NotImplementedException();
	}

	public void OnGameUIActivated() {
		// todo
	}

	public void OnGameUIHidden() {
		throw new NotImplementedException();
	}

	public void OnLevelLoadingFinished(bool error, ReadOnlySpan<char> failureReason, ReadOnlySpan<char> extendedReason) {
		throw new NotImplementedException();
	}

	public void OnLevelLoadingStarted(bool showProgressDialog) {
		throw new NotImplementedException();
	}

	BasePanel staticPanel;
	IEngineVGui enginevguifuncs;

	public void Initialize(IEngineAPI engineAPI) {
		enginevguifuncs = engineAPI.GetRequiredService<IEngineVGui>();

		staticPanel = engineAPI.New<BasePanel>();
		staticPanel.SetBounds(0, 0, 400, 300);
		staticPanel.SetPaintBorderEnabled(false);
		staticPanel.SetPaintBackgroundEnabled(true);
		staticPanel.SetPaintEnabled(false);
		staticPanel.SetVisible(true);
		staticPanel.SetMouseInputEnabled(false);
		staticPanel.SetKeyboardInputEnabled(false);

		IPanel rootpanel = enginevguifuncs.GetPanel(VGuiPanelType.GameUIDll);
		staticPanel.SetParent(rootpanel);
	}

	public void PostInit() {
		throw new NotImplementedException();
	}

	public void RunFrame() {
		throw new NotImplementedException();
	}

	public void SetMainMenuOverride(IPanel panel) {
		throw new NotImplementedException();
	}

	public bool SetShowProgressText(bool show) {
		throw new NotImplementedException();
	}

	public void Shutdown() {
		throw new NotImplementedException();
	}

	public void Start() {

	}

	public bool UpdateProgressBar(float progress, ReadOnlySpan<char> statusText) {
		throw new NotImplementedException();
	}
}
