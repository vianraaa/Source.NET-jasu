using Microsoft.Extensions.DependencyInjection;
using Source.Common.GUI;
using Source.Common.Launcher;
namespace Source.GUI;

[EngineComponent]
public static class SourceDllMain
{
	public static void Link(IServiceCollection services) {
		services.AddSingleton<IVGui, VGui>();
		services.AddSingleton<ISchemeManager, SchemeManager>();
	}
}

public class VGui(ISurface surface, ISystem system) : IVGui {
	public void Quit() {

	}

	public void RunFrame() {
		surface.RunFrame();
		system.RunFrame();
		surface.SolveTraverse(surface.GetEmbeddedPanel());
		surface.ApplyChanges();
	}

	public void Stop() {

	}
}