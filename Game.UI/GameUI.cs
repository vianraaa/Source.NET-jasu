using Source.Common.GameUI;
using Source.Common.GUI;

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
