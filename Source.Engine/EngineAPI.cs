using Microsoft.Extensions.DependencyInjection;

using Source.Common;

namespace Source.Engine;


public class EngineAPI(IServiceProvider provider) : IEngineAPI, IDisposable
{
	public bool Dedicated;

	public void Dispose() {
		((IDisposable)provider).Dispose();
		GC.SuppressFinalize(this);
	}

	StartupInfo startupInfo;

	public IEngineAPI.Result RunListenServer() {
		IEngineAPI.Result result = IEngineAPI.Result.RunOK;
		IMod mod = provider.GetRequiredService<IMod>();
		if (mod.Init(startupInfo.InitialMod, startupInfo.InitialGame)) {
			result = (IEngineAPI.Result)mod.Run();
			mod.Shutdown();
		}

		return result;
	}

	public void SetStartupInfo(in StartupInfo info) {
		startupInfo = info;
	}

	public IEngineAPI.Result Run() {
		return RunListenServer();
	}

	public object? GetService(Type serviceType) => provider.GetService(serviceType);
}