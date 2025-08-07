using Source.Common.Bitbuffers;
using Source.Common.Commands;
using Source.Common.Filesystem;
using Source.Common.Networking;

using Steamworks;

using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;

using static Source.Dbg;

namespace Source.Engine.Client;
/// <summary>
/// Base client state, in CLIENT
/// </summary>
public abstract class BaseClientState(Host Host, IFileSystem fileSystem, Net Net) : INetChannelHandler, IConnectionlessPacketHandler, IServerMessageHandler
{
	public ConVar cl_connectmethod = new(nameof(cl_connectmethod), "", FCvar.UserInfo | FCvar.Hidden, "Method by which we connected to the current server.");

	public const int CL_CONNECTION_RETRIES = 4;
	public const double CL_MIN_RESEND_TIME = 1.5;
	public const double CL_MAX_RESEND_TIME = 20;
	public const double MIN_CMD_RATE = 10;
	public const double MAX_CMD_RATE = 100;

	public NetSocket Socket;
	public NetChannel NetChannel;
	public uint ChallengeNumber;
	public double ConnectTime;
	public int RetryNumber;
	public string RetryAddress;
	public string RetrySourceTag;
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

	public virtual bool ProcessConnectionlessPacket(ref NetPacket packet) {
		bf_read msg = packet.Message;

		int c = msg.ReadByte();
		switch (c) {
			case S2C.Connection:
				if (SignOnState == SignOnState.Challenge) {
					int myChallenge = msg.ReadLong();
					if (myChallenge != RetryChallenge) {
						ConWarning("incorrect challenge\n");
						return false;
					}
					FullConnect(packet.From);
				}
				break;
			case S2C.ConnectionRejected:
				if (SignOnState == SignOnState.Challenge) {
					int myChallenge = msg.ReadLong();
					if (myChallenge != RetryChallenge) {
						ConWarning("Connection rejection challenge mis-match, ignoring\n");
						return false;
					}

					string? why = msg.ReadString(Protocol.MAX_ROUTABLE_PAYLOAD);
					Disconnect(why ?? "<null>", true);
				}
				break;
			case S2C.Challenge:
				if (SignOnState == SignOnState.Challenge) {
					uint magicVersion = msg.ReadULong();
					ConWarning($"Server.MagicVersion: {magicVersion}\n");
					if (magicVersion != S2C.MagicVersion) {
						Disconnect("Server has not updated to the most recent version.", true);
						return false;
					}

					int challenge = msg.ReadLong();
					int myChallenge = msg.ReadLong();
					if (myChallenge != RetryChallenge) {
						ConWarning("Server challenge was not correct, ignoring.\n");
						return false;
					}

					int authProtocol = msg.ReadLong();
					ulong gameServerID = 0;
					bool secure = false;

					if (authProtocol == Protocol.PROTOCOL_STEAM) {
						if (msg.ReadShort() != 0) {
							Disconnect("Invalid Steam key size.", true);
							return false;
						}

						if (msg.BytesLeft > sizeof(ulong)) {
							if (!msg.ReadInto(out gameServerID)) {
								Disconnect("Invalid game-server Steam ID.", true);
								return false;
							}

							secure = msg.ReadByte() == 1;
						}

						if (secure && !Host.IsSecureServerAllowed()) {
							Disconnect("You are in insecure mode.  You must restart before you can connect to secure servers.", true);
						}
						SendConnectPacket(challenge, authProtocol, gameServerID, secure);
					}
				}
				break;
		}

		return false;
	}

	public virtual void ConnectionStart(NetChannel channel) {
		channel.RegisterMessage<NET_Tick>();
		channel.RegisterMessage<NET_SignonState>();
		channel.RegisterMessage<NET_SetConVar>();
		channel.RegisterMessage<NET_StringCmd>();
		channel.RegisterMessage<svc_Print>();
		channel.RegisterMessage<svc_ServerInfo>();
		channel.RegisterMessage<svc_CreateStringTable>();
		channel.RegisterMessage<svc_UpdateStringTable>();
		channel.RegisterMessage<svc_ClassInfo>();
		channel.RegisterMessage<svc_BSPDecal>();
		channel.RegisterMessage<svc_VoiceInit>();
		channel.RegisterMessage<svc_GameEventList>();
		channel.RegisterMessage<svc_FixAngle>();
		channel.RegisterMessage<svc_SetView>();
		channel.RegisterMessage<svc_UserMessage>();
		channel.RegisterMessage<svc_PacketEntities>();
		channel.RegisterMessage<svc_TempEntities>();
		channel.RegisterMessage<svc_GMod_ServerToClient>();
	}
	public virtual void ConnectionClosing(string reason) {
		Disconnect(reason, true);
	}
	public abstract void ConnectionCrashed(string reason);

