
using Source.Common;
using Source.Common.Client;
using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.Common.Mathematics;
using Source.Engine.Client;

using System.Drawing;

namespace Source.Engine;

public class View(Host Host, IEngineVGuiInternal EngineVGui, IMaterialSystem materials,
	Render EngineRenderer, Shader Shader, ClientState cl, IBaseClientDLL ClientDLL, IVideoMode videomode)
{
	public void RenderGuiOnly() {
		materials.BeginFrame(Host.FrameTime);
		EngineVGui.Simulate();
		EngineRenderer.FrameBegin();
		RenderGuiOnly_NoSwap();
		EngineRenderer.FrameEnd();
		materials.EndFrame();
		Shader.SwapBuffers();
	}

	private void RenderGuiOnly_NoSwap() {
		using MatRenderContextPtr renderContext = new(materials);
		renderContext.ClearBuffers(true, true);

		EngineVGui.Paint(PaintMode.UIPanels | PaintMode.Cursor);
	}

	internal void RenderView() {
		bool canRenderWorld = cl.IsActive();
		if (!canRenderWorld) {
			RenderGuiOnly_NoSwap();
		}
		else {
			ViewRects screenrect = videomode.GetClientViewRect();
			ClientDLL.View_Render(screenrect);
		}
	}
}


public class RenderView : IRenderView
{
	public void PopView(Frustum frustumPlanes) {
		throw new NotImplementedException();
	}

	public void Push2DView(ViewSetup view, int flags, ITexture? renderTarget, Frustum frustumPlanes) {
		throw new NotImplementedException();
	}

	public void VGui_Paint(PaintMode mode) {
		throw new NotImplementedException();
	}
}