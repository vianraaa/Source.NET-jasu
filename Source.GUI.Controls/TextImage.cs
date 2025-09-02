using Source.Common.GUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.GUI.Controls;

public struct ColorChange {
	public nint TextStreamIndex;
	public Color Color;
}

public class TextImage : Image
{

	string? Text;
	IFont? Font;
	IFont? FallbackFont;
	ulong UnlocalizedTextSymbol;
	int DrawWidth;
	nint EllipsesPosition;
	bool RecalculateTruncation;
	bool Wrap;
	bool UseFallbackFont;
	bool RenderUsingFallbackFont;
	bool AllCaps;
	bool WrapCenter;

	List<nint> LineBreaks = [];
	List<int> LineXIndent = [];
	List<ColorChange> ColorChangeStream = new();

	public TextImage(string text) : base() {
		Text = null;
		Font = null;
		FallbackFont = null;
		UnlocalizedTextSymbol = unchecked((ulong)-1);
		DrawWidth = 0;
		EllipsesPosition = 0;
		Wrap = false;
		UseFallbackFont = false;
		RenderUsingFallbackFont = false;
		AllCaps = false;
		WrapCenter = false;
		LineBreaks.Clear();
		LineXIndent.Clear();

		SetText(text);
	}

	public void SetText(ReadOnlySpan<char> text) {
		if (text == null)
			text = "";

		//  if (text[0] == '#') {
		//  	// TODO: Localization!!!
		//  }
		SetText(text, false);
	}

	private void SetText(ReadOnlySpan<char> text, bool clearUnlocalizedSymbol) {
		if (clearUnlocalizedSymbol) {
			UnlocalizedTextSymbol = unchecked((ulong)-1);
		}

		if (text == null)
			text = "";

		Text = new(text);
		LineBreaks.Clear();
		LineXIndent.Clear();
		RecalculateTruncation = true;
	}

	public void GetText(Span<char> buffer) {
		// todo
	}

	public IFont? GetFont() {
		if (RenderUsingFallbackFont)
			return FallbackFont;
		return Font;
	}

	protected override void SetSize(int wide, int tall) {
		base.SetSize(wide, tall);
		DrawWidth = wide;
		RecalculateTruncation = true;
	}

	public override void Paint() {
		GetSize(out int wide, out int tall);
		IFont? font = GetFont();

		if (Text == null || font == null)
			return;

		if (RecalculateTruncation) {
			if (Wrap || WrapCenter)
				RecalculateNewLinePositions();

			RecalculateEllipsesPosition();
		}

		DrawSetTextColor(GetSpewOutputColor());
		DrawSetTextFont(font);

		int lineHeight = Surface.GetFontTall(font);
		float x = 0.0f;
		int y = 0;
		int indent = 0;
		int nextColorChange = 0;

		GetPos(out int px, out int py);
		int currentLineBreak = 0;

		if (WrapCenter && LineXIndent.Count > 0)
			x = LineXIndent[0];

		for (nint i = 0, len = Text.Length; i < len; i++) {
			char ch = Text[(int)i];
			if (AllCaps)
				ch = char.ToUpperInvariant(ch);

			if(ColorChangeStream.Count > nextColorChange) 
				if (ColorChangeStream[nextColorChange].TextStreamIndex == i) 
					DrawSetTextColor(ColorChangeStream[nextColorChange++].Color);

			if (ch == '\r' || ch <= 8)
				continue;
			else if(ch == '\n') {
				indent++;
				if (WrapCenter && indent < LineXIndent.Count)
					x = LineXIndent[indent];
				else
					x = 0;
				y += lineHeight;
			}
			else if(ch == '&') {
				if (i + 1 < len && Text[(int)(i + 1)] == '&')
					i++;
				else {
					continue;
				}
			}

			if(i == EllipsesPosition) {
				for (int _ = 0; i < 3; i++) {
					Surface.DrawSetTextPos((int)x + px, y + py);
					Surface.DrawChar('.');
					x += Surface.GetCharacterWidth(font, '.');
				}
				break;
			}

			if(currentLineBreak != LineBreaks.Count) {
				if(i == LineBreaks[currentLineBreak]) {
					indent++;
					if (WrapCenter && indent < LineXIndent.Count)
						x = LineXIndent[indent];
					else
						x = 0;

					y += lineHeight;
					currentLineBreak++;
				}

				Surface.DrawSetTextPos((int)x + px, y + py);
				Surface.DrawChar(ch);
				x += Surface.GetCharacterWidth(font, '.');
			}
		}
	}

	private void RecalculateEllipsesPosition() {
		throw new NotImplementedException();
	}

	private void RecalculateNewLinePositions() {
		throw new NotImplementedException();
	}
}
