using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Common.Commands;
using Source.Common.Engine;
using Source.Common.Filesystem;

using System.Reflection;

using System.Runtime.CompilerServices;

using static Source.Dbg;

namespace Source.Engine;


public class EngineAPI(IServiceProvider provider, COM COM, IFileSystem fileSystem, Sys Sys) : IEngineAPI, IDisposable
{
	public bool Dedicated;

	public void Dispose() {
		((IDisposable)provider).Dispose();
		GC.SuppressFinalize(this);
	}

	StartupInfo startupInfo;

	Lazy<IEngine> engR = new(provider.GetRequiredService<IEngine>);

	public IEngineAPI.Result RunListenServer() {
		IEngineAPI.Result result = IEngineAPI.Result.RunOK;
		IMod mod = provider.GetRequiredService<IMod>();
		if (mod.Init(startupInfo.InitialMod, startupInfo.InitialGame)) {
			Msg("Source.NET: ready\n");
			result = (IEngineAPI.Result)mod.Run();
			mod.Shutdown();
		}

		return result;
	}

	public void SetStartupInfo(in StartupInfo info) {
		startupInfo = info;
		Sys.TextMode = info.TextMode;
		COM.InitFilesystem(info.InitialMod);
	}

	public IEngineAPI.Result Run() {
		ConVar_Register();
		return RunListenServer();
	}

	public object? GetService(Type serviceType) => provider.GetService(serviceType);

	public bool InEditMode() => false;
	public void PumpMessages() {

	}
	public void PumpMessagesEditMode(bool idle, long idleCount) => throw new NotImplementedException();
	public void ActivateEditModeShaders(bool active) { }

	public bool MainLoop() {
		bool idle = true;
		long idleCount = 0;
		while (true) {
			IEngine eng = engR.Value;
			switch (eng.GetQuitting()) {
				case IEngine.Quit.NotQuitting:
					if (!InEditMode())
						PumpMessages();
					else
						PumpMessagesEditMode(idle, idleCount);

					if (!InEditMode()) {
						ActivateEditModeShaders(false);
						eng.Frame();
						ActivateEditModeShaders(true);
					}

					if (InEditMode()) {
						// hammer.RunFrame()? How would this work? todo; learn how editmode works.
					}
					break;
				case IEngine.Quit.ToDesktop: return false;
				case IEngine.Quit.Restart: return true;
			}
		}
	}

	static IEnumerable<Type> safeTypeGet(Assembly assembly) {
		IEnumerable<Type?> types;
		try {
			types = assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException e) {
			types = e.Types;
		}
		foreach (var t in types.Where(t => t != null && t.Assembly.GetName().Name != "Steamworks.NET"))
			yield return t!;
	}
	void ConVar_Register() {
		ICvar cvar = this.GetRequiredService<ICvar>();
		var types = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(safeTypeGet);

		foreach (var type in types) {
			cvar.SetAssemblyIdentifier(type.Assembly);

			var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

			// If any props/fields exist, run the cctor so we can pull out static cvars/concmds
			if (props.Any() || fields.Any())
				RuntimeHelpers.RunClassConstructor(type.TypeHandle);

			foreach (var prop in props.Where(x => x.PropertyType == typeof(ConVar))) {
				var getMethod = prop.GetGetMethod();

				if (getMethod == null)
					continue;

				if (getMethod.IsStatic) {
					// Pull a static reference out to link
					ConVar cv = (ConVar)getMethod.Invoke(null, null)!;
					cv.SetName(prop.Name);
					cvar.RegisterConCommand(cv);
				}
				else if (type != typeof(ConVar)) {
					object? instance = DetermineInstance(type);
					ConVar cv = (ConVar)getMethod.Invoke(instance, null)!;
					cv.SetName(prop.Name);
					cvar.RegisterConCommand(cv);
				}
			}

			foreach (var field in fields.Where(x => x.FieldType == typeof(ConVar))) {
				if (field.IsStatic) {
					// Pull a static reference out to link
					ConVar cv = (ConVar)field.GetValue(null)!;
					cv.SetName(field.Name);
					cvar.RegisterConCommand(cv);
				}
				else if (type != typeof(ConVar)) {
					object? instance = DetermineInstance(type);
					ConVar cv = (ConVar)field.GetValue(instance)!;
					cv.SetName(field.Name);
					cvar.RegisterConCommand(cv);
				}
			}

			foreach (var method in methods.Where(x => x.GetCustomAttribute<ConCommandAttribute>() != null)) {
				ConCommandAttribute attribute = method.GetCustomAttribute<ConCommandAttribute>()!; // ^^ never null!
				object? instance = method.IsStatic ? null : DetermineInstance(type);

				// Lets see if we can find a FnCommandCompletionCallback...
				FnCommandCompletionCallback? completionCallback = null;
				if (attribute.AutoCompleteMethod != null)
					type.TryExtractMethodDelegate(instance, x => x.Name == attribute.AutoCompleteMethod, out completionCallback);

				// Construct a new ConCommand
				string cmdName = attribute.Name ?? method.Name;
				ConCommand cmd;

				if (method.TryToDelegate<FnCommandCallbackVoid>(instance, out var callbackVoid))
					cmd = new(cmdName, callbackVoid, attribute.HelpText, attribute.Flags, completionCallback);
				else if (method.TryToDelegate<FnCommandCallback>(instance, out var callback))
					cmd = new(cmdName, callback, attribute.HelpText, attribute.Flags, completionCallback);
				else if (method.TryToDelegate<FnCommandCallbackSourced>(instance, out var callbackSourced))
					cmd = new(cmdName, callbackSourced, attribute.HelpText, attribute.Flags, completionCallback);
				else
					throw new ArgumentException("Cannot dynamically produce ConCommand with the arguments we were given");

				cvar.RegisterConCommand(cmd);
			}
		}
	}

	private object? DetermineInstance(Type type) {
		// We need to find an appropriate instance of the type in question.
		// If it's not registered with the dependency injection framework, then we can't really link anything
		// Should've made it static...
		object? instance = GetService(type);

		// As a last resort, try pulling at interface types.
		if (instance == null) {
			foreach (var iface in type.GetInterfaces()) {
				instance = GetService(iface);
				if (instance != null)
					return instance;
			}
		}
		else
			return instance;

		throw new DllNotFoundException("Cannot find an instance of the type...");
	}
}