
using Source.Common.MaterialSystem;
using Source.Engine.Client;

namespace Source.Engine;

public class View(Host Host, IEngineVGuiInternal EngineVGui, IMaterialSystem materials, Render EngineRenderer, Shader Shader, ClientState cl)
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
			// todo!
		}
	}
}
