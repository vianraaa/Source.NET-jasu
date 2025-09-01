using Source.Common.GUI;

namespace Source.GUI.Controls;

public class Label : Panel {
	protected Alignment Alignment;

	public Label(Panel? parent, string panelName) : base(parent, panelName) {

	}

	public const int Content = 8;

	public void SetContentAlignment(Alignment alignment) {
		Alignment = alignment;
	}

	public virtual void GetContentSize(out int wide, out int tall) {
		wide = 0;
		tall = 0;
	}
	public virtual void SetText(ReadOnlySpan<char> text) {
		// todo
	}
}
