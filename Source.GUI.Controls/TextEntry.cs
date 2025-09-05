using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Common.Input;

using System;

using static System.Net.Mime.MediaTypeNames;

namespace Source.GUI.Controls;

public class TextEntry : Panel
{
	public TextEntry(Panel? parent, string? name) : base(parent, name) {
		SetTriplePressAllowed(true);

		CursorBlinkRate = 400;
		MaxCharCount = -1;
		Select[0] = Select[1] = -1;

		ResetCursorBlink();
		SetCursor(CursorCode.IBeam);
		SetEditable(true);
		RecalculateBreaksIndex = 0;
		ShouldSelectAllOnFirstFocus = false;
		ShouldSelectAllOnFocusAlways = false;

	}

	public override void OnKeyFocusTicked() {
		base.OnKeyFocusTicked();
		long time = System.GetTimeMillis();
		if (time > CursorNextBlinkTime) {
			CursorBlink = !CursorBlink;
			CursorNextBlinkTime = time + CursorBlinkRate;
			Repaint();
		}
	}
	const int DRAW_OFFSET_X = 3;
	const int DRAW_OFFSET_Y = 1;
	public int GetYStart() => Multiline ? DRAW_OFFSET_Y : (GetTall() / 2) - (Surface.GetFontTall(Font) / 2);

	public override void PaintBackground() {
		base.PaintBackground();

		Color col = IsEnabled() ? GetBgColor() : DisabledBgColor;
		Color saveBgColor = col;

		GetSize(out int wide, out int tall);

		int x = DRAW_OFFSET_X + PixelsIndent, y = GetYStart();

		LangInset = 0;

		int langlen = 0;
		if (AllowNonAsciiCharacters) {
			// TODO: IME related things
		}

		IFont? useFont = Font;

		Surface.DrawSetTextFont(useFont);
		if (IsEnabled())
			col = GetFgColor();
		else
			col = DisabledFgColor;

		Surface.DrawSetTextColor(col);
		PixelsIndent = 0;

		int lineBreakIndexIndex = 0;
		int startIndex = GetStartDrawIndex(lineBreakIndexIndex);
		int remembery = y;

		int oldEnd = TextStream.Count();
		int oldCursorPos = CursorPos;
		int nCompStart = -1;
		int nCompEnd = -1;

		bool composing = false; // todo
		bool invertcomposition = Input.GetShouldInvertCompositionString();

		bool highlight_composition = (nCompStart != -1 && nCompEnd != -1) ? true : false;

		if ((!Multiline) && (!HorizScrollingAllowed)) {
			int endIndex = TextStream.Count;
			if ((!HasFocus() && (IsEditable())) || (!IsEditable())) {
				int i1 = -1;

				bool addEllipses = NeedsEllipses(useFont, ref i1);
				if (addEllipses && !IsEditable() && UseFallbackFont && FallbackFont != null) {
					useFont = FallbackFont;
					Surface.DrawSetTextFont(useFont);
					addEllipses = NeedsEllipses(useFont, ref i1);
				}
				if (addEllipses) {
					int elipsisWidth = 3 * getCharWidth(useFont, '.');
					while (elipsisWidth > 0 && i1 >= 0) {
						elipsisWidth -= getCharWidth(useFont, TextStream[i1]);
						i1--;
					}
					endIndex = i1 + 1;
				}

				if (TextStream.Count - endIndex < 3 && TextStream.Count - endIndex > 0) 
					endIndex = TextStream.Count - 3;
			}

			int i;
			for (i = startIndex; i < endIndex; i++) {
				char ch = TextStream[i];
				if (HideText) 
					ch = '*';

				bool iscompositionchar = false;

				if (highlight_composition) {
					iscompositionchar = (i >= nCompStart && i < nCompEnd) ? true : false;
					if (iscompositionchar) {
						Surface.DrawSetColor(col);

						int w = getCharWidth(useFont, ch);

						if (invertcomposition) {
							Surface.DrawSetTextColor(saveBgColor);
							Surface.DrawSetColor(col);

							Surface.DrawFilledRect(x, 0, x + w, tall);
							Surface.DrawSetColor(saveBgColor);
						}

						Surface.DrawFilledRect(x, tall - 2, x + w, tall - 1);
					}
				}

				x += DrawChar(ch, useFont, i, x, y);

				Surface.DrawSetTextColor(col);

			}
			if (endIndex < TextStream.Count) 
			{
				x += DrawChar('.', useFont, i++, x, y);
				x += DrawChar('.', useFont, i++, x, y);
				x += DrawChar('.', useFont, i++, x, y);
			}
		}
		else {
			for (int i = startIndex; i < TextStream.Count(); i++) {
				char ch = TextStream[i];
				if (HideText) 
					ch = '*';

				if (Multiline && LineBreaks[lineBreakIndexIndex] == i) {
					AddAnotherLine(ref x, ref y);
					lineBreakIndexIndex++;
				}

				bool iscompositionchar = false;

				if (highlight_composition) {
					iscompositionchar = (i >= nCompStart && i < nCompEnd) ? true : false;
					if (iscompositionchar) {
						Surface.DrawSetColor(col);

						int w = getCharWidth(useFont, ch);

						if (invertcomposition) {
							Surface.DrawSetTextColor(saveBgColor);
							Surface.DrawFilledRect(x, 0, x + w, tall);
							Surface.DrawSetColor(saveBgColor);
						}

						Surface.DrawFilledRect(x, tall - 2, x + w, tall - 1);
					}
				}

				x += DrawChar(ch, useFont, i, x, y);

				Surface.DrawSetTextColor(col);
			}
		}

		Surface.DrawSetColor(50, 50, 50, 255);

		if (IsEnabled() && IsEditable() && HasFocus()) {
			Surface.DrawSetColor(0, 0, 0, 255);

			DrawCursor(x, y);

			if (composing) {
				LocalToScreen(ref x, ref y);
				Input.SetCandidateWindowPos(x, y);
			}
		}

		int newEnd = TextStream.Count;
		int remove = newEnd - oldEnd;
		if (remove > 0)
			TextStream.RemoveRange(oldCursorPos, remove);

		CursorPos = oldCursorPos;

		if (HasFocus() && AllowNonAsciiCharacters && langlen > 0) {
			wide += LangInset;

			if (DrawLanguageIDAtLeft)
				x = 0;
			else
				x = wide - LangInset;


			Surface.DrawSetColor(col);

			Surface.DrawFilledRect(x, 2, x + LangInset - 2, tall - 2);

			saveBgColor[3] = 255;
			Surface.DrawSetTextColor(saveBgColor);

			x += 1;

			Surface.DrawSetTextFont(SmallFont);
			for (int i = 0; i < langlen; ++i) {
				// x += DrawChar(shortcode[i], SmallFont, i, x, remembery);
			}
		}
	}

