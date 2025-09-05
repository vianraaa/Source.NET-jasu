namespace Source.GUI.Controls;

public class ScrollBar : Panel {
	static ScrollBar() => ChainToAnimationMap<ScrollBar>();
	public ScrollBar(Panel parent, ReadOnlySpan<char> panelName, bool vertical) : base(parent, panelName) {

	}

	public int GetValue() {
		throw new NotImplementedException();
	}

	public void SetValue(int val) {
		throw new NotImplementedException();
	}
}
