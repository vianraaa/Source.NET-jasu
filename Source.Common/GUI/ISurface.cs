using System.Numerics;

using static System.Net.Mime.MediaTypeNames;

namespace Source.Common.GUI;

public struct Vertex
{
	public Vector2 Position;
	public Vector2 TexCoord;
}

public enum FontDrawType
{
	Default,
	NonAdditive,
	Additive
}

public enum SurfaceFeature
{
	AntialiasedFonts = 1,
	DropShadowFonts = 2,
	EscapeKey = 3,
	OpeningNewHTMLWindows = 4,
	FrameMinimizeMaximize = 5,
	OutlineFonts = 6,
	DirectHwndRender = 7
}

public enum SurfaceFontFlags
{
	None,
	Italic = 0x001,
	Underline = 0x002,
	Strikeout = 0x004,
	Symbol = 0x008,
	Antialias = 0x010,
	GaussianBlur = 0x020,
	Rotary = 0x040,
	DropShadow = 0x080,
	Additive = 0x100,
	Outline = 0x200,
	Custom = 0x400,
	Bitmap = 0x800,
}

public interface ISurface
{
	void RunFrame();

	IPanel GetEmbeddedPanel();
	void SetEmbeddedPanel(IPanel panel);

	void PushMakeCurrent(IPanel panel, bool useInsets);
	void PopMakeCurrent(IPanel panel);

	void DrawSetColor(int r, int g, int b, int a);
	void DrawSetColor(in Color color);

	void DrawFilledRect(int x0, int y0, int x1, int y1);
	void DrawOutlinedRect(int x0, int y0, int x1, int y1);

	void DrawLine(int x0, int y0, int x1, int y1);
	void DrawPolyLine(Span<int> x, Span<int> y);

	void DrawSetTextFont(IFont font);
	void DrawSetTextColor(int r, int g, int b, int a);
	void DrawSetTextColor(in Color color);
	void DrawSetTextPos(int x, int y);
	void DrawGetTextPos(out int x, out int y);
	void DrawPrintText(Span<char> text, int textLen, FontDrawType drawType);

	void DrawFlushText();

	int DrawGetTextureId(ReadOnlySpan<char> filename);
	bool DrawGetTextureFile(int id, out ReadOnlySpan<char> filename);
	void DrawSetTextureFile(int id, in ReadOnlySpan<char> filename, int hardwareFilter, bool forceReload);
	void DrawSetTextureRGBA(int id, Span<byte> rgba, int wide, int tall, int hardwareFilter, bool forceReload);
	void DrawSetTexture(int id);
	void DrawGetTextureSize(int id, out int wide, out int tall);
	void DrawTexturedRect(int x0, int y0, int x1, int y1);
	bool IsTextureIDValid(int id);
	bool DeleteTextureByID(int id);
	int CreateNewTextureID(bool procedural = false);

	void GetScreenSize(out int wide, out int tall);
	void SetAsTopMost(IPanel panel, bool state);
	void BringToFront(IPanel panel);
	void SetForegroundWindow(IPanel panel);
	void SetPanelVisible(IPanel panel, bool state);
	void SetMinimized(IPanel panel, bool state);
	bool IsMinimized(IPanel panel);
	void FlashWindow(IPanel panel, bool state);
	void SetTitle(IPanel panel, ReadOnlySpan<char> title);
	void SetAsToolBar(IPanel panel, bool state);

	void CreatePopup(IPanel panel, bool minimised, bool showTaskbarIcon = true, bool disabled = false, bool mouseInput = true, bool kbInput = true);
	void SwapBuffers(IPanel panel);
	void Invalidate(IPanel panel);
	void SetCursor(ICursor cursor);
	void SetCursorAlwaysVisible(bool visible);
	bool IsCursorVisible();
	void ApplyChanges();
	bool IsWithin(int x, int y);
	bool HasFocus();

	bool SupportsFeature(SurfaceFeature feature);

	void RestrictPaintToSinglePanel(IPanel panel);


	void UnlockCursor();
	void LockCursor();
	void SetTranslateExtendedKeys(bool state);
	IPanel? GetTopmostPopup();
	void SetTopLevelFocus(IPanel panel);

	IFont CreateFont();

	bool SetFontGlyphSet(IFont font, ReadOnlySpan<char> windowsFontName, int tall, int weight, int blur, int scanlines, SurfaceFontFlags flags, int rangeMin = 0, int rangeMax = 0);
	bool AddCustomFontFile(ReadOnlySpan<char> fontName, ReadOnlySpan<char> fontFileName);

	int GetCharacterWidth(IFont font, int ch);
	void GetTextSize(IFont font, ReadOnlySpan<char> text, out int wide, out int tall);

	int GetPopupCount();
	IPanel? GetPopup(int index);
	bool ShouldPaintChildPanel(IPanel childPanel);
	bool RecreateContext(IPanel panel);
	void AddPanel(IPanel panel);
	void ReleasePanel(IPanel panel);
	void MovePopupToFront(IPanel panel);
	void MovePopupToBack(IPanel panel);

	void SolveTraverse(IPanel panel, bool forceApplySchemeSettings = false);
	void PaintTraverse(IPanel panel);

	void EnableMouseCapture(IPanel panel, bool state);

	void GetWorkspaceBounds(out int x, out int y, out int wide, out int tall);
	void GetAbsoluteWindowBounds(out int x, out int y, out int wide, out int tall);
	void GetProportionalBase(out int width, out int height);

	void CalculateMouseVisible();
	bool NeedKBInput();

	bool HasCursorPosFunctions();
	void SurfaceGetCursorPos(out int x, out int y);
	void SurfaceSetCursorPos(int x, int y);
}