namespace Source.Engine;

public enum VGuiPanelType {
	Root,
	GameUIDll,
	ClientDll,
	Tools,
	InGameScreens,
	GameDll,
	ClientDllTools
}
public enum PaintMode {
	UIPanels,
	InGamePanels,
	Cursor
}
public interface IEngineVGui
{
	public object? GetPanel(VGuiPanelType type);
	public bool IsGameUIVisible();
}