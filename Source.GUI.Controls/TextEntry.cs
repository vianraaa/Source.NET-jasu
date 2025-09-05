using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Common.Input;

using static System.Net.Mime.MediaTypeNames;

namespace Source.GUI.Controls;

public class TextEntry : Panel
{
	public TextEntry(Panel? parent, string? name) : base(parent, name) {
		SetTriplePressAllowed(true);

		SetCursor(CursorCode.IBeam);
	}

	List<char> TextStream = [];
	List<char> UndoTextStream = [];
	List<int> LineBreaks = [];

	int CursorPos;
	bool CursorIsAtEnd;
	bool PutCursorAtEnd;
	int UndoCursorPos;
	bool CursorBlink;
	bool HideText;
	bool Editable;
	bool MouseSelection;
	bool MouseDragSelection;
	long CursorNextBlinkTime;
	int CursorBlinkRate;
	int PixelsIndent;
	int CharsCount;
	int MaxCharCount;
	bool DataChanged;
	bool MultiLine;
	bool VerticalScrollbar;
	ScrollBar? VertScrollBar;
	int CurrentStartLine;
	int CurrentStartIndex;
	bool HorizScrollingAllowed;
	bool CatchEnterKey;
	bool Wrap;
	bool SendNewLines;
	int DrawWidth;
	int[] Select = new int[2];
	Menu? EditMenu;
	int RecalculateBreaksIndex;
	bool ShouldSelectAllOnFirstFocus;
	bool ShouldSelectAllOnFocusAlways;
	Color CursorColor;
	Color DisabledFgColor, DisabledBgColor;
	Color SelectionTextColor, SelectionColor, DefaultSelectionBG2Color, FocusEdgeColor;
	IFont? Font, SmallFont;
	bool AllowNonAsciiCharacters;
	bool AllowNumericInputOnly;

	public void SetWrap(bool state) {
		Wrap = state;
	}
	public void SendNewLine(bool state) {
		SendNewLines = state;
	}

	public override void OnMousePressed(ButtonCode code) {
		if (code == ButtonCode.MouseLeft) {
			bool keepChecking = SelectCheck(true);
			if (!keepChecking) {
				base.OnMousePressed(code);
				return;
			}

			Input.GetCursorPos(out int x, out int y);
			ScreenToLocal(ref x, ref y);

			CursorIsAtEnd = PutCursorAtEnd; 
			CursorPos = PixelToCursorSpace(x, y);
			if (CursorPos == 0)
				PutCursorAtEnd = false;

			Input.SetMouseCapture(this);
			MouseSelection = true;

			if (Select[0] < 0) 
				Select[0] = CursorPos;
			
			Select[1] = CursorPos;

			ResetCursorBlink();
			RequestFocus();
			Repaint();
		}
		else if (code == ButtonCode.MouseRight)
		{
			//CreateEditMenu();
			//Assert(EditMenu);

			//OpenEditMenu();
		}
	}
	public override void OnMouseReleased(ButtonCode code) {
		MouseSelection = false;
		Input.SetMouseCapture(null);
		if(GetSelectedRange(out int cx0, out int cx1)) {
			// todo
		}
	}

	private void ResetCursorBlink() {
		CursorBlink = false;
		CursorNextBlinkTime = System.GetTimeMillis() + CursorBlinkRate;
	}

	public void SetTextHidden(bool hideText) {
		HideText = hideText;
		Repaint();
	}

	private IPanel? GetDragPanel() => null; // todo

	public int PixelToCursorSpace(int cx, int cy) {
		return 0; // todo
	}

	public bool GetSelectedRange(out int cx0, out int cx1) {
		cx0 = cx1 = 0;
		return false;
	}


	private bool SelectCheck(bool fromMouse) {
		bool ret = true;
		if (!HasFocus() || !(Input.IsKeyDown(ButtonCode.KeyLShift) || Input.IsKeyDown(ButtonCode.KeyRShift))) {
			bool deselect = true;
			int cx0, cx1;
			if (fromMouse && GetDragPanel() != null) {
				Input.GetCursorPos(out int x, out int y);
				ScreenToLocal(ref x, ref y);
				int cursor = PixelToCursorSpace(x, y);

				bool check = GetSelectedRange(out cx0, out cx1);

				if (check && cursor >= cx0 && cursor < cx1) {
					deselect = false;
					ret = false;
				}
			}

			if (deselect) {
				Select[0] = -1;
			}
		}
		else if (Select[0] == -1) {
			Select[0] = CursorPos;
		}
		return ret;
	}

	public void SelectNone() {
		Select[0] = -1;
		Repaint();
	}
	public void SelectNoText() {
		Select[0] = -1;
		Select[1] = -1;
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