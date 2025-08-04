using Source.Common;

namespace Source.Engine;

public class BaseMod(IEngineAPI engineAPI, GameEngine eng, EngineParms host_parms) : IMod
{
	private bool IsServerOnly() => ((EngineAPI)engineAPI).Dedicated;

	public bool Init(string initialMod, string initialGame) {
		return true;
	}

	public IMod.Result Run() {
		IMod.Result res = IMod.Result.RunOK;

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
