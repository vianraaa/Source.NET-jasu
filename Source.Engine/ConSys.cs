using Source.Common.Client;
using Source.Common.Commands;
using Source.Common.GUI;

namespace Source.Engine;

public class Con(ICvar cvar, IEngineVGuiInternal EngineVGui, IVGuiInput Input, IBaseClientDLL ClientDLL)
{
	static ConVar con_enable = new("1", FCvar.Archive, "Allows the console to be activated.");

	[ConCommand]
	void toggleconsole() => ToggleConsole();

	public void ShowConsole() {
		if (Input.GetAppModalSurface() != null)
			return;

		if (!ClientDLL.ShouldAllowConsole())
			return;

		if (con_enable.GetBool()) {
			EngineVGui.ShowConsole();
			Singleton<Scr>().EndLoadingPlaque();
		}
	}

	public void HideConsole() {
		if (EngineVGui.IsConsoleVisible())
			EngineVGui.HideConsole();
	}

	public void ToggleConsole() {
		if (EngineVGui.IsConsoleVisible()) {
			HideConsole();
			EngineVGui.HideGameUI();
		}
		else
			ShowConsole();
	}

	public void Init() { }
	public void Shutdown() { }
	public void Execute() { }

	// TODO: ConPanel

	internal void ClearNotify() {

	}

	public void Clear() {
		Singleton<IEngineVGui>().ClearConsole();
		ClearNotify();
	}

	[ConCommand] void clear() => Clear();

	public void ColorPrintf(in Color clr, ReadOnlySpan<char> fmt) {
		cvar.ConsoleColorPrintf(in clr, fmt);
	}

	public bool IsVisible() => EngineVGui.IsConsoleVisible();
}