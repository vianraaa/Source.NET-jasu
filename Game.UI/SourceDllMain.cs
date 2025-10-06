using Microsoft.Extensions.DependencyInjection;

using Source;
using Source.Common.GameUI;

namespace Game.UI;

[EngineComponent]
public static class SourceDllMain
{
	public static void Link(IServiceCollection services) {
		services.AddSingleton<IGameConsole, GameConsole>();
	}
}
