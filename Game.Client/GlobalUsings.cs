global using EHANDLE = Game.Shared.Handle<Game.Client.C_BaseEntity>;
global using static Game.Client.ClientGlobals;
using Source.Common;
using Source.Common.Client;

namespace Game.Client;

public static class ClientGlobals
{
	public static ClientGlobalVariables gpGlobals { get; private set; }
	public static IEngineClient engine { get; private set; }
	public static ClientEntityList cl_entitylist { get; private set; }
	public static double TICK_INTERVAL => gpGlobals.IntervalPerTick;

	public static TimeUnit_t TICKS_TO_TIME(int t) => TICK_INTERVAL * t;
	public static int TIME_TO_TICKS(TimeUnit_t dt) => (int)(0.5f + dt / TICK_INTERVAL);


	/// <summary>
	/// Sets client globals for the client state.
	/// </summary>
	public static void InitClientGlobals() {
		gpGlobals = Singleton<ClientGlobalVariables>();
		engine = Singleton<IEngineClient>();
		cl_entitylist = Singleton<ClientEntityList>();
	}
}