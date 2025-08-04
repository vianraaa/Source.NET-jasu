using Microsoft.Extensions.DependencyInjection;

using Source.Common;

namespace Source.Engine;

public class BaseMod(IServiceProvider services, EngineParms host_parms) : IMod
{
	private bool IsServerOnly() => ((EngineAPI)services.GetRequiredService<IEngineAPI>()).Dedicated;

	public bool Init(string initialMod, string initialGame) {
		return true;
	}

	public IMod.Result Run() {
		IMod.Result res = IMod.Result.RunOK;
		IEngine eng = services.GetRequiredService<IEngine>();

		if (IsServerOnly()) {
			if (eng.Load(true, host_parms.BaseDir)) {
				// Dedicated stuff one day?
			}
		}
		else {

		}

		return res;
	}

	public void Shutdown() {

	}
}
