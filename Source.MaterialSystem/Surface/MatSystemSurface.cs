using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Common.Commands;
using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Common.Input;
using Source.Common.Launcher;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;
using Source.Common.Utilities;
using Source.GUI.Controls;

using System.ComponentModel;
using System.Drawing;
using System.Numerics;

using static Source.Dbg;
using static System.Runtime.CompilerServices.RuntimeHelpers;
namespace Source.MaterialSystem.Surface;

public struct ScreenOverride
{
	public bool Active;
	public int X;
	public int Y;
}
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


public class MatSystemSurface : IMatSystemSurface
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

	IMesh? Mesh;
	MeshBuilder meshBuilder;
	MaterialReference White = new();

	float AlphaMultiplier;

	readonly ICommandLine CommandLine;
	readonly IFileSystem FileSystem;
	readonly FontManager FontManager;
	readonly IInputSystem InputSystem;
	readonly ClientGlobalVariables globals;
	readonly IServiceProvider services;
	readonly TextureDictionary TextureDictionary;

	public MatSystemSurface(IMaterialSystem materials, IShaderAPI shaderAPI, ICommandLine commandLine,
							ISchemeManager schemeManager, IFileSystem fileSystem, ClientGlobalVariables globals,
							IServiceProvider services, IInputSystem inputSystem, ISystem system) {
		this.materials = materials;
		this.services = services;
		this.TextureDictionary = new(materials, this);
		this.FileSystem = fileSystem;
		this.FontManager = new FontManager(materials, fileSystem, system, this);
		this.InputSystem = inputSystem;
		this.globals = globals;
		CommandLine = commandLine;

		DrawColor[0] = DrawColor[1] = DrawColor[2] = DrawColor[3] = 25; ;
		TranslateX = TranslateY = 0;
		// Scissor rect...
		AlphaMultiplier = 1;

		KeyValues vmtKeyValues = new KeyValues("UnlitGeneric");
		vmtKeyValues.SetString("$basetexture", "white");
		vmtKeyValues.SetInt("$vertexcolor", 1);
		vmtKeyValues.SetInt("$vertexalpha", 1);
		vmtKeyValues.SetInt("$ignorez", 1);
		vmtKeyValues.SetInt("$no_fullbright", 1);

		White.Init(materials, "VGUI_White", TEXTURE_GROUP_OTHER, vmtKeyValues);

		InitInput();
		InitCursors();

		DefaultEmbeddedPanel = new MatEmbeddedPanel() {
			materials = materials,
			Surface = this,
			SchemeManager = schemeManager
		};
		SetEmbeddedPanel(DefaultEmbeddedPanel);
	}

	private void InitInput() {
		EnableInput(true);
	}

	public void InternalThinkTraverse(IPanel panel) {
		panel.TraverseLevel(1);
		panel.Think();
		IList<IPanel> children = (IList<IPanel>)panel.GetChildren(); // Annoying but the internal value is an IList so hopefully this works

		for (int i = 0; i < children.Count(); i++) {
			var child = children[i];
			if (child.IsVisible()) {
				InternalThinkTraverse(child);
			}
		}

		panel.TraverseLevel(-1);
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
		BoundTexture = TextureID.INVALID;

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

		return false;
	}

	public void AddPanel(IPanel panel) {
		throw new NotImplementedException();
	}

	public void ApplyChanges() {

	}

	public void BringToFront(IPanel panel) {
		throw new NotImplementedException();
	}

	public void CalculateMouseVisible() {
		throw new NotImplementedException();
	}

	public IFont CreateFont() {
		return FontManager.CreateFont();
	}

	public TextureID CreateNewTextureID(bool procedural = false) {
		return TextureDictionary.CreateTexture(procedural);
	}

	LinkedList<IPanel> PopupList = [];

	public void CreatePopup(IPanel panel, bool minimised, bool showTaskbarIcon = true, bool disabled = false, bool mouseInput = true, bool kbInput = true) {
		if (panel.GetParent() == null)
			panel.SetParent(GetEmbeddedPanel());

		panel.SetPopup(true);
		panel.SetKeyboardInputEnabled(kbInput);
		panel.SetMouseInputEnabled(mouseInput);

		if (PopupList.Find(panel) == null) {
			PopupList.AddLast(panel);
		}
		else {
			MovePopupToFront(panel);
		}
	}

	public bool DeleteTextureByID(in TextureID id) {
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

	}

	public void DrawGetTextPos(out int x, out int y) {
		throw new NotImplementedException();
	}

	public bool DrawGetTextureFile(in TextureID id, out ReadOnlySpan<char> filename) {
		throw new NotImplementedException();
	}

	public int DrawGetTextureId(ReadOnlySpan<char> filename) {
		throw new NotImplementedException();
	}

	public void DrawGetTextureSize(in TextureID id, out int wide, out int tall) {
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

	TextureID BoundTexture;

	public void DrawSetTexture(in TextureID id) {
		if (id != BoundTexture) {
			DrawFlushText();
			BoundTexture = id;
		}
	}

	public void DrawSetTextureFile(in TextureID id, in ReadOnlySpan<char> filename, int hardwareFilter, bool forceReload) {
		TextureDictionary.BindTextureToFile(id, filename);
		DrawSetTexture(id);
	}

	public void DrawSetTextureRGBA(in TextureID id, Span<byte> rgba, int wide, int tall, int hardwareFilter, bool forceReload) {
		throw new NotImplementedException();
	}

	public void DrawTexturedRect(int x0, int y0, int x1, int y1) {
		Assert(InDrawing);

		if (DrawColor.A == 0)
			return;

		TextureDictionary.GetTextureTexCoords(in BoundTexture, out float s0, out float t0, out float s1, out float t1);

		Span<SurfaceVertex> rect = stackalloc SurfaceVertex[2];
		Span<SurfaceVertex> clippedRect = stackalloc SurfaceVertex[2];
		InitVertex(ref rect[0], x0, y0, s0, t0);
		InitVertex(ref rect[1], x1, y1, s1, t1);

		// Fully clipped?
		if (!ClipRect(rect[0], rect[1], out clippedRect[0], out clippedRect[1]))
			return;

		IMaterial? material = TextureDictionary.GetTextureMaterial(in BoundTexture);
		InternalSetMaterial(material);
		DrawQuad(in clippedRect[0], in clippedRect[1], in DrawColor);
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

	const int BASE_WIDTH = 640;
	const int BASE_HEIGHT = 480;

	public void GetProportionalBase(out int width, out int height) {
		width = BASE_WIDTH;
		height = BASE_HEIGHT;
	}

	public void GetScreenSize(out int wide, out int tall) {
		if (ScreenSizeOverride.Active) {
			wide = ScreenSizeOverride.X;
			tall = ScreenSizeOverride.Y;
			return;
		}

		IMatRenderContext renderContext = materials.GetRenderContext();
		renderContext.GetViewport(out _, out _, out wide, out tall);
	}

	public void GetTextSize(IFont font, ReadOnlySpan<char> text, out int wide, out int tall) {
		throw new NotImplementedException();
	}

	public IPanel? GetTopmostPopup() {
		throw new NotImplementedException();
	}

	ScreenOverride ScreenPosOverride;
	ScreenOverride ScreenSizeOverride;

	int[] WorkSpaceInsets = new int[4];

	public void GetWorkspaceBounds(out int x, out int y, out int wide, out int tall) {
		if (ScreenSizeOverride.Active) {
			x = y = 0;
			wide = ScreenSizeOverride.X;
			tall = ScreenSizeOverride.Y;
			return;
		}

		x = WorkSpaceInsets[0];
		y = WorkSpaceInsets[1];
		EmbeddedPanel!.GetSize(out wide, out tall);

		wide -= WorkSpaceInsets[2];
		tall -= WorkSpaceInsets[3];
	}

	public void SetWorkspaceInsets(int left, int top, int right, int bottom) {
		WorkSpaceInsets[0] = left;
		WorkSpaceInsets[1] = top;
		WorkSpaceInsets[2] = right;
		WorkSpaceInsets[3] = bottom;
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

	public bool IsTextureIDValid(in TextureID id) {
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
		PopupList.Remove(panel);
		PopupList.AddLast(panel);
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
			/*DrawSetColor(255, 0, 0, 255);
			for (int i = 0; i < 128; i++) {
				int offset = (int)(float)(Math.Sin((globals.CurTime + (i / 3d) * 0.2d)) * 128);
				DrawFilledRect(256 + offset, 64 + (i * 6), 512 + offset, 70 + (i * 6));
			}*/

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
				panel.PaintTraverse(forceRepaint: true);
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
	int BatchedCharVertCount;
	public void PopMakeCurrent(IPanel panel) {
		if (BatchedCharVertCount > 0)
			DrawFlushText();
		int top = PaintStateStack.Count() - 1;
		Assert(top >= 0);
		Assert(PaintStateStack[top].Panel == panel);
		PaintStateStack.Pop();
		if (top > 0)
			SetupPaintState(PaintStateStack[top - 1]);
	}

	readonly RefStack<PaintState> PaintStateStack = new();

	public void PushMakeCurrent(IPanel panel, bool useInsets) {
		Span<int> inSets = [0, 0, 0, 0];
		Span<int> absExtents = stackalloc int[4];
		Span<int> clipRect = stackalloc int[4];

		if (useInsets)
			panel.GetInset(out inSets[0], out inSets[1], out inSets[2], out inSets[3]);

		panel.GetAbsPos(out absExtents[0], out absExtents[1]);
		int wide, tall;
		panel.GetSize(out wide, out tall);
		absExtents[2] = absExtents[0] + wide;
		absExtents[3] = absExtents[1] + tall;

		panel.GetClipRect(out clipRect[0], out clipRect[1], out clipRect[2], out clipRect[3]);


		ref PaintState paintState = ref PaintStateStack.Push();
		paintState.Panel = panel;

		// Determine corrected top left origin
		paintState.TranslateX = inSets[0] + absExtents[0] - SurfaceExtents[0];
		paintState.TranslateY = inSets[1] + absExtents[1] - SurfaceExtents[1];

		// Setup clipping rectangle for scissoring
		paintState.ScissorLeft = clipRect[0] - SurfaceExtents[0];
		paintState.ScissorTop = clipRect[1] - SurfaceExtents[1];
		paintState.ScissorRight = clipRect[2] - SurfaceExtents[0];
		paintState.ScissorBottom = clipRect[3] - SurfaceExtents[1];

		SetupPaintState(in paintState);
	}

	private void SetupPaintState(in PaintState paintState) {
		TranslateX = paintState.TranslateX;
		TranslateY = paintState.TranslateY;
		SetScissorRect(paintState.ScissorLeft, paintState.ScissorTop, paintState.ScissorRight, paintState.ScissorBottom);
	}

	private void SetScissorRect(int left, int top, int right, int bottom) {
		Assert(left <= right);
		Assert(top <= bottom);

		if (scissorRect.Left == left && scissorRect.Right == right &&
			 scissorRect.Top == top && scissorRect.Bottom == bottom)
			return;

		scissorRect.Left = left;
		scissorRect.Top = top;
		scissorRect.Right = right;
		scissorRect.Bottom = bottom;
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

	int LastInputPollCount;
	bool AppDrivesInput;

	public void RunFrame() {
		int pollCount = InputSystem.GetPollCount();
		if (LastInputPollCount == pollCount)
			return;

		if (!AppDrivesInput && LastInputPollCount != pollCount - 1) {
			Assert(false);
			Warning("Vgui is losing input messages!\n");
		}

		LastInputPollCount = pollCount;

		if (AppDrivesInput)
			return;

		// Generate all input messages
		long eventCount = InputSystem.GetEventCount();
		foreach (var ev in InputSystem.GetEventData())
			HandleInputEvent(in ev);
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
		switch (ev.Type) {
			case InputEventType.IE_ButtonPressed: {
					ButtonCode code = (ButtonCode)ev.Data2;
					if (code.IsKeyCode())
						return InputSystem.InternalKeyCodePressed(code);


					if (code.IsMouseCode())
						return InputSystem.InternalMousePressed(code);

				}
				break;
			case InputEventType.IE_ButtonReleased: {
					ButtonCode code = (ButtonCode)ev.Data2;
					if (code.IsKeyCode())
						return InputSystem.InternalKeyCodeReleased(code);

					if (code.IsMouseCode())
						return InputSystem.InternalMouseReleased(code);
				}
				break;
			case InputEventType.IE_ButtonDoubleClicked: {
					ButtonCode code = (ButtonCode)ev.Data2;
					if (code.IsMouseCode())
						return InputSystem.InternalMouseDoublePressed(code);
				}
				break;

			case InputEventType.IE_AnalogValueChanged: {
					if ((AnalogCode)ev.Data == AnalogCode.MouseWheel)
						return InputSystem.InternalMouseWheeled(ev.Data3);
					if ((AnalogCode)ev.Data == AnalogCode.MouseXY)
						return InputSystem.InternalCursorMoved(ev.Data2, ev.Data3);
				}
				break;

			case InputEventType.Gui_KeyCodeTyped: {
					InputSystem.InternalKeyCodeTyped((ButtonCode)ev.Data);
				}
				return true;

			case InputEventType.Gui_KeyTyped: {
					InputSystem.InternalKeyTyped((ButtonCode)ev.Data);
				}
				return true;

			case InputEventType.System_Quit:
				// Referencing VGUI here will result in circular dependency...
				// so im gonna resolve it this way since System_Quit doesnt run frequently...
				services.GetRequiredService<IVGui>().Quit();
				return false;

			case InputEventType.Gui_Close:
				// FIXME: Change this so we don't stop until 'save' occurs, etc.
				services.GetRequiredService<IVGui>().Stop();
				return true;

			case InputEventType.Gui_SetCursor:
				// ActivateCurrentCursor(); // todo
				return true;

			// All of IME is todo
			case InputEventType.Gui_IMESetWindow: {
					return true;
				}

			case InputEventType.Gui_LocateMouseClick:
				InputSystem.InternalCursorMoved(ev.Data, ev.Data2);
				return true;

			case InputEventType.Gui_InputLanguageChanged:
				InputSystem.OnInputLanguageChanged();
				return true;

			case InputEventType.Gui_IMEStartComposition:
				return true;

			case InputEventType.Gui_IMEComposition:
				return true;

			case InputEventType.Gui_IMEEndComposition:
				return true;

			case InputEventType.Gui_IMEShowCandidates:
				return true;

			case InputEventType.Gui_IMEChangeCandidates:
				return true;

			case InputEventType.Gui_IMECloseCandidates:
				return true;

			case InputEventType.Gui_IMERecomputeModes:
				return true;
		}

		return false;
	}

	public bool SetFontGlyphSet(IFont font, ReadOnlySpan<char> windowsFontName, int tall, int weight, int blur, int scanlines, SurfaceFontFlags flags, int rangeMin = 0, int rangeMax = 0) {
		return FontManager.SetFontGlyphSet(font, windowsFontName, tall, weight, blur, scanlines, flags, rangeMin, rangeMax);
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
		if (RestrictedPanel != null && RestrictedPanel != childPanel && !childPanel.HasParent(RestrictedPanel))
			return false;

		return !childPanel.IsPopup();
	}

	public void SolveTraverse(IPanel panel, bool forceApplySchemeSettings = false) {
		InternalSchemeSettingsTraverse(panel, forceApplySchemeSettings);
		InternalThinkTraverse(panel);
		InternalSolveTraverse(panel);
	}

	private void InternalSchemeSettingsTraverse(IPanel panel, bool forceApplySchemeSettings) {
		panel.TraverseLevel(1);
		IList<IPanel> children = (IList<IPanel>)panel.GetChildren();

		for (int i = 0; i < children.Count(); ++i) {
			IPanel child = children[i];
			if (forceApplySchemeSettings || child.IsVisible()) {
				InternalSchemeSettingsTraverse(child, forceApplySchemeSettings);
			}
		}

		panel.PerformApplySchemeSettings();
		panel.TraverseLevel(-1);
	}

	private void InternalSolveTraverse(IPanel panel) {
		panel.TraverseLevel(1);
		panel.Solve();

		IList<IPanel> children = (IList<IPanel>)panel.GetChildren();

		for (int i = 0; i < children.Count(); ++i) {
			IPanel child = children[i];
			if (child.IsVisible()) {
				InternalSolveTraverse(child);
			}
		}

		panel.TraverseLevel(-1);
	}

	public bool SupportsFeature(SurfaceFeature feature) {
		switch (feature) {
			default:
				return false; // Most things arent implemented yet...
		}
	}

	public void SurfaceGetCursorPos(out int x, out int y) {
		throw new NotImplementedException();
	}

	public void SurfaceSetCursorPos(int x, int y) {
		throw new NotImplementedException();
	}

	public void SwapBuffers(IPanel panel) {

	}

	public void UnlockCursor() {
		throw new NotImplementedException();
	}

	public void PaintSoftwareCursor() {

	}

	event VGuiPlayFunc? play;

	public void InstallPlaySoundFunc(VGuiPlayFunc func) => play += func;

	List<string> BitmapFontFileNames = [];

	public bool AddBitmapFontFile(ReadOnlySpan<char> fontFileName) {
		bool found = false;
		found = FileSystem.FileExists(fontFileName, null);
		if (!found) {
			Msg($"Couldn't find bitmap font file '{fontFileName}'\n");
			return false;
		}
		Span<char> path = stackalloc char[MAX_PATH];
		fontFileName.CopyTo(path);

		// only add if it's not already in the list
		((ReadOnlySpan<char>)path).ToLowerInvariant(path);
		ulong sym = path.Hash();

		int i;
		for (i = 0; i < BitmapFontFileNames.Count; i++) {
			if (BitmapFontFileNames[i].Hash() == sym)
				break;
		}
		if (i < BitmapFontFileNames.Count) {
			BitmapFontFileNames.Add(new string(path));

			FileSystem.GetLocalCopy(path);
		}

		return true;
	}

	public void SetBitmapFontName(string name, ReadOnlySpan<char> fontFile) {
		throw new NotImplementedException();
	}

	public float DrawGetAlphaMultiplier() {
		return AlphaMultiplier;
	}

	public void DrawSetAlphaMultiplier(float alpha) {
		AlphaMultiplier = alpha;
	}

	public void OffsetAbsPos(ref int x, ref int y) {
		if (!ScreenPosOverride.Active)
			return;
		x += ScreenPosOverride.X;
		y += ScreenPosOverride.Y;
	}

	IWindow? window;
	public void AttachToWindow(IWindow? window, bool appDrivesInput) {
		InputDetachFromWindow(this.window);
		this.window = window;
		if (window != null) {
			InputAttachToWindow(this.window);
			AppDrivesInput = appDrivesInput;
		}
		else {
			AppDrivesInput = false;
		}
	}

	private void InputDetachFromWindow(IWindow? window) {

	}

	private void InputAttachToWindow(IWindow? window) {

	}

	public void EnableWindowsMessages(bool enabled) {

	}

	public bool GetBitmapFontName(ReadOnlySpan<char> name) {
		throw new NotImplementedException();
	}

	public void SetBitmapFontGlyphSet(IFont font, bool v, float scalex, float scaley, SurfaceFontFlags flags) {
		throw new NotImplementedException();
	}
}
