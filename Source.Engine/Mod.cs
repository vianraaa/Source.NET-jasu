using Microsoft.Extensions.DependencyInjection;

using Source.Common.Engine;

namespace Source.Engine;

public class BaseMod(IServiceProvider services, EngineParms host_parms, SV SV) : IMod
{
	private bool IsServerOnly(IEngineAPI api) => ((EngineAPI)api).Dedicated;

	public bool Init(string initialMod, string initialGame) {
		return true;
	}

	public IMod.Result Run() {
		IMod.Result res = IMod.Result.RunOK;
		IEngine eng = services.GetRequiredService<IEngine>();
		IEngineAPI engineAPI = services.GetRequiredService<IEngineAPI>();

		if (IsServerOnly(engineAPI)) {
			if (eng.Load(true, host_parms.BaseDir)) {
				// Dedicated stuff one day?
			}
		}
		else {
			eng.SetQuitting(IEngine.Quit.NotQuitting);

			if(eng.Load(false, host_parms.BaseDir)) {
#if !SWDS
				if (engineAPI.MainLoop())
					res = IMod.Result.RunRestart;

				eng.Unload();
#endif

				SV.ShutdownGameDLL();
			}
		}

		return res;
	}

	public void Shutdown() {

	}
}
