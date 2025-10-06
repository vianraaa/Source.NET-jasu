namespace Source.Common;


public class GlobalVarsBase
{
	private bool isClient;
	private int timestampNetworkingBase;
	private int timestampRandomizeWindow;

	public GlobalVarsBase(bool isClient) {
		this.isClient = isClient;
		this.timestampNetworkingBase = 100;
		this.timestampRandomizeWindow = 32;
	}
	public bool IsClient => isClient;
	public long GetNetworkBase(long tick, int entity) {
		long entityMod = entity % timestampRandomizeWindow;
		long baseTick = timestampNetworkingBase * ((tick - entityMod) / timestampNetworkingBase);
		return baseTick;
	}

	public double RealTime;
	public long FrameCount;
	public double AbsoluteFrameTime;
	public double CurTime;
	public double FrameTime;
	public int MaxClients;
	public long TickCount;
	public double IntervalPerTick;
	public double InterpolationAmount;
	public int SimTicksThisFrame;
	public int NetworkProtocol;
}
public class GlobalVars(bool isClient) : GlobalVarsBase(isClient)
{
	public string? MapName;
	public int MapVersion;
	public string? StartSpot;
	public MapLoadType LoadType;
	public bool MapLoadFailed;
	public bool Deathmatch;
	public bool Coop;
	public bool Teamplay;
	public int MaxEntities;
	public int ServerCount;
}

public class ServerGlobalVariables() : GlobalVars(false);
public class ClientGlobalVariables() : GlobalVarsBase(true);