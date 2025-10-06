using Microsoft.Extensions.DependencyInjection;

using Source.Common.GUI;

namespace Source.GUI.Controls;

[EngineComponent]
public static class SourceDllMain
{
	public static void Link(IServiceCollection services) {
		services.AddSingleton<AnimationController>();
		services.AddSingleton<IAnimationController>(x => x.GetRequiredService<AnimationController>());
	}
}
