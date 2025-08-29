using Source.Common.Commands;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Common.Input;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;
using Source.GUI.Controls;

using System.Drawing;

using static Source.Dbg;
namespace Source.MaterialSystem.Surface;


public struct PaintState
{
	public IPanel Panel;
	public int TranslateX;
	public int TranslateY;
	public int ScissorLeft;
	public int ScissorRight;
	public int ScissorTop;
	public int ScissorBottom;
}

public struct ScissorRect
{
	public int Left;
	public int Top;
	public int Right;
	public int Bottom;
}


public class MatSystemSurface : ISurface
{
	readonly IMaterialSystem materials;
	bool InDrawing;
	bool In3DPaintMode;
	float zPos;
	ScissorRect scissorRect;
	bool scissor = false;
	bool fullScreenScissor = false;
	public int TranslateX;
	public int TranslateY;
	public float alphaMultiplier = 1;
	public int[] SurfaceExtents = new int[4];
	public Color DrawColor = new(255, 255, 255, 255);
	public Color DrawTextColor = new(255, 255, 255, 255);
	public IFont? DrawTextFont = null;
	public int TextPosX;
	public int TextPosY;
	IPanel DefaultEmbeddedPanel;
	IPanel? EmbeddedPanel;
	public int BoundTexture;

	IMesh? Mesh;
	MeshBuilder meshBuilder;
	MaterialReference White = new();

	float AlphaMultiplier;

	readonly ICommandLine CommandLine;

	public MatSystemSurface(IMaterialSystem materials, IShaderAPI shaderAPI, ICommandLine commandLine) {
		this.materials = materials;
		CommandLine = commandLine;

		DrawColor[0] = DrawColor[1] = DrawColor[2] = DrawColor[3] = 25; ;
		TranslateX = TranslateY = 0;
		// Scissor rect...
		AlphaMultiplier = 1;

		KeyValues vmtKeyValues = new KeyValues("UnlitGeneric");
		vmtKeyValues.SetString("$basetexture", "error");
		vmtKeyValues.SetInt("$vertexcolor", 1);
		vmtKeyValues.SetInt("$vertexalpha", 1);
		vmtKeyValues.SetInt("$ignorez", 1);
		vmtKeyValues.SetInt("$no_fullbright", 1);

		White.Init(materials, "VGUI_White", TEXTURE_GROUP_OTHER, vmtKeyValues);

		InitInput();
		InitCursors();

		DefaultEmbeddedPanel = new MatEmbeddedPanel();
		SetEmbeddedPanel(DefaultEmbeddedPanel);
	}

	private void InitInput() {
		EnableInput(true);
	}




	private void EnableInput(bool v) {

	}

	private void InitCursors() {

	}

	public bool FullyTransparent => DrawColor.A <= 0;

	public void InitVertex(ref SurfaceVertex vertex, int x, int y, float u, float v) {
		vertex.Position = new(x + TranslateX, y + TranslateY);
		vertex.TexCoord = new(u, v);
	}

	public void InternalSetMaterial(IMaterial? material = null) {
		if (material == null)
			material = White.Get();

		using MatRenderContextPtr renderContext = new(materials);
		Mesh = renderContext.GetDynamicMesh(true, null, null, material);
	}

	bool checkedCommandLine;

	float pixelOffsetX;
	float pixelOffsetY;

