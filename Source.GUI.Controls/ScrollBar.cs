namespace Source.GUI.Controls;

public class ScrollBar : Panel {
	static ScrollBar() => ChainToAnimationMap<ScrollBar>();
	public ScrollBar(Panel parent, string panelName, bool vertical) : base(parent, panelName) {

	}

	public int GetValue() {
		return 0; // todo
	}

	public void SetValue(int val) {
		// todo
	}

	internal void GetRange(out int min, out int max) {
		min = max = 0; // todo
	}
}
