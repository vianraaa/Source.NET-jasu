using Microsoft.Extensions.DependencyInjection;

using Source.Common.GUI;
using Source.Common.MaterialSystem;
using Source.MaterialSystem;
using Source.MaterialSystem.Surface;

namespace Source.ShaderAPI.Gl46;

[EngineComponent]
public static class SourceDllMain
{
	public static void Link(IServiceCollection services) {
		services.AddSingleton<MatSystemSurface>();
		services.AddSingleton<IMatSystemSurface>(x => x.GetRequiredService<MatSystemSurface>());
		services.AddSingleton<ISurface>(x => x.GetRequiredService<MatSystemSurface>());
		services.AddSingleton<ITextureManager, TextureManager>();
		services.AddSingleton<IMaterialSystemHardwareConfig, HardwareConfig>();
		services.AddSingleton<MaterialSystem_Config>();
	}
}
