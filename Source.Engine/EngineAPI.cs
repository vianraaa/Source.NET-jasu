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

	Lazy<IEngine> engR = new(provider.GetRequiredService<IEngine>);

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
}