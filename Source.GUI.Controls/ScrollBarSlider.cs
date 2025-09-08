using Source.Common.GUI;

namespace Source.GUI.Controls;

public class ScrollBarSlider : Panel
{
	bool Vertical;
	bool Dragging;
	readonly int[] NobPos = new int[2];
	readonly int[] NobDragStartPos = new int[2];
	readonly int[] DragStartPos = new int[2];
	readonly int[] Range = new int[2];
	int Value;
	int RangeWindow;
	int ButtonOffset;
	IBorder? ScrollBarSliderBorder;

	public ScrollBarSlider(Panel? parent, ReadOnlySpan<char> panelName, bool vertical) : base(parent, panelName) {
		Vertical = vertical;
	}

	public bool IsVertical() => Vertical;
	public void SetButtonOffset(int buttonOffset) => ButtonOffset = buttonOffset;
	public bool IsSliderVisible() {
		int itemRange = Range[1] - Range[0];

		if (itemRange <= 0)
			return false;

		if (itemRange <= RangeWindow)
			return false;

		return true;
	}

	public override void ApplySchemeSettings(IScheme scheme) {
		base.ApplySchemeSettings(scheme);

		SetFgColor(GetSchemeColor("ScrollBarSlider.FgColor", scheme));
		SetBgColor(GetSchemeColor("ScrollBarSlider.BgColor", scheme));

		ScrollBarSliderBorder = scheme.GetBorder("ScrollBarSliderBorder") ?? scheme.GetBorder("ButtonBorder");
	}

	public override void Paint() {
		GetPaintSize(out int wide, out int tall);

		if (!IsSliderVisible())
			return;

		Color col = GetFgColor();
		Surface.DrawSetColor(col);

		if (Vertical) {
			if (GetPaintBackgroundType() == PaintBackgroundType.Box) 
				DrawBox(1, NobPos[0], wide - 2, NobPos[1] - NobPos[0], col, 1.0f);
			else 
				Surface.DrawFilledRect(1, NobPos[0], wide - 2, NobPos[1]);
			
			ScrollBarSliderBorder?.Paint(0, NobPos[0], wide, NobPos[1]);
		}
		else {
			Surface.DrawFilledRect(NobPos[0], 1, NobPos[1], tall - 2);
			ScrollBarSliderBorder?.Paint(NobPos[0] - 1, 1, NobPos[1], tall);
		}
	}

	internal int GetRangeWindow() => RangeWindow;

	internal void GetNobPos(out int min, out int max) {
		min = NobPos[0];
		max = NobPos[1];
	}

	public int GetValue() {
		return Value;
	}

	public void SetValue(int val) {
		Value = val;
	}

	public void SetRange(int min, int max) {
		Range[0] = min;
		Range[1] = max;
	}
}
