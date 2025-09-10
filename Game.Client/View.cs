using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Common.Client;
using Source.Common.Commands;
using Source.Common.Engine;
using Source.Common.GUI;
using Source.Common.MaterialSystem;
using Source.Common.Mathematics;
using Source.Engine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client;

public class ViewRender(IMaterialSystem materials, IServiceProvider services, Render engineRenderer) : IViewRender
{
	public ViewSetup? CurrentView;
	bool ForceNoVis;
	DrawFlags BaseDrawFlags;
	Frustum Frustum;

	IRenderView render;
	Base3dView? ActiveRenderer;

	public int BuildWorldListsNumber() {
		throw new NotImplementedException();
	}

	public void DisableVis() {
		throw new NotImplementedException();
	}

	public void DriftPitch() {
		throw new NotImplementedException();
	}

	public void FreezeFrame(float flFreezeTime) {
		throw new NotImplementedException();
	}

	public DrawFlags GetDrawFlags() {
		throw new NotImplementedException();
	}

	public Frustum GetFrustum() => ActiveRenderer?.GetFrustrum() ?? Frustum;

	public ref ViewSetup GetPlayerViewSetup() {
		throw new NotImplementedException();
	}

	public void GetScreenFadeDistances(out float min, out float max) {
		throw new NotImplementedException();
	}

	public IMaterial? GetScreenOverlayMaterial() {
		throw new NotImplementedException();
	}

	public ref ViewSetup GetViewSetup() {
		throw new NotImplementedException();
	}

	public void GetWaterLODParams(ref float cheapWaterStartDistance, ref float cheapWaterEndDistance) {
		throw new NotImplementedException();
	}

	public float GetZFar() {
		throw new NotImplementedException();
	}

	public float GetZNear() {
		throw new NotImplementedException();
	}

	public void Init() {
		render = services.GetRequiredService<IRenderView>();
	}

	public void LevelInit() {
		throw new NotImplementedException();
	}

	public void LevelShutdown() {
		throw new NotImplementedException();
	}

	public void OnRenderStart() {
		throw new NotImplementedException();
	}

	public void QueueOverlayRenderView(in ViewSetup view, ClearFlags clearFlags, DrawFlags whatToDraw) {
		throw new NotImplementedException();
	}

	ViewSetup View = new();
	ViewSetup View2D = new();

	StereoEye GetFirstEye() => StereoEye.Mono;
	StereoEye GetLastEye() => StereoEye.Mono;
	ref ViewSetup GetView(StereoEye eye) {
		switch (eye) {
			case StereoEye.Mono:
				return ref View;
			default:
				Assert(false);
				return ref View;
		}
	}

	public void Render(ViewRects rect) {
		using MatRenderContextPtr renderContext = new(materials);
		ref ViewRect vr = ref rect[0];

		for (StereoEye eye = GetFirstEye(); eye <= GetLastEye(); eye = eye + 1) {
			ref ViewSetup viewEye = ref GetView(eye);

			viewEye.UnscaledX = vr.X;
			viewEye.UnscaledY = vr.Y;
			viewEye.UnscaledWidth = vr.Width;
			viewEye.UnscaledHeight = vr.Height;

			ClearFlags clearFlags = ClearFlags.ClearColor | ClearFlags.ClearDepth | ClearFlags.ClearStencil;

			bool drawViewModel = true; // todo

			RenderViewInfo flags = 0;
			if (eye == StereoEye.Mono)
				flags = RenderViewInfo.DrawHUD;

			if (drawViewModel)
				flags |= RenderViewInfo.DrawViewmodel;

			RenderView(in viewEye, clearFlags, flags);
		}

		View2D.X = rect[0].X;
		View2D.Y = rect[0].Y;
		View2D.Width = rect[0].Width;
		View2D.Height = rect[0].Height;

		render.Push2DView(View2D, 0, null, GetFrustum());
		render.VGui_Paint(PaintMode.UIPanels | PaintMode.Cursor);
		render.PopView(GetFrustum());
	}

	IEngineVGui? _enginevgui;
	IEngineVGui enginevgui => _enginevgui ??= Singleton<IEngineVGui>();

