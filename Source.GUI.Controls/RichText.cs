
using Source.Common.GUI;

using static System.Net.Mime.MediaTypeNames;

namespace Source.GUI.Controls;

public class RichText : Panel
{
	public RichText(Panel? parent, string? name) : base(parent, name) { }


	public void SetFont(IFont? font) {
		Font = font;
		InvalidateLayout();
		RecalcLineBreaks = true;
		Repaint();
	}

	IFont? Font;
	IFont? FontUnderline;
	bool RecalcLineBreaks;

	Color SelectionTextColor, SelectionColor;
	int DrawOffsetX;
	int DrawOffsetY;

	public void SetDrawOffsets(int x, int y) {
		DrawOffsetX = x;
		DrawOffsetY = y;
	}

	public override void ApplySchemeSettings(IScheme scheme) {
		base.ApplySchemeSettings(scheme);

		Font = scheme.GetFont("Default", IsProportional());
		FontUnderline = scheme.GetFont("DefaultUnderline", IsProportional());

		SetFgColor(GetSchemeColor("RichText.TextColor", scheme));
		SetBgColor(GetSchemeColor("RichText.BgColor", scheme));

		SelectionTextColor = GetSchemeColor("RichText.SelectedTextColor", GetFgColor(), scheme);
		SelectionColor = GetSchemeColor("RichText.SelectedBgColor", scheme);

		ReadOnlySpan<char> insetX = scheme.GetResourceString("RichText.InsetX");
		if (insetX != null && insetX.Length != 0)
			SetDrawOffsets(int.TryParse(scheme.GetResourceString("RichText.InsetX"), out int i1) ? i1 : 0, int.TryParse(scheme.GetResourceString("RichText.InsetY"), out int i2) ? i2 : 0);
	}
}