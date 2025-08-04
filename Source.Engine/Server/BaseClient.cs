using Source.Common.Algorithms;
using Source.Common.Networking;
using Source.Common.Server;

using Steamworks;

namespace Source.Engine.Server;


public abstract class BaseClient : IClient, IClientMessageHandler {
	public int GetPlayerSlot() => ClientSlot;
	public int GetUserID() => UserID;
	// NetworkID?
	public string GetClientName() => Name;
	public INetChannel GetNetChannel() => NetChannel;


	public int ClientSlot;
	public int EntityIndex;
	public int UserID;

	public string Name;
	public string GUID;

	public CSteamID SteamID;
	public uint FriendsID;
	public string FriendsName;

	// convars...
	public bool SendServerInfo;
	public BaseServer Server;
	public bool IsHLTV;
	public bool IsReplay;
	public int ClientChallenge;
	
	public uint SendTableCRC;

	public CustomFile[] CustomFiles;
	public int FilesDownloaded;

	public INetChannel NetChannel;
	public SignOnState SignOnState;
	public int DeltaTick;
	public int StringTableAckTick;
	public int SignOnTick;
	// CSmartPtr<CFrameSnapshot, CRefCountAccessorLongName>
	// CFrameSnapshot baseline
	int BaselineUpdateTick;
	// CBitVec<MAX_EDICTS> BaselinesSent;
	public int BaselineUsed;
}