	public void StartDrawing() {
		if (!checkedCommandLine) {
			checkedCommandLine = true;

			ReadOnlySpan<char> pX = CommandLine.ParmValue("-pixel_offset_x", null);
			if (pX != null)
				pixelOffsetX = float.TryParse(pX, out pixelOffsetX) ? pixelOffsetX : 0;


			ReadOnlySpan<char> pY = CommandLine.ParmValue("-pixel_offset_y", null);
			if (pY != null)
				pixelOffsetY = float.TryParse(pY, out pixelOffsetY) ? pixelOffsetY : 0;
		}

		InDrawing = true;
		BoundTexture = -1;

		using MatRenderContextPtr renderContext = new(materials);
		renderContext.GetViewport(out int x, out int y, out int width, out int height);

		SurfaceExtents[0] = 0;
		SurfaceExtents[1] = 0;
		SurfaceExtents[2] = width;
		SurfaceExtents[3] = height;

		renderContext.MatrixMode(MaterialMatrixMode.Projection);
		renderContext.PushMatrix();
		renderContext.LoadIdentity();
		renderContext.Scale(1, -1, 1);

		renderContext.Ortho(pixelOffsetX, pixelOffsetY, width + pixelOffsetX, height + pixelOffsetY, -1.0f, 1.0f);

		// make sure there is no translation and rotation laying around
		renderContext.MatrixMode(MaterialMatrixMode.Model);
		renderContext.PushMatrix();
		renderContext.LoadIdentity();

		// Always enable scissoring (translate to origin because of the glTranslatef call above..)
		EnableScissor(true);

		TranslateX = 0;
		TranslateY = 0;

		renderContext.MatrixMode(MaterialMatrixMode.View);
		renderContext.PushMatrix();
		renderContext.LoadIdentity();
	}

	private void EnableScissor(bool v) {

	}

	public bool ClipRect(in SurfaceVertex inUL, in SurfaceVertex inLR, out SurfaceVertex outUL, out SurfaceVertex outLR) {
		if (scissor) {
			outUL = new();
			outLR = new();

			outUL.Position.X = scissorRect.Left > inUL.Position.X ? scissorRect.Left : inUL.Position.X;
			outLR.Position.X = scissorRect.Right <= inLR.Position.X ? scissorRect.Right : inLR.Position.X;
			outUL.Position.Y = scissorRect.Top > inUL.Position.Y ? scissorRect.Top : inUL.Position.Y;
			outLR.Position.Y = scissorRect.Bottom <= inLR.Position.Y ? scissorRect.Bottom : inLR.Position.Y;

			// check non intersecting
			if (outUL.Position.X > outLR.Position.X || outUL.Position.Y > outLR.Position.Y)
				return false;

			outUL.TexCoord = inUL.TexCoord;
			outLR.TexCoord = inLR.TexCoord;
		}
		else {
			outUL = inUL;
			outLR = inLR;
		}

		return true;
	}


	public void DrawQuad(in SurfaceVertex ul, in SurfaceVertex lr, in Color color) {
		if (Mesh == null)
			return;

		meshBuilder.Begin(Mesh, MaterialPrimitiveType.Quads, 1);

		meshBuilder.Position3f(ul.Position.X, ul.Position.Y, zPos);
		meshBuilder.Color4ubv(color);
		meshBuilder.TexCoord2f(0, ul.TexCoord.X, ul.TexCoord.Y);
		meshBuilder.AdvanceVertex();

		meshBuilder.Position3f(lr.Position.X, ul.Position.Y, zPos);
		meshBuilder.Color4ubv(color);
		meshBuilder.TexCoord2f(0, lr.TexCoord.X, ul.TexCoord.Y);
		meshBuilder.AdvanceVertex();

		meshBuilder.Position3f(lr.Position.X, lr.Position.Y, zPos);
		meshBuilder.Color4ubv(color);
		meshBuilder.TexCoord2f(0, lr.TexCoord.X, lr.TexCoord.Y);
		meshBuilder.AdvanceVertex();

		meshBuilder.Position3f(ul.Position.X, lr.Position.Y, zPos);
		meshBuilder.Color4ubv(color);
		meshBuilder.TexCoord2f(0, ul.TexCoord.X, lr.TexCoord.Y);
		meshBuilder.AdvanceVertex();

		meshBuilder.End();
		Mesh.Draw();
	}

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
		Assert(InDrawing);

		if (FullyTransparent)
			return;

		Span<SurfaceVertex> rect = stackalloc SurfaceVertex[2];
		Span<SurfaceVertex> clippedRect = stackalloc SurfaceVertex[2];
		InitVertex(ref rect[0], x0, y0, 0, 0);
		InitVertex(ref rect[1], x1, y1, 1, 1);

		if (!ClipRect(in rect[0], in rect[1], out clippedRect[0], out clippedRect[1]))
			return;