	public override void OnSizeChanged(int newWide, int newTall) {
		RecalculateBreaksIndex = 0;
		LineBreaks.Clear();

		if (newWide > DrawWidth)
			ScrollLeftForResize();

		base.OnSizeChanged(newWide, newTall);
		DrawWidth = newWide;
		InvalidateLayout();
	}

	private void ScrollLeftForResize() {

	}

	public int GetDrawWidth() => DrawWidth;
	public void SetDrawWidth(int width) => DrawWidth = width;

	private bool NeedsEllipses(IFont? font, ref int index) {
		index = -1;
		int wide = DRAW_OFFSET_X;
		for (int i = 0; i < TextStream.Count; ++i) {
			wide += getCharWidth(font, TextStream[i]);
			if (wide > DrawWidth) {
				index = i;
				return true;
			}
		}
		return false;
	}

	public void CursorToPixelSpace(int cursorPos, out int x, out int y) {
		int yStart = GetYStart();

		x = DRAW_OFFSET_X;
		y = yStart;
		PixelsIndent = 0;
		int lineBreakIndexIndex = 0;

		for (int i = GetStartDrawIndex(lineBreakIndexIndex); i < TextStream.Count; i++) {
			char ch = TextStream[i];
			if (HideText) 
				ch = '*';

			if (cursorPos == i) 
				break;
			

			if (LineBreaks.Count > 0 && lineBreakIndexIndex < LineBreaks.Count && LineBreaks[lineBreakIndexIndex] == i) {
				AddAnotherLine(ref x, ref y);
				lineBreakIndexIndex++;
			}

			x += getCharWidth(Font, ch);
		}

		if (DrawLanguageIDAtLeft) 
			x += LangInset;
		
	}

	private void AddAnotherLine(ref int x, ref int y) {
		x = DRAW_OFFSET_X + PixelsIndent;
		y += Surface.GetFontTall(Font) + DRAW_OFFSET_Y;
	}

	private bool DrawCursor(int x, int y) {
		if (!CursorBlink) {
			CursorToPixelSpace(CursorPos, out int cx, out int cy);
			Surface.DrawSetColor(CursorColor);
			int fontTall = Surface.GetFontTall(Font);
			Surface.DrawFilledRect(cx, cy, cx + 1, cy + fontTall);
			return true;
		}

		return false;
	}

