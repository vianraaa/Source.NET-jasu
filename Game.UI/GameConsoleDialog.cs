using Source.Common.Client;
using Source.Common.Engine;
using Source.Common.Input;
using Source.GUI.Controls;

namespace Game.UI;

public class GameConsoleDialog : ConsoleDialog  {
	public GameConsoleDialog() : base(null, "GameConsole", false) {
		AddActionSignalTarget(this);
	}

	public override void OnKeyCodeTyped(ButtonCode code) {
		base.OnKeyCodeTyped(code);

		if (ConsolePanel.TextEntryHasFocus()) {

		}
	}

	[Imported] public IEngineClient engine;

	protected override void OnCommandSubmitted(ReadOnlySpan<char> command) => engine.ClientCmd_Unrestricted(command);
	public override void OnCommand(ReadOnlySpan<char> command) {
		base.OnCommand(command);
	}
}