		InternalSetMaterial();
		DrawQuad(in clippedRect[0], in clippedRect[1], in DrawColor);
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
		DrawColor.SetColor(r, g, b, a);
	}

	public void DrawSetColor(in Color color) {
		DrawColor = color;
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
		return EmbeddedPanel;
	}

	public IPanel? GetPopup(int index) {
		throw new NotImplementedException();
	}

	public int GetPopupCount() {
		return 0; // PopupList.Count;
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

	}

	IPanel? RestrictedPanel;

	public void PaintTraverseEx(IPanel panel, bool paintPopups) {
		// todo: painting panels
		if (!panel.IsVisible())
			return;

		using MatRenderContextPtr renderContext = new(materials);
		bool topLevelDraw = false;
		if (InDrawing == false) {
			topLevelDraw = true;
			StartDrawing();

			renderContext.ClearBuffers(false, true, true);

			// Can comment this out later, this is just to test if ISurface is rendering.
			DrawSetColor(255, 0, 0, 255);
			DrawFilledRect(256, 256, 512, 384);

			// TODO!!!
			// renderContext.SetStencilEnable(true);
			// renderContext.SetStencilFailOperation(STENCILOPERATION_KEEP);
			// renderContext.SetStencilZFailOperation(STENCILOPERATION_KEEP);
			// renderContext.SetStencilPassOperation(STENCILOPERATION_REPLACE);
			// renderContext.SetStencilCompareFunction(STENCILCOMPARISONFUNCTION_GREATEREQUAL);
			// renderContext.SetStencilReferenceValue(0);
			// renderContext.SetStencilTestMask(0xFFFFFFFF);
			// renderContext.SetStencilWriteMask(0xFFFFFFFF);
		}

		float oldZPos = zPos;
		zPos = 0;
		if (panel == GetEmbeddedPanel()) {
			if (RestrictedPanel != null) {
				RestrictedPanel.GetParent()!.PaintTraverse(true);
			}
			else {
				panel.PaintTraverse(true);
			}
		}
		else {
			if (!paintPopups || !panel.IsPopup())
				panel.PaintTraverse(true);
		}

		if (paintPopups) {
			int popups = GetPopupCount();
			if (popups > 254)
				Warning("Too many popups! Rendering will be bad!\n");

			int stencilRef = 254;
			for (int i = popups - 1; i >= 0; --i) {
				IPanel? popupPanel = GetPopup(i);

				if (popupPanel == null)
					continue;

				if (popupPanel.IsFullyVisible())
					continue;

				if (!IsPanelUnderRestrictedPanel(popupPanel))
					continue;

				bool isTopmostPopup = popupPanel.IsTopmostPopup();

				// renderContext.SetStencilReferenceValue(isTopmostPopup ? 255 : stencilRef);
				--stencilRef;

				zPos = i / (float)popups;
				popupPanel.PaintTraverse(true);
			}
		}

		zPos = oldZPos;

		if (topLevelDraw) {
			// renderContext.SetStencilEnable(false);
			FinishDrawing();
		}
	}

	private bool IsPanelUnderRestrictedPanel(IPanel? panel) {
		if (RestrictedPanel == null)
			return true;

		while (panel != null) {
			if (panel == RestrictedPanel)
				return true;
			panel = panel.GetParent();
		}

		return false;
	}

	private void FinishDrawing() {
		EnableScissor(false);
		using MatRenderContextPtr renderContext = new(materials);
		renderContext.MatrixMode(MaterialMatrixMode.Projection);
		renderContext.PopMatrix();

		renderContext.MatrixMode(MaterialMatrixMode.Model);
		renderContext.PopMatrix();

		renderContext.MatrixMode(MaterialMatrixMode.View);
		renderContext.PopMatrix();

		Assert(InDrawing);
		InDrawing = false;
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
		Warning("MatSystemSurface.SetCursor not implemented.\n");
	}

	public void SetCursorAlwaysVisible(bool visible) {
		throw new NotImplementedException();
	}

	public void SetEmbeddedPanel(IPanel panel) {
		EmbeddedPanel = panel;
		EmbeddedPanel.RequestFocus();
	}

	public bool HandleInputEvent(in InputEvent ev) {
		return false;
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

	public void PaintSoftwareCursor() {

	}

	event VGuiPlayFunc? play;

	public void InstallPlaySoundFunc(VGuiPlayFunc func) => play += func;
}
