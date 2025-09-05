using Source.Common.Input;
using Source.GUI.Controls;

namespace Game.UI;

public class GameConsoleDialog : ConsoleDialog  {
	public GameConsoleDialog(Panel? parent, string? name, bool statusVersion) : base(parent, name, statusVersion) {
	}

	public override void OnKeyCodeTyped(ButtonCode code) {
		base.OnKeyCodeTyped(code);
	}
	public override void OnCommand(ReadOnlySpan<char> command) {
		base.OnCommand(command);
	}
}
