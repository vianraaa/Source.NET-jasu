using Source.Common.Client;
using Source.Common.Networking;

namespace Source.Common.Server;

public interface IServer : IConnectionlessPacketHandler
{
	/// <summary>
	/// Current number of clients
	/// </summary>
	/// <returns></returns>
	public int GetNumClients();
	/// <summary>
	/// Current number of attached HLTV proxies (i am never doing this!)
	/// </summary>
	/// <returns></returns>
	public int GetNumProxies();
	/// <summary>
	/// Number of fake client/bots
	/// </summary>
	/// <returns></returns>
	public int GetNumFakeClients();
	/// <summary>
	/// Current client limit
	/// </summary>
	/// <returns></returns>
	public int GetMaxClients();
	/// <summary>
	/// Interface to client
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public IClient GetClient(int index);
	/// <summary>
	/// Number of client slots, used & unused
	/// </summary>
	/// <returns></returns>
	public int GetClientCount();
	/// <summary>
	/// Current used UDP port
	/// </summary>
	/// <returns></returns>
	public int GetUDPPort();
	/// <summary>
	/// Game world time
	/// </summary>
	/// <returns></returns>
	public double GetTime();
	/// <summary>
	/// Game world tick
	/// </summary>
	/// <returns></returns>
	public long GetTick();
	/// <summary>
	/// Tick interval in seconds
	/// </summary>
	/// <returns></returns>
	public double GetTickInterval();
	/// <summary>
	/// Public server name
	/// </summary>
	/// <returns></returns>
	public string GetName();
	/// <summary>
	/// Current map name (with .bsp at the end)
	/// </summary>
	/// <returns></returns>
	public string? GetMapName();
	public int GetSpawnCount();
	public int GetNumClasses();
	public int GetClassBits();
	/// <summary>
	/// Total net in/out in bytes/sec
	/// </summary>
	/// <param name="avgIn"></param>
	/// <param name="avgOut"></param>
	/// <returns></returns>
	public int GetNetStats(in float avgIn, out float avgOut);
	public int GetNumPlayers();
	public bool GetPlayerInfo(int clientIndex, out PlayerInfo pinfo);

	public bool IsActive();
	public bool IsLoading();
	public bool IsDedicated();
	public bool IsPaused();
	public bool IsMultiplayer();
	public bool IsPausable();
	public bool IsHLTV();
	public bool IsReplay();

	/// <summary>
	/// Returns the password or null if not set
	/// </summary>
	/// <returns></returns>
	public string? GetPassword();

	public void SetPaused(bool paused);
	public void SetPassword(string? password);

	public void BroadcastMessage(in INetMessage msg, bool onlyActive = false, bool reliable = false);

	public void DisconnectClient(IClient client, string? reason);
}