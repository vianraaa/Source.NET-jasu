using Microsoft.Extensions.DependencyInjection;
using Source.Common.GUI;
namespace Source.GUI;

[EngineComponent]
public static class SourceDllMain
{
	public static void Link(IServiceCollection services) {
		services.AddSingleton<ISchemeManager, SchemeManager>();
	}
}