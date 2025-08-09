using Microsoft.Extensions.DependencyInjection;

using Source.Common.ShaderAPI;

namespace Source.ShaderAPI.GL46;

public class ShaderAPI_GL46 : IShaderAPI
{
	public static void PreInit(IServiceCollection services) {
		// Add our required dependencies.
		// EngineBuilder will run PreInit
		services.AddSingleton<IShaderDynamicAPI, ShaderAPI_GL46>(x => x.GetRequiredService<ShaderAPI_GL46>());
		services.AddSingleton<IShaderDevice, ShaderDevice_GL46>();
	}
}