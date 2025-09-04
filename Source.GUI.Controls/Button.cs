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

	Color DefaultFgColor, DefaultBgColor;
	Color ArmedFgColor, ArmedBgColor;
	Color SelectedFgColor, SelectedBgColor;
	Color DepressedFgColor, DepressedBgColor;

	public void SetDefaultColor(Color fgColor, Color bgColor) {
		if (!(DefaultFgColor == fgColor && DefaultBgColor == bgColor)) {
			DefaultFgColor = fgColor;
			DefaultBgColor = bgColor;

			InvalidateLayout(false);
		}
	}
	public void SetSelectedColor(Color fgColor, Color bgColor) {
		if (!(SelectedFgColor == fgColor && SelectedBgColor == bgColor)) {
			SelectedFgColor = fgColor;
			SelectedBgColor = bgColor;

			InvalidateLayout(false);
		}
	}
	public void SetArmedColor(Color fgColor, Color bgColor) {
		if (!(ArmedFgColor == fgColor && ArmedBgColor == bgColor)) {
			ArmedFgColor = fgColor;
			ArmedBgColor = bgColor;

			InvalidateLayout(false);
		}
	}
	public void SetDepressedColor(Color fgColor, Color bgColor) {
		if (!(DepressedFgColor == fgColor && DepressedBgColor == bgColor)) {
			DepressedFgColor = fgColor;
			DepressedBgColor = bgColor;

			InvalidateLayout(false);
		}
	}

	public void SizeToContents() {
		GetContentSize(out int wide, out int tall);
		SetSize(wide + Label.Content, tall + Label.Content);
	}
}
