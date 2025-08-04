using Source.Common;
using Source.Common.Networking;

namespace Source.Engine.Server;

/// <summary>
/// Base server, in SERVER. Often referred to by 'sv'
/// </summary>
public class GameServer : BaseServer
{
	public override void BroadcastMessage(in INetMessage msg, bool onlyActive = false, bool reliable = false) {
		throw new NotImplementedException();
	}

	public override void DisconnectClient(IClient client, string? reason) {
		throw new NotImplementedException();
	}

	public override string GetName() {
		throw new NotImplementedException();
	}

	public override int GetNetStats(in float avgIn, out float avgOut) {
		throw new NotImplementedException();
	}

	public override int GetNumClients() {
		throw new NotImplementedException();
	}

	public override int GetNumFakeClients() {
		throw new NotImplementedException();
	}

	public override int GetNumPlayers() {
		throw new NotImplementedException();
	}

	public override int GetNumProxies() {
		throw new NotImplementedException();
	}

	public override string? GetPassword() {
		throw new NotImplementedException();
	}

	public override bool GetPlayerInfo(int clientIndex, out PlayerInfo pinfo) {
		throw new NotImplementedException();
	}

	public override double GetTime() {
		throw new NotImplementedException();
	}

	public override bool ProcessConnect(ref NetPacket packet) {
		throw new NotImplementedException();
	}

	public override bool ProcessConnectionlessPacket(ref NetPacket packet) {
		throw new NotImplementedException();
	}

	public override bool ProcessDetails(ref NetPacket packet) {
		throw new NotImplementedException();
	}

	public override bool ProcessGetChallenge(ref NetPacket packet) {
		throw new NotImplementedException();
	}

	public override bool ProcessInfo(ref NetPacket packet) {
		throw new NotImplementedException();
	}

	public override bool ProcessPlayers(ref NetPacket packet) {
		throw new NotImplementedException();
	}

	public override bool ProcessProcessLog(ref NetPacket packet) {
		throw new NotImplementedException();
	}

	public override bool ProcessRcon(ref NetPacket packet) {
		throw new NotImplementedException();
	}

	public override bool ProcessRules(ref NetPacket packet) {
		throw new NotImplementedException();
	}

	public override void SetPassword(string? password) {
		throw new NotImplementedException();
	}

	public override void SetPaused(bool paused) {
		throw new NotImplementedException();
	}

	internal void Init(bool dedicated) {
		
	}
}
