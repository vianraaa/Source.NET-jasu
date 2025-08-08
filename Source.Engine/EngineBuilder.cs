using Microsoft.Extensions.DependencyInjection;

using Nucleus.FileSystem;

using Source.Common;
using Source.Common.Client;
using Source.Common.Commands;
using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Common.Networking;
using Source.Common.Server;
using Source.Engine.Client;
using Source.Engine.Server;

using System.Reflection;

namespace Source.Engine;

/// <summary>
/// Builds a capable engine instance and provides EngineAPI to interact with it.
/// </summary>
public class EngineBuilder(ICommandLine cmdLine, string basedir, bool textmode) : ServiceCollection
{
	public EngineBuilder Add<I, T>() where T : class, I where I : class {
		this.AddSingleton<I, T>();
		return this;
	}

	static void PreInject<T>(IServiceCollection services) {
		Type t = typeof(T);
		var preInject = t.GetMethod("DLLInit", BindingFlags.Public | BindingFlags.Static)?.CreateDelegate<PreInject>();
		if (preInject != null) 
			preInject(services);
	}

	public EngineBuilder WithClientDLL<ClDLL>() where ClDLL : class, IBaseClientDLL {
		PreInject<ClDLL>(this);
		Add<IBaseClientDLL, ClDLL>();
		return this;
	}

	public EngineBuilder WithGameDLL<SvDLL>() where SvDLL : class, IServerGameDLL {
		PreInject<SvDLL>(this);
		Add<IServerGameDLL, SvDLL>();
		return this;
	}

	public EngineAPI Build(bool dedicated) {
		// We got the ICommandLine from EngineBuilder, insert it into the app system
		this.AddSingleton(cmdLine);
		// temp
		Dbg.SpewActivate(Dbg.GROUP_DEVELOPER, 1);
		Dbg.SpewActivate(Dbg.GROUP_CONSOLE, 1);
		// Internal methods. These are class instances for better restart
		// support, and I feel like every time I try this, I end up getting
		// "static creep" where I start to revert like a primate into using
		// static singletons/god classes - if we're gonna use DI we might as
		// well go all the way with it...
		this.AddSingleton<CL>();
		this.AddSingleton<SV>();
		this.AddSingleton<Sys>();
		this.AddSingleton<Host>();
		this.AddSingleton<Net>();
		this.AddSingleton<Cbuf>();
		this.AddSingleton<Cmd>();
		this.AddSingleton<Con>();
		this.AddSingleton<Cvar>();
		this.AddSingleton<COM>();
		this.AddSingleton<Util>();
		this.AddSingleton<FileSystem>();
		this.AddSingleton<CvarUtilities>();
		// Engine components, individually...
		this.AddSingleton<IFileSystem, BaseFileSystem>();
		// Engine components that we provide.
		this.AddSingleton<ICvar, Cvar>((services) => services.GetRequiredService<Cvar>());
		this.AddSingleton<IHostState, HostState>();
		this.AddSingleton<CommonHostState>();
		this.AddSingleton<EngineParms>();
		this.AddSingleton<ClientDLL>();
		this.AddSingleton<IMod, BaseMod>();
		// Client state and server state singletons
		this.AddSingleton<ClientState>();
		this.AddSingleton<GameServer>();
		this.AddSingleton<ClientGlobalVariables>();
		this.AddSingleton<ServerGlobalVariables>();
		// We have to tell the dependency injection system how to resolve parent classes ourselves.
		this.AddSingleton<BaseClientState>(x => x.GetRequiredService<ClientState>());
		this.AddSingleton<BaseServer>(x => x.GetRequiredService<GameServer>());
		// Singleton implementations of IEngineAPI and IEngine
		this.AddSingleton<IEngineAPI, EngineAPI>();
		this.AddSingleton<IEngine, GameEngine>();
		// Everything else should be provided by the launcher!
		ServiceProvider provider = this.BuildServiceProvider();
		EngineAPI api = (EngineAPI)provider.GetRequiredService<IEngineAPI>();
		api.Dedicated = dedicated;
		return api;
	}
}