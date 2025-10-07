using Microsoft.Extensions.DependencyInjection;

using Source.Common.GUI;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;
using Source.MaterialSystem;

namespace Source.ShaderAPI.Gl46;

[EngineComponent]
public static class SourceDllMain
{
	public static void Link(IServiceCollection services) {
		services.AddSingleton<ShaderAPIGl46>();
		services.AddSingleton<IShaderAPI>(x => x.GetRequiredService<ShaderAPIGl46>());
		services.AddSingleton<IShaderDevice>(x => x.GetRequiredService<ShaderAPIGl46>());
		services.AddSingleton<IMeshMgr, MeshMgr>();
		services.AddSingleton<IShaderSystem, ShaderSystem>();
		services.AddSingleton<MaterialSystem_Config>();
		services.AddSingleton<MeshMgr>();
	}
}
