using Source.Common.GUI;

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
[Flags]
public enum PaintMode {
	UIPanels = 1 << 0,
	InGamePanels = 1 << 1,
	Cursor = 1 << 2
}
public interface IEngineVGui
{
	public IPanel GetPanel(VGuiPanelType type);
	public bool IsGameUIVisible();
}