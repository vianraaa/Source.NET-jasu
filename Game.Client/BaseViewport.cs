using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
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

	public BaseViewport() : base(null, "BaseViewport") {

	}

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
}