	public virtual void PacketStart(int incomingSequence, int outgoingAcknowledged) { }
	public virtual void PacketEnd() { }

	public abstract void FileRequested(string fileName, uint transferID);
	public abstract void FileReceived(string fileName, uint transferID);
	public abstract void FileDenied(string fileName, uint transferID);
	public abstract void FileSent(string fileName, uint transferID);

	public virtual bool ProcessMessage(INetMessage message) {
		switch (message) {
			case NET_SignonState signonstate:
				SetSignonState(signonstate.SignOnState, signonstate.SpawnCount);
				break;
			case svc_UserMessage usermsg:
				byte[] userdata = new byte[Constants.MAX_USER_MSG_DATA];

				bf_read userMsg = new bf_read("UserMessage(read)", userdata, Constants.MAX_USER_MSG_DATA);
				int bitsRead = usermsg.DataIn.ReadBitsClamped(userdata, (uint)usermsg.Length);
				userMsg.StartReading(userdata, Net.Bits2Bytes(bitsRead));

				/*if (!UserMessages.DispatchUserMessage(usermsg.MessageType, userMsg)) {
					ConWarning($"Couldn't dispatch user message ({userMsg})\n");
					return false;
				}*/

				return true;
			case NET_Tick tickmsg:
				NetChannel.SetRemoteFramerate(tickmsg.HostFrameTime, tickmsg.HostFrameDeviation);
				SetClientTickCount(tickmsg.Tick);
				SetServerTickCount(tickmsg.Tick);
				// string tables?

				return GetServerTickCount() > 0;
			case svc_PacketEntities entmsg:
				// Cheating; need to better implement all of this
				if (SignOnState < SignOnState.Spawn) {
					ConWarning("Received packet entities while connecting!\n");
					return false;
				}

				if (entmsg.UpdateBaseline) {
					var clcAck = new clc_BaselineAck(0, entmsg.Baseline);
					NetChannel.SendNetMsg(clcAck, true);
				}

				if (SignOnState == SignOnState.Spawn) {
					if (!entmsg.IsDelta) {
						SetSignonState(SignOnState.Full, ServerCount);
					}
					else {
						ConWarning("Received delta packet entities while spawning!\n");
						return false;
					}
				}

				if (DeltaTick >= 0 || !entmsg.IsDelta)
					DeltaTick = GetServerTickCount();

				break;
		}
		// ignore
		return true;
	}

	public bool IsActive() => SignOnState == SignOnState.Full;
	public bool IsConnected() => SignOnState >= SignOnState.Connected;
	public virtual void Clear() { }
	public virtual void FullConnect(NetAddress adr) { } // a connection was established
	public virtual void Connect(string adr, string sourceTag) {
		RetryChallenge = (Random.Shared.Next(0, 0x0FFF) << 16) | (Random.Shared.Next(0, 0xFFFF));
		GameServerSteamID = 0;
		RetrySourceTag = sourceTag;
		cl_connectmethod.SetValue(sourceTag);

		SetSignonState(SignOnState.Challenge, -1);
		ConnectTime = -double.MaxValue;
		RetryNumber = 0;
	} // start a connection challenge
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
	public virtual void Disconnect(string reason, bool showMainMenu) {
		if (SignOnState == SignOnState.None)
			return;

		if (NetChannel != null) {
			NetChannel.Shutdown(reason ?? "Disconnect by user.");
			NetChannel = null;
		}

		Msg($"Disconnect: {reason}\n");
		SignOnState = SignOnState.None;
	}
	public virtual void SendConnectPacket(int challengeNr, int authProtocol, ulong gameServerSteamID, bool gameServerSecure) {
		string serverName;
		string cdKey = "NOCDKEY";

		if (RetryAddress == null || !Net.StringToAdr(RetryAddress, out IPEndPoint? addr)) {
			ConWarning($"Bad server address ({RetryAddress})\n");
			Disconnect("Bad server address", true);
			return;
		}

		if (addr.Port == 0) {
			addr.Port = Net.PORT_SERVER;
		}

		bf_write msg = new();
		byte[] packet = new byte[Protocol.MAX_ROUTABLE_PAYLOAD];
		msg.StartWriting(packet, packet.Length, 0);
		msg.WriteLong(Protocol.CONNECTIONLESS_HEADER);
		msg.WriteByte(C2S.Connect);
		msg.WriteLong(Protocol.VERSION);
		msg.WriteLong(authProtocol);
		msg.WriteLong((int)ChallengeNumber);
		msg.WriteLong(RetryChallenge);
		msg.WriteUBitLong(2729496039, 32);
		msg.WriteString(GetClientName());
		msg.WriteString(""); // Password in the future
		msg.WriteString(SteamAppInfo.GetSteamInf(fileSystem).PatchVersion);

		switch (authProtocol) {
			case Protocol.PROTOCOL_HASHEDCDKEY:
				throw new Exception("Cannot use CD key protocol");
			case Protocol.PROTOCOL_STEAM:
				if (!PrepareSteamConnectResponse(gameServerSteamID, gameServerSecure, addr, msg))
					return;
				break;
			default:

				break;
		}
		Socket.UDP!.SendTo(msg.BaseArray!, addr);


		this.ConnectTime = Net.Time;
		this.ChallengeNumber = (uint)challengeNr;
	}

