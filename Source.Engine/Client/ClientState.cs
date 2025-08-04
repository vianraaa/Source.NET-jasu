using Source.Common.Hashing;
using Source.Common.Mathematics;
using Source.Common.Networking;

using static Source.Constants;

namespace Source.Engine.Client;

/// <summary>
/// Client state, in CLIENT. Often referred to by 'cl'
/// </summary>
public class ClientState(CommonHostState host_state) : BaseClientState
{
	public bool ProcessConnectionlessPacket(in NetPacket packet) {
		return false;
	}

	public override void Disconnect(string pszReason, bool bShowMainMenu) {
		base.Disconnect(pszReason, bShowMainMenu);
	}
	public override void FullConnect(NetAddress adr) {
		base.FullConnect(adr);
	}

	public override bool SetSignonState(SignOnState state, int count) {
		return base.SetSignonState(state, count);
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
	public override void ConnectionClosing(string reason) {
		throw new NotImplementedException();
	}
	public void SendClientInfo() { }
	public void StartUpdatingSteamResources() { }
	public void CheckUpdatingSteamResources() { }
	public void CheckFileCRCsWithServer() { }
	public void FinishSignonState_New() { }
	public void RunFrame() { }

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

	public override void ConnectionStart(NetChannel channel) {
		throw new NotImplementedException();
	}
}
