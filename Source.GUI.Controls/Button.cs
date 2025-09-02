using Source.Common.Formats.Keyvalues;

namespace Source.GUI.Controls;

public class Button : Label {
	KeyValues? _actionMessage;

	public Button(Panel parent, string name, string text) : base(parent, name, text) {
	}

	public void SetCommand(ReadOnlySpan<char> command) => SetCommand(new KeyValues("Command", "command", command));
	public void SetCommand(KeyValues command) {
		_actionMessage = null;
		_actionMessage = command;
	}
}
