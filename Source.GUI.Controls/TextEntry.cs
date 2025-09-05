using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;

using static System.Net.Mime.MediaTypeNames;

namespace Source.GUI.Controls;

public class TextEntry : Panel
{
	public TextEntry(Panel? parent, string? name) : base(parent, name) { }

	bool SendNewLines;
	bool Wrap;
	public void SetWrap(bool state) {
		Wrap = state;
	}
	public void SendNewLine(bool state) {
		SendNewLines = state;
	}

	Color CursorColor;
	Color DisabledFgColor, DisabledBgColor;
	Color SelectionTextColor, SelectionColor, DefaultSelectionBG2Color, FocusEdgeColor;
	IFont? Font, SmallFont;
	bool HideText;
	bool Editable;
	int MaxCharCount;
	bool AllowNonAsciiCharacters;
	bool AllowNumericInputOnly;
	bool ShouldSelectAllOnFirstFocus;

	public void SetTextHidden(bool state) {
		HideText = state;
		Repaint();
	}
	public void SetEditable(bool state) {
		if (state)
			SetDropEnabled(true, 1);
		else
			SetDropEnabled(false);

		Editable = state;
	}
	public void SetMaximumCharCount(int chars) => MaxCharCount = chars;
	public void SetAllowNonAsciiCharacters(bool state) => AllowNonAsciiCharacters = state;
	public void SetAllowNumericInputOnly(bool state) => AllowNumericInputOnly = state;
	public void SelectAllOnFirstFocus(bool state) => ShouldSelectAllOnFirstFocus = state;

	public override void ApplySettings(KeyValues resourceData) {
		base.ApplySettings(resourceData);

		Font = GetScheme()!.GetFont(resourceData.GetString("font", "Default"), IsProportional());
		SetFont(Font);

		SetTextHidden(resourceData.GetInt("textHidden", 0) != 0);
		SetEditable(resourceData.GetInt("editable", 1) != 0);
		SetMaximumCharCount(resourceData.GetInt("maxchars", -1));
		SetAllowNumericInputOnly(resourceData.GetInt("NumericInputOnly", 0) != 0);
		SetAllowNonAsciiCharacters(resourceData.GetInt("unicode", 0) != 0);
		SelectAllOnFirstFocus(resourceData.GetInt("selectallonfirstfocus", 0) != 0);
	}
	public override void ApplySchemeSettings(IScheme scheme) {
		base.ApplySchemeSettings(scheme);

		SetFgColor(GetSchemeColor("TextEntry.TextColor", scheme));
		SetBgColor(GetSchemeColor("TextEntry.BgColor", scheme));

		CursorColor = GetSchemeColor("TextEntry.CursorColor", scheme);
		DisabledFgColor = GetSchemeColor("TextEntry.DisabledTextColor", scheme);
		DisabledBgColor = GetSchemeColor("TextEntry.DisabledBgColor", scheme);

		SelectionTextColor = GetSchemeColor("TextEntry.SelectedTextColor", GetFgColor(), scheme);
		SelectionColor = GetSchemeColor("TextEntry.SelectedBgColor", scheme);
		DefaultSelectionBG2Color = GetSchemeColor("TextEntry.OutOfFocusSelectedBgColor", scheme);
		FocusEdgeColor = GetSchemeColor("TextEntry.FocusEdgeColor", new(0, 0, 0, 0), scheme);

		SetBorder(scheme.GetBorder("ButtonDepressedBorder"));

		if (Font == null) Font = scheme.GetFont("Default", IsProportional());
		if (SmallFont == null) SmallFont = scheme.GetFont("DefaultVerySmall", IsProportional());

		SetFont(Font);
	}

	private void SetFont(IFont? font) {
		Font = font;
		InvalidateLayout();
		Repaint();
	}
}