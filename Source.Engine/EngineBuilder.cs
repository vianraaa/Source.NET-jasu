using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Engine.Client;
using Source.Engine.Server;

namespace Source.Engine;

/// <summary>
/// Builds a capable engine instance and provides EngineAPI to interact with it.
/// </summary>
public class EngineBuilder : ServiceCollection
{
	public EngineBuilder Add<I, T>() where T : class, I where I : class {
		this.AddSingleton<I, T>();
		return this;
	}
	public EngineAPI Build(bool dedicated) {
		// Internal methods. These are class instances for better restart
		// support, and I feel like every time I try this, I end up getting
		// "static creep" where I start to revert like a primate into using
		// static singletons/god classes - if we're gonna use DI we might as
		// well go all the way with it...
		this.AddSingleton<CL>();
		this.AddSingleton<SV>();
		this.AddSingleton<Sys>();
		this.AddSingleton<Host>();
		this.AddSingleton<CommonHostState>();
		this.AddSingleton<EngineParms>();
		// Client state and server state singletons
		this.AddSingleton<ClientState>();
		this.AddSingleton<GameServer>();
		// Singleton implementations of IEngineAPI and IEngine
		this.AddSingleton<IEngineAPI, EngineAPI>();
		this.AddSingleton<IEngine, GameEngine>();
		// Everything else should be provided by the launcher!
		ServiceProvider provider = this.BuildServiceProvider();
		EngineAPI api = provider.GetRequiredService<EngineAPI>();
		api.Dedicated = dedicated;
		return api;
	}
}