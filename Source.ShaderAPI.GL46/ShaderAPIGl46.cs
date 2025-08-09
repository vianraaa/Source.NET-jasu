using Microsoft.Extensions.DependencyInjection;

using Source.Common.ShaderAPI;

namespace Source.ShaderAPI.Gl46;

public class ShaderAPIGl46 : IShaderAPI
{
	public static void DLLInit(IServiceCollection services) {
		// Add our required dependencies.
		// EngineBuilder will run PreInit
		services.AddSingleton<IShaderDynamicAPI, ShaderAPIGl46>(x => x.GetRequiredService<ShaderAPIGl46>());
		services.AddSingleton<IShaderDevice, ShaderDeviceGl46>();
	}
}