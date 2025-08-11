using Source.Common.GUI;

namespace Source.MaterialSystem;

public class MatSystemSurface(MaterialSystem materials) : ISurface
{
	public bool AddCustomFontFile(ReadOnlySpan<char> fontName, ReadOnlySpan<char> fontFileName) {
		throw new NotImplementedException();
	}

	public void AddPanel(IPanel panel) {
		throw new NotImplementedException();
	}

	public void ApplyChanges() {
		throw new NotImplementedException();
	}

	public void BringToFront(IPanel panel) {
		throw new NotImplementedException();
	}

	public void CalculateMouseVisible() {
		throw new NotImplementedException();
	}

	public IFont CreateFont() {
		throw new NotImplementedException();
	}

	public int CreateNewTextureID(bool procedural = false) {
		throw new NotImplementedException();
	}

	public void CreatePopup(IPanel panel, bool minimised, bool showTaskbarIcon = true, bool disabled = false, bool mouseInput = true, bool kbInput = true) {
		throw new NotImplementedException();
	}

	public bool DeleteTextureByID(int id) {
		throw new NotImplementedException();
	}

	public void DrawFilledRect(int x0, int y0, int x1, int y1) {
		throw new NotImplementedException();
	}

	public void DrawFlushText() {
		throw new NotImplementedException();
	}

	public void DrawGetTextPos(out int x, out int y) {
		throw new NotImplementedException();
	}

	public bool DrawGetTextureFile(int id, out ReadOnlySpan<char> filename) {
		throw new NotImplementedException();
	}

	public int DrawGetTextureId(ReadOnlySpan<char> filename) {
		throw new NotImplementedException();
	}

	public void DrawGetTextureSize(int id, out int wide, out int tall) {
		throw new NotImplementedException();
	}

	public void DrawLine(int x0, int y0, int x1, int y1) {
		throw new NotImplementedException();
	}

	public void DrawOutlinedRect(int x0, int y0, int x1, int y1) {
		throw new NotImplementedException();
	}

	public void DrawPolyLine(Span<int> x, Span<int> y) {
		throw new NotImplementedException();
	}

	public void DrawPrintText(Span<char> text, int textLen, FontDrawType drawType) {
		throw new NotImplementedException();
	}

	public void DrawSetColor(int r, int g, int b, int a) {
		throw new NotImplementedException();
	}

	public void DrawSetColor(in Color color) {
		throw new NotImplementedException();
	}

	public void DrawSetTextColor(int r, int g, int b, int a) {
		throw new NotImplementedException();
	}

	public void DrawSetTextColor(in Color color) {
		throw new NotImplementedException();
	}

	public void DrawSetTextFont(IFont font) {
		throw new NotImplementedException();
	}

	public void DrawSetTextPos(int x, int y) {
		throw new NotImplementedException();
	}

	public void DrawSetTexture(int id) {
		throw new NotImplementedException();
	}

	public void DrawSetTextureFile(int id, in ReadOnlySpan<char> filename, int hardwareFilter, bool forceReload) {
		throw new NotImplementedException();
	}

	public void DrawSetTextureRGBA(int id, Span<byte> rgba, int wide, int tall, int hardwareFilter, bool forceReload) {
		throw new NotImplementedException();
	}

	public void DrawTexturedRect(int x0, int y0, int x1, int y1) {
		throw new NotImplementedException();
	}

	public void EnableMouseCapture(IPanel panel, bool state) {
		throw new NotImplementedException();
	}

	public void FlashWindow(IPanel panel, bool state) {
		throw new NotImplementedException();
	}

	public void GetAbsoluteWindowBounds(out int x, out int y, out int wide, out int tall) {
		throw new NotImplementedException();
	}

	public int GetCharacterWidth(IFont font, int ch) {
		throw new NotImplementedException();
	}

	public IPanel GetEmbeddedPanel() {
		throw new NotImplementedException();
	}

	public IPanel? GetPopup(int index) {
		throw new NotImplementedException();
	}

	public int GetPopupCount() {
		throw new NotImplementedException();
	}

	public void GetProportionalBase(out int width, out int height) {
		throw new NotImplementedException();
	}