	public virtual string GetCDKeyHash() => "123";
	public virtual void RunFrame() {
		if (SignOnState == SignOnState.Challenge) {
			CheckForResend();
		}
	}
	public virtual bool PrepareSteamConnectResponse(ulong gameServerSteamID, bool gameServerSecure, IPEndPoint addr, bf_write msg) {
		// Check steam user
		if (!SteamAPI.IsSteamRunning()) {
			Disconnect("The server requires Steam authentication.", true);
			return false;
		}

		byte[] steam3Cookie = new byte[Protocol.STEAM_KEYSIZE];
		var result = SteamUser.GetAuthSessionTicket(steam3Cookie, Protocol.STEAM_KEYSIZE, out uint keysize);

		msg.WriteShort((int)(keysize + sizeof(ulong)));
		msg.WriteLongLong((long)SteamUser.GetSteamID().m_SteamID);

		if (keysize > 0)
			msg.WriteBytes(steam3Cookie, (int)keysize);

		return true;
	}
	public virtual void CheckForResend() {
		if (SignOnState != SignOnState.Challenge) return;

		if ((Net.Time - ConnectTime) < 1)
			return;

		if (RetryAddress == null || !Net.StringToAdr(RetryAddress, out IPEndPoint? addr)) {
			ConMsg($"Bad server address ({RetryAddress})\n");
			Disconnect("Bad server address", true);
			return;
		}

		if (RetryNumber >= 5) {
			ConMsg($"Connection failed after {RetryNumber} retries.\n");
			Disconnect("Connection failed", true);
			return;
		}

		if (RetryNumber == 0)
			ConMsg($"Connecting to {RetryAddress}...\n");
		else
			ConMsg($"Retrying {RetryAddress}...\n");
		RetryNumber++;

		bf_write msg = new bf_write();
		msg.StartWriting(new byte[Protocol.MAX_ROUTABLE_PAYLOAD], Protocol.MAX_ROUTABLE_PAYLOAD, 0);
		msg.WriteLong(Protocol.CONNECTIONLESS_HEADER);
		msg.WriteByte(A2S.GetChallenge);
		msg.WriteLong(RetryChallenge);
		msg.WriteString("0000000000");

		Socket.UDP!.SendTo(msg.BaseArray!, addr);

		ConnectTime = Net.Time;
	}

	public virtual bool LinkClasses() => false;
	public virtual int GetConnectionRetryNumber() => CL_CONNECTION_RETRIES;

	public ConVar cl_name = new(nameof(cl_name), "unnamed", FCvar.Archive | FCvar.UserInfo | FCvar.PrintableOnly | FCvar.ServerCanExecute, "Current user name");

	public virtual string GetClientName() => cl_name.GetString();

	public virtual int GetClientTickCount() => 0;
	public virtual void SetClientTickCount(int tick) { }

	public virtual int GetServerTickCount() => 0;
	public virtual void SetServerTickCount(int tick) { }

	public virtual void SetClientAndServerTickCount(int tick) { }
}