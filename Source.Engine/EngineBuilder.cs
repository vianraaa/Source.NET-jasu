using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Common.Client;
using Source.Common.Commands;
using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Common.GameUI;
using Source.Common.MaterialSystem;
using Source.Common.Networking;
using Source.Common.Server;
using Source.Engine.Client;
using Source.Engine.Server;

using System.Reflection;

namespace Source.Engine;

/// <summary>
/// Builds a capable engine instance and provides EngineAPI to interact with it.
/// </summary>
public class EngineBuilder(ICommandLine cmdLine) : ServiceCollection
{
	public EngineBuilder MarkInterface<I, T>() where T : class, I where I : class {
		this.AddSingleton<I>(x => x.GetRequiredService<T>());
		return this;
	}

	/// <summary>
	/// Force loads an assembly.
	/// </summary>
	/// <param name="assemblyName"></param>
	/// <returns></returns>
	public EngineBuilder WithAssembly(string assemblyName)  {
		if (!assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
			assemblyName += ".dll";

		if (!Path.IsPathFullyQualified(assemblyName))
			assemblyName = Path.Combine(AppContext.BaseDirectory, assemblyName);

		Assembly.LoadFrom(assemblyName);

		return this;
	}
	public EngineBuilder WithComponent<I, T>() where T : class, I where I : class {
		PreInject<T>(this);
		this.AddSingleton<I, T>();
		return this;
	}

	static void PreInject<T>(IServiceCollection services) {
		Type t = typeof(T);
		var preInject = t.GetMethod("DLLInit", BindingFlags.Public | BindingFlags.Static)?.CreateDelegate<PreInject>();
		if (preInject != null) 
			preInject(services);
	}

	public EngineBuilder WithGameUIDLL<UIDLL>() where UIDLL : class, IGameUI {
		PreInject<UIDLL>(this);
		WithComponent<IGameUI, UIDLL>();
		return this;
	}

	public EngineBuilder WithClientDLL<ClDLL>() where ClDLL : class, IBaseClientDLL {
		PreInject<ClDLL>(this);
		WithComponent<IBaseClientDLL, ClDLL>();
		return this;
	}

	public EngineBuilder WithGameDLL<SvDLL>() where SvDLL : class, IServerGameDLL {
		PreInject<SvDLL>(this);
		WithComponent<IServerGameDLL, SvDLL>();
		return this;
	}

	/// <summary>
	/// Finalizes the dependency injection setup and returns a finalized <see cref="IServiceProvider"/> (as an <see cref="EngineAPI"/>).
	/// </summary>
	/// <param name="dedicated"></param>
	/// <returns></returns>
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
		this.AddSingleton<Common>();
		this.AddSingleton<Util>();
		this.AddSingleton<Scr>();
		this.AddSingleton<View>();
		this.AddSingleton<Render>();
		this.AddSingleton<FileSystem>();
		this.AddSingleton<CvarUtilities>();
		this.AddSingleton<RenderUtils>();
		// Engine components that we provide.
		this.AddSingleton<ICvar, Cvar>((services) => services.GetRequiredService<Cvar>());
		this.AddSingleton<ICvarQuery, DefaultCvarQuery>();
		this.AddSingleton<IHostState, HostState>();
		this.AddSingleton<CommonHostState>();
		this.AddSingleton<EngineParms>();
		this.AddSingleton<ClientDLL>();
		this.AddSingleton<IVideoMode, VideoMode_MaterialSystem>();
		this.AddSingleton<MaterialSystemConfig>();
		this.AddSingleton<IMod, BaseMod>();
		this.AddSingleton<IGame, Game>();
		// Client state and server state singletons
		this.AddSingleton<ClientState>();
		this.AddSingleton<GameServer>();
		this.AddSingleton<ClientGlobalVariables>();
		this.AddSingleton<Shader>();
		this.AddSingleton<ServerGlobalVariables>();
		// Engine VGUI and how to read it later
		this.AddSingleton<EngineVGui>();
		this.AddSingleton<IEngineVGuiInternal, EngineVGui>(x => x.GetRequiredService<EngineVGui>());
		this.AddSingleton<IEngineVGui, EngineVGui>(x => x.GetRequiredService<EngineVGui>());
		// These interfaces go to client and game dll's
		this.AddSingleton<IEngineClient, EngineClient>();
		this.AddSingleton<IEngineServer, EngineServer>();
		// We have to tell the dependency injection system how to resolve parent classes ourselves.
		this.AddSingleton<BaseClientState>(x => x.GetRequiredService<ClientState>());
		this.AddSingleton<BaseServer>(x => x.GetRequiredService<GameServer>());
		// Singleton implementations of IEngineAPI and IEngine
		this.AddSingleton<IEngineAPI, EngineAPI>();
		this.AddSingleton<IEngine, GameEngine>();

		List<Type> wantsInjection = [];
		object?[]? linkInput = [this];
		foreach(var assembly in ReflectionUtils.GetAssemblies()) {
			// This allows a type to define a class named SourceDllMain, with a static void Link(IServiceCollection),
			// which allows a loaded assembly to insert whatever it wants into the DI system before the provider is
			// fully built.
			Type? sourceDLL = assembly.GetTypes().FirstOrDefault(x => x.Name == "SourceDllMain");
			sourceDLL
				?.GetMethod("Link", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				?.Invoke(null, linkInput);

			// This checks for any classes with the MarkForDependencyInjection attribute.
			// They are then injected into the service collection.
			foreach(var typeKVP in assembly.GetTypesWithAttribute<EngineComponentAttribute>()) {
				if (typeKVP.Key.IsAbstract && typeKVP.Key.IsSealed)
					continue;

				this.AddSingleton(typeKVP.Key);
			}
		}

		// Everything else should be provided by the launcher!
		ServiceProvider provider = this.BuildServiceProvider();
		EngineAPI api = (EngineAPI)provider.GetRequiredService<IEngineAPI>();
		api.Dedicated = dedicated;
		return api;
	}
}