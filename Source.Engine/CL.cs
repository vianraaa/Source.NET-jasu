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

namespace Source.Engine;

/// <summary>
/// Various clientside methods. In Source, these would mostly be represented by
/// CL_MethodName's in the static global namespace
/// </summary>
public class CL(IServiceProvider services, ClientState cl, Net Net, 
	ClientGlobalVariables clientGlobalVariables, ServerGlobalVariables serverGlobalVariables,
	IBaseClientDLL ClientDLL, CommonHostState host_state, Host Host)
{
	public void ApplyAddAngle() {

	}

	public void CheckClientState() {

	}

	public void ExtraMouseUpdate(double frameTime) {
		
	}

	public void Init() {

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
				ok = ok && ClientDLL.WriteUsercmdDeltaToBuffer(move.DataOut, from, to, isnewcmd);
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
			ClientDLL.CreateMove(nextcmdnr, host_state.IntervalPerTick - accumulatedExtraSamples, !cl.IsPaused());

			if (sendPacket)
				SendMove();
			else {
				cl.NetChannel.SetChoked();
				cl.ChokedCommands++;
			}
		}

		if (!sendPacket)
			return;

		bool hasProblem = cl.NetChannel.IsTimingOut && cl.IsActive();

		if (cl.IsActive()) {
			NET_Tick mymsg = new NET_Tick(cl.DeltaTick, (float)Host.FrameTimeUnbounded, (float)Host.FrameTimeStandardDeviation);
			cl.NetChannel.SendNetMsg(mymsg);
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

	public void Connect(string address, string sourceTag) {
		if(!address.Equals("localhost", StringComparison.OrdinalIgnoreCase)) {
			Host.Disconnect(false);
			Net.SetMultiplayer(true);
			// begin loading plague?
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
}



/// <summary>
/// Loads and shuts down the client DLL
/// </summary>
/// <param name="services"></param>
public class ClientDLL(IServiceProvider services, Sys Sys)
{
	public void Init() {

	}

	public void Update() {

	}

	public void ProcessInput() {

	}
}