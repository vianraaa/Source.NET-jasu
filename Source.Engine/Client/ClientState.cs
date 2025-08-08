using Source.Common.Filesystem;
using Source.Common.Hashing;
using Source.Common.Mathematics;
using Source.Common.Networking;
using Source.Engine.Server;

using Steamworks;

using static Source.Constants;

using GameServer = Source.Engine.Server.GameServer;

namespace Source.Engine.Client;

/// <summary>
/// Client state, in CLIENT. Often referred to by 'cl'
/// </summary>
public class ClientState : BaseClientState
{
	readonly Host Host;
	readonly IFileSystem fileSystem;
	readonly Net Net;
	readonly CommonHostState host_state;
	readonly ClockDriftMgr ClockDriftMgr;
	public ClientState(Host Host, IFileSystem fileSystem, Net Net, CommonHostState host_state, GameServer sv, Cbuf Cbuf) : base(Host, fileSystem, Net, sv, Cbuf) {
		this.Host = Host;
		this.fileSystem = fileSystem;
		this.Net = Net;
		this.host_state = host_state;
		this.ClockDriftMgr = new(this, Host, host_state);
	}
	public bool ProcessConnectionlessPacket(in NetPacket packet) {
		return false;
	}

	public override void Disconnect(string? reason, bool showMainMenu) {
		base.Disconnect(reason, showMainMenu);
	}
	public override void FullConnect(NetAddress adr) {
		base.FullConnect(adr);
	}

	public override int GetClientTickCount() => ClockDriftMgr.ClientTick;
	public override void SetClientTickCount(int tick) => ClockDriftMgr.ClientTick = tick;
	public override int GetServerTickCount() => ClockDriftMgr.ServerTick;
	public override void SetServerTickCount(int tick) => ClockDriftMgr.ServerTick = tick;

	public override bool SetSignonState(SignOnState state, int count) {
		if (!base.SetSignonState(state, count))
			return false;

		ServerCount = count;

		switch (SignOnState) {
			case SignOnState.Connected:
				if (NetChannel == null) throw new Exception();
				NetChannel.Clear();
				NetChannel.Timeout = NetChannel.SIGNON_TIME_OUT;
				NetChannel.SetMaxBufferSize(true, Protocol.MAX_PAYLOAD);

				var msg = new NET_SetConVar();

				NetChannel.SendNetMsg(msg);
				break;
			case SignOnState.New:
				StartUpdatingSteamResources();
				return true; // Don't tell the server yet we're at this point
			case SignOnState.PreSpawn:
				break;

			case SignOnState.Full:
				Dbg.Msg("Fully connected!\n");
				break;
		}

		if (state >= SignOnState.Connected && NetChannel != null) {
			var msg = new NET_SignonState(state, ServerCount);
			NetChannel.SendNetMsg(msg);

			// TODO: where to *actually* send this? Next tick?
			if (SignOnState == SignOnState.PreSpawn) {
				CLC_ListenEvents msg2 = new CLC_ListenEvents();
				NetChannel.SendNetMsg(msg2);

				NET_SetConVar cvr = new NET_SetConVar();
				cvr.AddCVar("cl_playermodel", "combineelite");
				cvr.AddCVar("cl_playercolor", "1.000000 1.000000 1.000000");
				NetChannel.SendNetMsg(cvr);
			}
		}

		return true;
	}

	public override void PacketStart(int incomingSequence, int outgoingAcknowledged) {
		base.PacketStart(incomingSequence, outgoingAcknowledged);
	}

	public override void PacketEnd() {
		base.PacketEnd();
	}

	public override void FileReceived(string fileName, uint transferID) {
		throw new NotImplementedException();
	}
	public override void FileRequested(string fileName, uint transferID) {
		throw new NotImplementedException();
	}
	public override void FileDenied(string fileName, uint transferID) {
		throw new NotImplementedException();
	}
	public override void FileSent(string fileName, uint transferID) {
		throw new NotImplementedException();
	}
	public override void ConnectionCrashed(string reason) {
		throw new NotImplementedException();
	}
	public void StartUpdatingSteamResources() {
		// for now; just make signon state new
		FinishSignonState_New();
	}
	public void CheckUpdatingSteamResources() { }
	public void CheckFileCRCsWithServer() { }
	public void SendClientInfo() {
		CLC_ClientInfo info = new CLC_ClientInfo();
		info.SendTableCRC = CLC.ClientInfoCRC;
		info.ServerCount = ServerCount;
		info.IsHLTV = false;
		info.FriendsID = SteamUser.GetSteamID().m_SteamID;
		info.FriendsName = "";

		// check stuff later

		NetChannel.SendNetMsg(info);
	}

	public void FinishSignonState_New() {
		if (SignOnState != SignOnState.New)
			return;

		SendClientInfo();
		var msg1 = new CLC_GMod_ClientToServer();
		NetChannel.SendNetMsg(msg1);
		var msg = new NET_SignonState(SignOnState, ServerCount);
		NetChannel.SendNetMsg(msg);
	}

	public override void RunFrame() => base.RunFrame();

	public double LastServerTickTime;
	public bool InSimulation;

	public int OldTickCount;
	public double TickRemainder;
	public double FrameTime;

	public int LastOutgoingCommand;
	public int ChokedCommands;
	public int LastCommandAck;
	public int CommandAck;
	public int SoundSequence;

	public bool IsHLTV;
	public bool IsReplay;

	public MD5Value ServerMD5;

	public byte[] AreaBits = new byte[MAX_AREA_STATE_BYTES];
	public byte[] AreaPortalBits = new byte[MAX_AREA_PORTAL_STATE_BYTES];
	public bool AreaBitsValid;

	public QAngle ViewAngles;
	// add angle??
	public float AddAngleTotal;
	public float PrevAddAngleTotal;
	public CustomFile[] CustomFiles = new CustomFile[MAX_CUSTOM_FILES];
	public uint FriendsID;
	public string FriendsName;

	public double GetTime() {
		int tickCount = GetClientTickCount();
		double tickTime = tickCount * host_state.IntervalPerTick;
		if (InSimulation)
			return tickTime;

		return tickTime + TickRemainder;
	}
	public bool IsPaused() => Paused;
	public double GetPausedExpireTime() => PausedExpireTime;
	public double GetFrameTime() {
		if (!ClockDriftMgr.Enabled && InSimulation) {
			int elapsedTicks = (GetClientTickCount() - OldTickCount);
			return elapsedTicks * host_state.IntervalPerTick;
		}

		return IsPaused() ? 0 : FrameTime;
	}
	public void SetFrameTime(double dt) => FrameTime = dt;
	public double GetClientInterpAmount() => throw new NotImplementedException();
	public void Clear() {

	}

	public override void Connect(string adr, string sourceTag) {
		Socket = Net.GetSocket(NetSocketType.Client);
		base.Connect(adr, sourceTag);
	}
}