	public void GetScreenSize(out int wide, out int tall) {
		throw new NotImplementedException();
	}

	public void GetTextSize(IFont font, ReadOnlySpan<char> text, out int wide, out int tall) {
		throw new NotImplementedException();
	}

	public IPanel? GetTopmostPopup() {
		throw new NotImplementedException();
	}

	public void GetWorkspaceBounds(out int x, out int y, out int wide, out int tall) {
		throw new NotImplementedException();
	}

	public bool HasCursorPosFunctions() {
		throw new NotImplementedException();
	}

	public bool HasFocus() {
		throw new NotImplementedException();
	}

	public void Invalidate(IPanel panel) {
		throw new NotImplementedException();
	}

	public bool IsCursorVisible() {
		throw new NotImplementedException();
	}

	public bool IsMinimized(IPanel panel) {
		throw new NotImplementedException();
	}

	public bool IsTextureIDValid(int id) {
		throw new NotImplementedException();
	}

	public bool IsWithin(int x, int y) {
		throw new NotImplementedException();
	}

	public void LockCursor() {
		throw new NotImplementedException();
	}

	public void MovePopupToBack(IPanel panel) {
		throw new NotImplementedException();
	}

	public void MovePopupToFront(IPanel panel) {
		throw new NotImplementedException();
	}

	public bool NeedKBInput() {
		throw new NotImplementedException();
	}

	public void PaintTraverse(IPanel panel) {
		throw new NotImplementedException();
	}

	public void PopMakeCurrent(IPanel panel) {
		throw new NotImplementedException();
	}

	public void PushMakeCurrent(IPanel panel, bool useInsets) {
		throw new NotImplementedException();
	}

	public bool RecreateContext(IPanel panel) {
		throw new NotImplementedException();
	}

	public void ReleasePanel(IPanel panel) {
		throw new NotImplementedException();
	}

	public void RestrictPaintToSinglePanel(IPanel panel) {
		throw new NotImplementedException();
	}

	public void RunFrame() {
		throw new NotImplementedException();
	}

	public void SetAsToolBar(IPanel panel, bool state) {
		throw new NotImplementedException();
	}

	public void SetAsTopMost(IPanel panel, bool state) {
		throw new NotImplementedException();
	}

	public void SetCursor(ICursor cursor) {
		throw new NotImplementedException();
	}

	public void SetCursorAlwaysVisible(bool visible) {
		throw new NotImplementedException();
	}

	public void SetEmbeddedPanel(IPanel panel) {
		throw new NotImplementedException();
	}

	public bool SetFontGlyphSet(IFont font, ReadOnlySpan<char> windowsFontName, int tall, int weight, int blur, int scanlines, SurfaceFontFlags flags, int rangeMin = 0, int rangeMax = 0) {
		throw new NotImplementedException();
	}

	public void SetForegroundWindow(IPanel panel) {
		throw new NotImplementedException();
	}

	public void SetMinimized(IPanel panel, bool state) {
		throw new NotImplementedException();
	}

	public void SetPanelVisible(IPanel panel, bool state) {
		throw new NotImplementedException();
	}

	public void SetTitle(IPanel panel, ReadOnlySpan<char> title) {
		throw new NotImplementedException();
	}

	public void SetTopLevelFocus(IPanel panel) {
		throw new NotImplementedException();
	}

	public void SetTranslateExtendedKeys(bool state) {
		throw new NotImplementedException();
	}

	public bool ShouldPaintChildPanel(IPanel childPanel) {
		throw new NotImplementedException();
	}

	public void SolveTraverse(IPanel panel, bool forceApplySchemeSettings = false) {
		throw new NotImplementedException();
	}

	public bool SupportsFeature(SurfaceFeature feature) {
		throw new NotImplementedException();
	}

	public void SurfaceGetCursorPos(out int x, out int y) {
		throw new NotImplementedException();
	}

	public void SurfaceSetCursorPos(int x, int y) {
		throw new NotImplementedException();
	}

	public void SwapBuffers(IPanel panel) {
		throw new NotImplementedException();
	}

	public void UnlockCursor() {
		throw new NotImplementedException();
	}
}