	int GetCursorLine() {
		int cursorLine;
		for (cursorLine = 0; cursorLine < LineBreaks.Count; cursorLine++)
			if (CursorPos < LineBreaks[cursorLine])
				break;

		if (PutCursorAtEnd)
			if (CursorPos != TextStream.Count)
				cursorLine--;


		return cursorLine;
	}

	private int GetStartDrawIndex(int lineBreakIndexIndex) {
		int startIndex = 0;

		int numLines = LineBreaks.Count;
		int startLine = 0;

		if (VertScrollBar != null && !MouseDragSelection)
			; // startLine = VertScrollBar.GetValue();
		else {
			IFont? font = Font;
			int displayLines = GetTall() / (Surface.GetFontTall(font) + DRAW_OFFSET_Y);
			if (displayLines < 1) {
				displayLines = 1;
			}
			if (numLines > displayLines) {
				int cursorLine = GetCursorLine();

				startLine = CurrentStartLine;

				if (cursorLine < CurrentStartLine) {
					startLine = cursorLine;
					if (VertScrollBar != null) {
						// MoveScrollBar(1); 
						// startLine = VertScrollBar.GetValue();
					}
				}
				else if (cursorLine > (CurrentStartLine + displayLines - 1)) {
					startLine = cursorLine - displayLines + 1;
					if (VertScrollBar != null) {
						// MoveScrollBar(-1);
						// startLine = VertScrollBar.GetValue();
					}
				}
			}
			else if (!Multiline) {
				bool done = false;
				while (!done) {
					done = true;
					int x = DRAW_OFFSET_X;
					for (int i = CurrentStartIndex; i < TextStream.Count; i++) {
						done = false;
						char ch = TextStream[i];
						if (HideText)
							ch = '*';

						if (CursorPos == i)
							break;

						x += getCharWidth(font, ch);
					}

					if (x >= GetWide()) {
						CurrentStartIndex++;
						continue;
					}

					if (x <= 0) {
						if (CurrentStartIndex > 0)
							CurrentStartIndex--;
					}

					break;
				}
			}
		}

		if (startLine > 0) {
			lineBreakIndexIndex = startLine;
			if (startLine != 0 && startLine < LineBreaks.Count) {
				startIndex = LineBreaks[startLine - 1];
			}
		}

		if (!HorizScrollingAllowed)
			return 0;

		CurrentStartLine = startLine;
		if (Multiline)
			return startIndex;
		else
			return CurrentStartIndex;

	}

	int getCharWidth(IFont? font, char ch) {
		if (!char.IsControl(ch)) {
			Surface.GetCharABCwide(font, ch, out int a, out int b, out int c);
			return a + b + c;
		}
		return 0;
	}
	int DrawChar(char ch, IFont? font, int index, int x, int y) {
		int charWide = getCharWidth(font, ch);
		int fontTall = Surface.GetFontTall(font);
		if (!char.IsControl(ch)) {
			int selection0 = -1, selection1 = -1;
			if (GetSelectedRange(out selection0, out selection1) && index >= selection0 && index < selection1) {
				IPanel? focus = Input.GetFocus();
				Color bgColor;
				bool hasFocus = HasFocus();
				bool childOfFocus = focus != null && focus.HasParent(this);

				if (hasFocus || childOfFocus)
					bgColor = SelectionColor;
				else
					bgColor = DefaultSelectionBG2Color;

				Surface.DrawSetColor(bgColor);
				Surface.DrawFilledRect(x, y, x + charWide, y + 1 + fontTall);
				Surface.DrawSetTextColor(SelectionTextColor);
			}
			if (index == selection1)
				Surface.DrawSetTextColor(GetFgColor());

			Surface.DrawSetTextPos(x, y);
			Surface.DrawChar(ch);

			return charWide;
		}

		return 0;
	}

	List<char> TextStream = ['h', 'e', 'l', 'l', 'o'];
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
	bool Multiline;
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
	int LangInset;
	bool DrawLanguageIDAtLeft;
	bool UseFallbackFont;
	IFont? FallbackFont;

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
		else if (code == ButtonCode.MouseRight) {
			//CreateEditMenu();
			//Assert(EditMenu);

			//OpenEditMenu();
		}
	}
	public override void OnMouseReleased(ButtonCode code) {
		MouseSelection = false;
		Input.SetMouseCapture(null);
		if (GetSelectedRange(out int cx0, out int cx1)) {
			// todo
		}
	}

	public override void OnKeyCodeTyped(ButtonCode code) {
		base.OnKeyCodeTyped(code);
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

	public bool IsEditable() => Editable;
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