	public void RenderView(in ViewSetup viewRender, ClearFlags clearFlags, RenderViewInfo whatToDraw) {
		MatRenderContextPtr renderContext;
		using (renderContext = new MatRenderContextPtr(materials)) {
			ITexture? saveRenderTarget = renderContext.GetRenderTarget();
		}

		SetupMain3DView(in viewRender, clearFlags);

		if ((whatToDraw & RenderViewInfo.DrawHUD) != 0) {
			int viewWidth = viewRender.UnscaledWidth;
			int viewHeight = viewRender.UnscaledHeight;
			int viewActualWidth = viewRender.UnscaledWidth;
			int viewActualHeight = viewRender.UnscaledHeight;
			int viewX = viewRender.UnscaledX;
			int viewY = viewRender.UnscaledY;
			int viewFramebufferX = 0;
			int viewFramebufferY = 0;
			int viewFramebufferWidth = viewWidth;
			int viewFramebufferHeight = viewHeight;
			bool clear = false;
			bool paintMainMenu = false;
			ITexture? pTexture = null;

			using (renderContext = new MatRenderContextPtr(materials)) {
				if (clear)
					renderContext.ClearBuffers(false, true, true);

				renderContext.PushRenderTargetAndViewport(pTexture, viewX, viewY, viewActualWidth, viewActualHeight);

				// TODO
				// if (pTexture != null) 
				// renderContext.OverrideAlphaWriteEnable(true, true);

				if (clear) {
					renderContext.ClearColor4ub(0, 0, 0, 0);
					renderContext.ClearBuffers(true, false);
				}
			}

			// VGui_PreRender();

			IPanel? root = enginevgui.GetPanel(VGuiPanelType.ClientDll);
			root?.SetSize(viewWidth, viewHeight);

			root = enginevgui.GetPanel(VGuiPanelType.ClientDllTools);
			root?.SetSize(viewWidth, viewHeight);
			
			// AllowCurrentViewAccess(true);

			render.VGui_Paint(PaintMode.InGamePanels);
			if (paintMainMenu)
				render.VGui_Paint(PaintMode.UIPanels | PaintMode.Cursor);

			// AllowCurrentViewAccess(false);
			// VGui_PostRender();
			// ClientMode.PostRenderVGui();
			using (renderContext = new MatRenderContextPtr(materials)) {
				if (pTexture != null) {
					// renderContext.OverrideAlphaWriteEnable(false, true);
				}

				renderContext.PopRenderTargetAndViewport();
				renderContext.Flush();
			}
		}
	}

	// Needs more work. Mostly just to clear the buffers rn
	private void SetupMain3DView(in ViewSetup viewRender, ClearFlags clearFlags) {
		using MatRenderContextPtr renderContext = new(materials);
		renderContext.ClearColor4ub(0, 0, 0, 255);
		renderContext.ClearBuffers((clearFlags & ClearFlags.ClearColor) != 0, (clearFlags & ClearFlags.ClearDepth) != 0, (clearFlags & ClearFlags.ClearStencil) != 0);
	}

	public void SetCheapWaterEndDistance(float cheapWaterEndDistance) {
		throw new NotImplementedException();
	}

	public void SetCheapWaterStartDistance(float cheapWaterStartDistance) {
		throw new NotImplementedException();
	}

	public void SetScreenOverlayMaterial(IMaterial? pMaterial) {
		throw new NotImplementedException();
	}

	public bool ShouldDrawBrushModels() {
		throw new NotImplementedException();
	}

	public void Shutdown() {
		throw new NotImplementedException();
	}

	public void StartPitchDrift() {

	}

	public void StopPitchDrift() {

	}

	public bool UpdateShadowDepthTexture(ITexture? pRenderTarget, ITexture? pDepthTexture, in ViewSetup shadowView) {
		throw new NotImplementedException();
	}

	public void WriteSaveGameScreenshot(ReadOnlySpan<char> pFilename) {
		throw new NotImplementedException();
	}

	public void WriteSaveGameScreenshotOfSize(ReadOnlySpan<char> pFilename, int width, int height, bool bCreatePowerOf2Padded = false, bool bWriteVTF = false) {
		throw new NotImplementedException();
	}
}