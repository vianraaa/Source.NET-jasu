using Source.Common;
using Source.Engine.Client;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Source.Constants;
using static Source.Common.Networking.Protocol;
using Source.Common.Networking;
using Source.Common.Client;
using Source.Common.Commands;
using Microsoft.Extensions.DependencyInjection;
using Source.Common.Engine;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Source.Engine;

/// <summary>
/// Various clientside methods. In Source, these would mostly be represented by
/// CL_MethodName's in the static global namespace
/// </summary>
public class CL(IServiceProvider services, Net Net, 
	ClientGlobalVariables clientGlobalVariables, ServerGlobalVariables serverGlobalVariables,
	CommonHostState host_state, Host Host, Cbuf Cbuf, IEngineVGuiInternal? EngineVGui, Scr Scr, Shader Shader, ClientDLL ClientDLL)
{
	public IPrediction ClientSidePrediction => ClientDLL.ClientSidePrediction;
	public IClientEntityList EntityList => ClientDLL.EntityList;
	public ICenterPrint CenterPrint => ClientDLL.CenterPrint;
	public IClientLeafSystemEngine ClientLeafSystem => ClientDLL.ClientLeafSystem;


	public ClientState cl;
	public IBaseClientDLL clientDLL;
	public IEngineClient engineClient;
	public void ApplyAddAngle() {

	}

	public void CheckClientState() {

	}

	public void ExtraMouseUpdate(double frameTime) {
		if (!cl.IsActive())
			return;

		if (!Host.ShouldRun())
			return;

		clientDLL.ExtraMouseSample(frameTime, !cl.Paused);
	}

	public void Init() {
		cl = services.GetRequiredService<ClientState>();
		cl.Clear();

		clientDLL = services.GetRequiredService<IBaseClientDLL>();
		engineClient = services.GetRequiredService<IEngineClient>();
	}

	public void ReadPackets(bool finalTick) {
		//if(!Host.ShouldRun) return;

		cl.OldTickCount = cl.GetServerTickCount();
		if (!cl.IsPaused()) {
			cl.SetClientTickCount(cl.GetClientTickCount() + 1);

			if (!ClockDriftMgr.Enabled)
				cl.SetServerTickCount(cl.GetClientTickCount());

			clientGlobalVariables.TickCount = cl.GetClientTickCount();
			clientGlobalVariables.CurTime = cl.GetTime();
		}
		clientGlobalVariables.FrameTime = cl.GetFrameTime();

		Net.ProcessSocket(NetSocketType.Client, cl);
	}

	public void RunPrediction(PredictionReason reason) {
		
	}

	public void SendMove() {
		byte[] data = ArrayPool<byte>.Shared.Rent(MAX_CMD_BUFFER);
		{
			int nextcommandnr = cl.LastOutgoingCommand + cl.ChokedCommands + 1;
			CLC_Move move = new();

			move.DataOut.StartWriting(data, data.Length, 0);
			int cl_cmdbackup = 2;
			move.BackupCommands = Math.Clamp(cl_cmdbackup, 0, MAX_BACKUP_COMMANDS);
			move.NewCommands = 1 + cl.ChokedCommands;
			move.NewCommands = Math.Clamp(move.NewCommands, 0, MAX_NEW_COMMANDS);
			int numcmds = move.NewCommands + move.BackupCommands;
			int from = -1;
			bool ok = true;
			for (int to = nextcommandnr - numcmds + 1; to <= nextcommandnr; to++) {
				bool isnewcmd = to >= (nextcommandnr - move.NewCommands + 1);
				ok = ok && clientDLL.WriteUsercmdDeltaToBuffer(move.DataOut, from, to, isnewcmd);
				from = to;
			}

			if (ok)
				cl.NetChannel?.SendNetMsg(move);
		}
		ArrayPool<byte>.Shared.Return(data, true);
	}

	public void Move(double accumulatedExtraSamples, bool finalTick) {
		if (!cl.IsConnected())
			return;

		bool sendPacket = true;

		if (Net.Time < cl.NextCmdTime || !cl.NetChannel.CanSendPacket())
			sendPacket = false;

		if (cl.IsActive()) {
			int nextcmdnr = cl.LastOutgoingCommand + cl.ChokedCommands + 1;
			clientDLL.CreateMove(nextcmdnr, host_state.IntervalPerTick - accumulatedExtraSamples, !cl.IsPaused());

			if (sendPacket)
				SendMove();
			else {
				cl.NetChannel?.SetChoked();
				cl.ChokedCommands++;
			}
		}

		if (!sendPacket)
			return;

		bool hasProblem = cl.NetChannel.IsTimingOut && cl.IsActive();

		if (cl.IsActive()) {
			NET_Tick mymsg = new NET_Tick(cl.DeltaTick, (float)Host.FrameTimeUnbounded, (float)Host.FrameTimeStandardDeviation);
			cl.NetChannel?.SendNetMsg(mymsg);
		}

		cl.LastOutgoingCommand = cl.NetChannel.SendDatagram(null);
		cl.ChokedCommands = 0;

		if (cl.IsActive()) {
			const int cl_cmdrate = 66;
			float commandInterval = 1.0f / cl_cmdrate;
			float maxDelta = Math.Min((float)host_state.IntervalPerTick, commandInterval);
			float delta = Math.Clamp((float)(Net.Time - cl.NextCmdTime), 0, maxDelta);
			cl.NextCmdTime = Net.Time + commandInterval - delta;
		}
		else {
			cl.NextCmdTime = Net.Time + (1f / 5f);
		}
	}

	public void FullyConnected() {
		EngineVGui?.UpdateProgressBar(LevelLoadingProgress.FullyConnected);
		// Static prop manager level init client
		// Flush dynamic models
		// Purge unused models
		// Shutdown preload data
		// Pending pure file reloads
		// Level init post entity

		// Start notifying dependencies
		int ip = 0;
		short port = 0;
		int queryPort = 0;
		// TODO ^^^^^^^ requires some changes in netchannels.
		EngineVGui!.NotifyOfServerConnect(Common.Gamedir, ip, port, queryPort);
		EngineVGui!.UpdateProgressBar(LevelLoadingProgress.ReadyToPlay);
		// MDL cache end map load

		Scr.EndLoadingPlaque();
		// EndLoadingUpdates();
	}

	public void Connect(string address, string sourceTag) {
		if(!address.Equals("localhost", StringComparison.OrdinalIgnoreCase)) {
			Host.Disconnect(false);
			Net.SetMultiplayer(true);
			EngineVGui?.EnabledProgressBarForNextLoad();
			Scr.BeginLoadingPlaque();
			EngineVGui?.UpdateProgressBar(LevelLoadingProgress.BeginConnect);
		}
		else {
			cl.Disconnect("Connecting to local host", false);
		}

		cl.Connect(address, sourceTag);
	}

	[ConCommand]
	public void connect(in TokenizedCommand cmd) {
		var splits = cmd.ArgS().Split(' ');
		if(splits.Length == 1) {
			Connect(splits[0], "");
		}
		else if(splits.Length == 2) {
			Connect(splits[0], splits[1]);
		}
		else {
			Dbg.ConMsg("Usage:  connect <server>\n");
		}
	}

	[ConCommand(helpText: "Retry connection to last server.")]
	void retry() {
		Retry();
	}

	public void Retry() {
		if (cl == null) return;

		if(cl.RetryAddress == null) {
			Dbg.ConMsg("Can't retry, no previous connection\n");
			return;
		}

		bool canAddExecutionMarkers = Cbuf.HasRoomForExecutionMarkers(2);
		Dbg.ConMsg($"Commencing connection retry to {cl.RetryAddress}\n");

		ReadOnlySpan<char> command = $"connect {cl.RetryAddress} {cl.RetryChallenge}\n";
		if (cl.RestrictServerCommands && canAddExecutionMarkers)
			Cbuf.AddTextWithMarkers(CmdExecutionMarker.DisableServerCanExecute, command, CmdExecutionMarker.EnableServerCanExecute);
		else
			Cbuf.AddText(command);
	}

	internal void ProcessVoiceData() {

	}

	internal void TakeSnapshotAndSwap() {
		Shader.SwapBuffers();
	}

	public void DumpStringTables() => cl.StringTableContainer?.Dump();

	internal bool CheckCRCs(ReadOnlySpan<char> levelFileName) {
		return true;
	}

	internal void RegisterResources() {
		host_state.SetWorldModel(cl.GetModel(1));
		if (host_state.WorldModel == null)
			Host.Error("CL.RegisterResources: host_state.WorldModel/cl.GetModel(1) == NULL\n");
	}

	public ref PrecacheUserData GetPrecacheUserData(INetworkStringTable table, int index) {
		Span<byte> d = table.GetStringUserData(index);
		ref PrecacheUserData data = ref MemoryMarshal.Cast<byte, PrecacheUserData>(d)[0];
		if (!Unsafe.IsNullRef(ref data) && d.Length != Unsafe.SizeOf<PrecacheUserData>()) 
			Error($"CL.GetPrecacheUserData({table.GetTableId()}, {index}) - length ({d.Length}) invalid.");

		return ref data;
	}

	public int PropsDecoded;

	public bool ProcessPacketEntities(svc_PacketEntities entmsg) {
		ClientFrame newFrame = cl.AllocateFrame();
		newFrame.Init(cl.GetServerTickCount());
		ClientFrame? oldFrame = null;
		
		if (entmsg.IsDelta) {
			int nDeltaTicks = cl.GetServerTickCount() - entmsg.DeltaFrom;
			float flDeltaSeconds = Host.TicksToTime(nDeltaTicks);

			if (cl.GetServerTickCount() == entmsg.DeltaFrom) {
				Host.Error("Update self-referencing, connection dropped.\n");
				return false;
			}

			oldFrame = cl.GetClientFrame(entmsg.DeltaFrom);

			if (oldFrame == null) {
				FlushEntityPacket(newFrame, "Update delta not found.\n");
				return false;
			}
		}
		else {
			for (int i = 0; i <= ClientDLL.EntityList.GetHighestEntityIndex(); i++) {
				DeleteDLLEntity(i, "ProcessPacketEntities", true);
			}
		}

		ClientDLL.FrameStageNotify(ClientFrameStage.NetUpdateStart);

		PropsDecoded = 0;

		Assert(entmsg.Baseline >= 0 && entmsg.Baseline < 2);

		if (entmsg.UpdateBaseline) {
			int updateBaseline = (entmsg.Baseline == 0) ? 1 : 0;
			cl.CopyEntityBaseline(entmsg.Baseline, updateBaseline);

			// WHEN GETTING THIS WORKING AGAIN, REMOVE THE OTHER TIME WE DO THIS AS A TEMPORARY MEASURE
			throw new Exception("Look at the above comment");
			var msg = new clc_BaselineAck(cl.GetServerTickCount(), entmsg.Baseline);
			cl.NetChannel!.SendNetMsg(msg, true);
		}

		EntityReadInfo readInfo = new();
		readInfo.Buf = entmsg.DataIn;
		readInfo.From = oldFrame;
		readInfo.To = newFrame;
		readInfo.AsDelta = entmsg.IsDelta;
		readInfo.HeaderCount = entmsg.UpdatedEntries;
		readInfo.Baseline = entmsg.Baseline;
		readInfo.UpdateBaselines = entmsg.UpdateBaseline;

		cl.ReadPacketEntities(readInfo);

		ClientDLL.FrameStageNotify(ClientFrameStage.NetUpdatePostDataUpdateStart);

		CallPostDataUpdates(readInfo);

		ClientDLL.FrameStageNotify(ClientFrameStage.NetUpdatePostDataUpdateEnd);

		MarkEntitiesOutOfPVS(ref newFrame.TransmitEntity);

		cl.NetChannel!.UpdateMessageStats(NetChannelGroup.LocalPlayer, readInfo.LocalPlayerBits);
		cl.NetChannel!.UpdateMessageStats(NetChannelGroup.OtherPlayers, readInfo.OtherPlayerBits);
		cl.NetChannel!.UpdateMessageStats(NetChannelGroup.Entities, -(readInfo.LocalPlayerBits + readInfo.OtherPlayerBits));

		cl.DeleteClientFrames(entmsg.DeltaFrom);

		if (ClientFrame.MAX_CLIENT_FRAMES < cl.AddClientFrame(newFrame)) 
			DevMsg(1, "CL.ProcessPacketEntities: frame window too big (>%i)\n", ClientFrame.MAX_CLIENT_FRAMES);
		
		ClientDLL.FrameStageNotify(ClientFrameStage.NetUpdateEnd);

		return true;
	}

	private void CallPostDataUpdates(EntityReadInfo u) {
		throw new NotImplementedException();
	}

	private void MarkEntitiesOutOfPVS(ref MaxEdictsBitVec transmitEntity) {
		throw new NotImplementedException();
	}

	private void DeleteDLLEntity(int entIndex, ReadOnlySpan<char> reason, bool onRecreatingAllEntities) {
		IClientNetworkable? net = EntityList.GetClientNetworkable(entIndex);

		if (net != null) {
			ClientClass clientClass = net.GetClientClass();

			RecordDeleteEntity(entIndex, clientClass);
			if (onRecreatingAllEntities) 
				net.SetDestroyedOnRecreateEntities();

			net.Release();
		}
	}

	private void RecordDeleteEntity(int entIndex, ClientClass clientClass) {

	}

	private void FlushEntityPacket(ClientFrame newFrame, string v) {
		throw new NotImplementedException();
	}

	internal void PreprocessEntities() {
		throw new NotImplementedException();
	}
}

/// <summary>
/// Loads and shuts down the client DLL
/// </summary>
/// <param name="services"></param>
public class ClientDLL(IServiceProvider services, Sys Sys)
{
	public IBaseClientDLL clientDLL;
	public IPrediction ClientSidePrediction;
	public IClientEntityList EntityList;
	public ICenterPrint CenterPrint;
	public IClientLeafSystemEngine ClientLeafSystem;
	public void Init() {
		clientDLL = services.GetRequiredService<IBaseClientDLL>();

		if (!clientDLL.Init())
			Sys.Error("Client.dll Init() in library client failed.");

		ClientSidePrediction = services.GetRequiredService<IPrediction>();
		EntityList = services.GetRequiredService<IClientEntityList>();
		CenterPrint = services.GetRequiredService<ICenterPrint>();
		ClientLeafSystem = services.GetRequiredService<IClientLeafSystemEngine>();
	}

	public void Update() {

	}

	public void ProcessInput() {

	}

	public void FrameStageNotify(ClientFrameStage stage) {
		clientDLL.FrameStageNotify(stage);
	}
}