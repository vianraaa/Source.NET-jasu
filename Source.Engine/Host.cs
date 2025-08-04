using Source.Engine.Client;
using Source.Engine.Server;

namespace Source.Engine;

public class CommonHostState
{
	public double IntervalPerTick;
}

public class Host(EngineParms host_parms, CommonHostState host_state, GameServer sv, ClientState cl)
{
	public int TimeToTicks(float dt) => (int)(0.5f + (float)dt / (float)host_state.IntervalPerTick);
	public float TicksToTime(int dt) => (float)host_state.IntervalPerTick * (float)dt;

	public string GetCurrentMod() => host_parms.Mod;
	public string GetCurrentGame() => host_parms.Game;
	public string GetBaseDirectory() => host_parms.BaseDir;

	public bool Initialized;
	public double FrameTime;
	public double FrameTimeUnbounded;
	public double FrameTimeStandardDeviation;
	public double RealTime;
	public double IdealTime;
	public double NextTick;
	public double[] JitterHistory = new double[128];
	public uint JitterHistoryPos;
	public long FrameCount;
	public int HunkLevel;

	public bool IsSinglePlayerGame() {
		if (sv.IsActive())
			return !sv.IsMultiplayer();
		else
			return cl.MaxClients == 1;
	}
}