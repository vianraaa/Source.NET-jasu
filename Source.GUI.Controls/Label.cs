using Source.Common.GUI;

namespace Source.GUI.Controls;

public class Label : Panel {
	protected Alignment Alignment;

	public Label(Panel? parent, string panelName) : base(parent, panelName) {

	}

	public void SetContentAlignment(Alignment alignment) {
		Alignment = alignment;
	}

	public virtual void SetText(ReadOnlySpan<char> text) {
		// todo
	}
}
