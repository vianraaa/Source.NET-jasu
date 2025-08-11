using Microsoft.Extensions.DependencyInjection;

using Raylib_cs;

using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public class MaterialSystem : IMaterialSystem
{
	nint graphics;
	public static void DLLInit(IServiceCollection services) {

	}

	public void ModInit() {

	}

	public void ModShutdown() {

	}

	public bool InitializeGraphics(nint graphics, int width, int height) {
		this.graphics = graphics;
		Rlgl.GlInit(width, height);
		Rlgl.Viewport(0, 0 , width, height);
		Rlgl.MatrixMode(MatrixMode.Projection);
		Rlgl.LoadIdentity();
		Rlgl.Ortho(0, width, height, 0, 0, 1);
		Rlgl.MatrixMode(MatrixMode.ModelView);
		Rlgl.LoadIdentity();
		Rlgl.ClearColor(255, 255, 255, 255);
		Rlgl.EnableDepthTest();

		Rlgl.ClearScreenBuffers();
		return true;
	}
}