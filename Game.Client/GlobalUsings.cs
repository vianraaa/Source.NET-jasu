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

	/// <summary>
	/// Sets client globals for the client state.
	/// </summary>
	public static void InitClientGlobals() {
		gpGlobals = Singleton<ClientGlobalVariables>();
		engine = Singleton<IEngineClient>();
		cl_entitylist = Singleton<ClientEntityList>();
	}
}