using Source.Common.Client;
using Source.Common.Networking;
using Source.Common.Server;

namespace Source.Engine.Server;

public enum ServerState {
	Dead,
	Loading,
	Active,
	Paused
}

/// <summary>
/// Base server, in SERVER
/// </summary>
public abstract class BaseServer : IServer
{
	public abstract int GetNumClients();
	public abstract int GetNumProxies();
	public abstract int GetNumFakeClients();
	public virtual int GetMaxClients() => MaxClients;
	public virtual IClient GetClient(int index) => Clients[index];
	public virtual int GetClientCount() => Clients.Count;
	public virtual int GetUDPPort() => 0;
	public abstract double GetTime();
	public virtual long GetTick() => TickCount;
	public virtual double GetTickInterval() => TickInterval;
	public abstract string GetName();
	public virtual string? GetMapName() => MapName;
	public virtual int GetSpawnCount() => SpawnCount;
	public virtual int GetNumClasses() => ServerClasses;
	public virtual int GetClassBits() => ServerClassBits;
	public abstract int GetNetStats(in float avgIn, out float avgOut);
	public abstract int GetNumPlayers();
	public abstract bool GetPlayerInfo(int clientIndex, out PlayerInfo pinfo);
	public virtual bool IsActive() => State >= ServerState.Active;
	public virtual bool IsLoading() => State == ServerState.Loading;
	public virtual bool IsDedicated() => Dedicated;
	public virtual bool IsPaused() => State == ServerState.Paused;
	public virtual bool IsMultiplayer() => MaxClients > 1;
	public virtual bool IsPausable() => false;
	public virtual bool IsHLTV() => false;
	public virtual bool IsReplay() => false;
	public abstract string? GetPassword();
	public abstract void SetPaused(bool paused);
	public abstract void SetPassword(string? password);
	public abstract void BroadcastMessage(in INetMessage msg, bool onlyActive = false, bool reliable = false);
	public abstract void DisconnectClient(IClient client, string? reason);



	public ServerState State;
	public int Socket;
	public long TickCount;
	public bool SimulatingTicks;
	public string? MapName;
	public string? MapFilename;
	public string? SkyName;
	public string? Password;

	// worldmap md5?

	// stringtables?

	// bf_write etc

	public int ServerClasses;
	public int ServerClassBits;

	protected int GetNextUserID() {
		for (int i = 0; i < Clients.Count + 1; i++) {
			int testID = (UserID + i + 1) % short.MaxValue;
			int iClient;
			for (iClient = 0; i < Clients.Count; iClient++) {
				if (Clients[iClient].GetUserID() == testID)
					break;
			}
			// No client has the ID
			if (iClient == Clients.Count)
				return testID;
		}

		Dbg.AssertMsg(false, "GetNextUserID: can't find a unique ID.");
		return UserID + 1;
	}
	public abstract bool ProcessConnectionlessPacket(ref NetPacket packet);
	public abstract bool ProcessProcessLog(ref NetPacket packet);
	public abstract bool ProcessGetChallenge(ref NetPacket packet);
	public abstract bool ProcessConnect(ref NetPacket packet);
	public abstract bool ProcessInfo(ref NetPacket packet);
	public abstract bool ProcessDetails(ref NetPacket packet);
	public abstract bool ProcessPlayers(ref NetPacket packet);
	public abstract bool ProcessRules(ref NetPacket packet);
	public abstract bool ProcessRcon(ref NetPacket packet);

	int UserID;

	protected int MaxClients;
	protected int SpawnCount;
	protected double TickInterval;

	protected List<BaseClient> Clients = [];

	protected bool Dedicated;

	protected uint CurrentRandomNonce;
	protected uint LastRandomNonce;
	protected double LastRandomNumberGenerationTime;
	protected double CPUPercent;
	protected double StartTime;
	protected double LastCPUCheckTime;

	protected int NumConnections;
}