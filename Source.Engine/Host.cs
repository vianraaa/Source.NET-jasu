using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Engine.Client;
using Source.Engine.Server;

using static Source.Constants;

namespace Source.Engine;

public class CommonHostState
{
	public double IntervalPerTick;
}

public class Host(EngineParms host_parms, CommonHostState host_state, GameServer sv, ClientState cl, IServiceProvider services)
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

	public void RunFrame(double frameTime) {

	}

	public void PostInit() {
		var serverGameDLL = services.GetService<IServerGameDLL>();
		if (serverGameDLL != null)
			serverGameDLL.PostInit();

		var clientDLL = services.GetService<IBaseClientDLL>();
		if (clientDLL != null)
			clientDLL.PostInit();
	}

	public void ReadConfiguration() {

	}

	public bool IsSecureServerAllowed() => true;

	public void Init(bool dedicated) {
		RealTime = 0;
		IdealTime = 0;


		host_state.IntervalPerTick = DEFAULT_TICK_INTERVAL;

		var engineAPI = services.GetRequiredService<IEngineAPI>();
		var hostState = services.GetRequiredService<IHostState>();
		var CL = services.GetRequiredService<CL>();
		var ClientDLL = services.GetRequiredService<ClientDLL>();

		//engineAPI.InitSubsystem<Con>();
		//engineAPI.InitSubsystem<Cbuf>();
		//engineAPI.InitSubsystem<Cmd>();
		//engineAPI.InitSubsystem<Cvar>();
#if !SWDS
		//engineAPI.InitSubsystem<Video>();
#endif
		//engineAPI.InitSubsystem<Filter>();
#if !SWDS
		//engineAPI.InitSubsystem<Key>();
#endif
		engineAPI.InitSubsystem<Net>(dedicated);
		sv.Init(dedicated);
#if !SWDS
		if (!dedicated) {
			engineAPI.InitSubsystem<CL>();
			engineAPI.InitSubsystem<ClientDLL>();
			// engineAPI.InitSubsystem<Scr>();
			// engineAPI.InitSubsystem<Render>();
			// engineAPI.InitSubsystem<Decal>();
		}
		else {
			cl.SignOnState = SignOnState.None;
		}
#endif

#if !SWDS
		ReadConfiguration();
		// engineAPI.InitSubsystem<Sound>();
#endif
		Initialized = true;
		hostState.Init();

		PostInit();
	}
}