using Microsoft.Extensions.DependencyInjection;

using Source;
using Source.Common.GameUI;
using Source.Common.GUI;

namespace Game.UI;

[EngineComponent]
public static class SourceDllMain
{
	public static void Link(IServiceCollection services) {
		services.AddSingleton<IGameConsole, GameConsole>();
	}
}
