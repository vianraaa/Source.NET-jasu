using Source.Common;

using System.Diagnostics;

using static Source.Dbg;

namespace Source.Engine.Client;


/// <summary>
/// Base client state, in CLIENT
/// </summary>
public abstract class BaseClientState : INetChannelHandler, IConnectionlessPacketHandler, IServerMessageHandler
{
	public const int CL_CONNECTION_RETRIES = 4;
	public const double CL_MIN_RESEND_TIME = 1.5;
	public const double CL_MAX_RESEND_TIME = 20;
	public const double MIN_CMD_RATE = 10;
	public const double MAX_CMD_RATE = 100;

	public virtual bool ProcessConnectionlessPacket(ref NetPacket packet) => false;

	public abstract void ConnectionStart(NetChannel channel);
	public abstract void ConnectionClosing(string reason);
	public abstract void ConnectionCrashed(string reason);

	public virtual void PacketStart(int incomingSequence, int outgoingAcknowledged) { }
	public virtual void PacketEnd() { }

	public abstract void FileRequested(string fileName, uint transferID);
	public abstract void FileReceived(string fileName, uint transferID);
	public abstract void FileDenied(string fileName, uint transferID);
	public abstract void FileSent(string fileName, uint transferID);

	public virtual bool ProcessMessage(INetMessage message) => false;

	public bool IsActive() => SignOnState == SignOnState.Full;
	public bool IsConnected() => SignOnState >= SignOnState.Connected;
	public virtual void Clear() { }
	public virtual void FullConnect(NetAddress adr) { } // a connection was established
	public virtual void Connect(string adr, string pszSourceTag) { } // start a connection challenge
	public virtual bool SetSignonState(SignOnState state, int count) {
		if (state < SignOnState.None || state > SignOnState.ChangeLevel) {
			Debug.Assert(false, $"Received signon {state} when at {SignOnState}");
			return false;
		}

		if (state > SignOnState.Connected && state <= SignOnState) {
			Debug.Assert(false, $"Received signon {state} when at {SignOnState}");
			return false;
		}

		Msg($"SourceClient: Tracked SignOnState {SignOnState} -> {state}\n");
		SignOnState = state;

		return true;
	}
	public virtual void Disconnect(string pszReason, bool bShowMainMenu) { }
	public virtual void SendConnectPacket(int challengeNr, int authProtocol, ulong unGSSteamID, bool bGSSecure) { }
	public virtual string GetCDKeyHash() => "123";
	public virtual void RunFrame() {
		if (SignOnState == SignOnState.Challenge) {
			CheckForResend();
		}
	}
	public virtual void CheckForResend() { }
	public virtual bool LinkClasses() => false;
	public virtual int GetConnectionRetryNumber() => CL_CONNECTION_RETRIES;
	public virtual string GetClientName() => "WIP"; // cl_name.GetString();

	public virtual int GetClientTickCount() => 0;
	public virtual void SetClientTickCount(int tick) { }

	public virtual int GetServerTickCount() => 0;
	public virtual void SetServerTickCount(int tick) { }

	public virtual void SetClientAndServerTickCount(int tick) { }

	// fields

	public NetSocket Socket;
	public NetChannel NetChannel;
	public uint ChallengeNumber;
	public double ConnectTime;
	public int RetryNumber;
	public string RetryAddress;
	public string retrySourceTag;
	public int RetryChallenge;
	public SignOnState SignOnState;
	public double NextCmdTime;
	public int ServerCount = -1;
	public ulong GameServerSteamID;
	public int CurrentSequence;
	public ClockDriftMgr ClockDriftMgr;
	public int DeltaTick;
	public bool Paused;
	public double PausedExpireTime;
	public int ViewEntity;
	public int PlayerSlot;
	public string LevelFileName;
	public string LevelBaseName;
	public int MaxClients;
	// PackedEntity entity baselines
	// ServerClassInfo

	public int ServerClasses;
	public int ServerClassBits;
	public string EncryptionKey;
	public uint EncryptionKeySize;

	// NetworkStringTableContainer

	//NetworkStringTableContainer StringTableContainer;

	public bool RestrictServerCommands;
	public bool RestrictClientCommands;
}