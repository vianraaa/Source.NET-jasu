using Game.Shared;

using Microsoft.Extensions.DependencyInjection;

using Source.Common.Bitbuffers;
using Source.Common.Client;
using Source.Common.Engine;
using Source.Common.Input;
using Source.Common.Mathematics;
using Source.Common.Networking;
using Source.Engine.Client;

using System.Runtime.CompilerServices;

namespace Game.Client;

public class HLInput(IServiceProvider provider, IClientMode ClientMode) : IInput {
	readonly Lazy<IBaseClientDLL> clientDLLLzy = new(provider.GetRequiredService<IBaseClientDLL>);
	IBaseClientDLL clientDLL => clientDLLLzy.Value;

	ClientState cl;

	public const int MULTIPLAYER_BACKUP = 90;
	UserCmd[] Commands = new UserCmd[MULTIPLAYER_BACKUP];

	public unsafe ref UserCmd GetUserCmd(int sequenceNumber) {
		ref UserCmd usercmd = ref Commands[MathLib.Modulo(sequenceNumber, MULTIPLAYER_BACKUP)];
		if (usercmd.CommandNumber != sequenceNumber)
			return ref UserCmd.NULL;

		return ref usercmd;
	}

	public void CreateMove(int sequenceNumber, double inputSampleFrametime, bool active) {
		int nextcmdnr = cl.LastOutgoingCommand + cl.ChokedCommands + 1;
		ref UserCmd cmd = ref Commands[MathLib.Modulo(nextcmdnr, MULTIPLAYER_BACKUP)];
		{
			cmd.Reset();
			cmd.CommandNumber = nextcmdnr;
			cmd.TickCount = cl.GetClientTickCount();
		}
	}

	public unsafe void ValidateUserCmd(UserCmd* f, int from) {

	}


	public unsafe bool WriteUsercmdDeltaToBuffer(bf_write buf, int from, int to, bool isNewCommand) {
		UserCmd nullcmd;
		UserCmd* f = null, t = null;

		int startbit = buf.BitsWritten;
		if (from == -1) {
			f = &nullcmd;
		}
		else {
			f = (UserCmd*)Unsafe.AsPointer(ref GetUserCmd(from));
			if (f == null) {
				f = &nullcmd;
			}
			else {
				ValidateUserCmd(f, from);
			}
		}

		t = (UserCmd*)Unsafe.AsPointer(ref GetUserCmd(to));
		if (t == null) {
			t = &nullcmd;
		}
		else {
			ValidateUserCmd(t, to);
		}

		UserCmd.WriteUsercmd(buf, in *t, in *f);
		if (buf.Overflowed) {
			Warning("WARNING! User command buffer overflow\n");
			return false;
		}

		return true;
	}

	public void EncodeUserCmdToBuffer(bf_write buf, int sequenceNumber) {
		ref UserCmd cmd = ref GetUserCmd(sequenceNumber);
		UserCmd.WriteUsercmd(buf, in cmd, in UserCmd.NULL);
	}

	public void DecodeUserCmdFromBuffer(bf_read buf, int sequenceNumber) {
		ref UserCmd cmd = ref Commands[MathLib.Modulo(sequenceNumber, MULTIPLAYER_BACKUP)];
		UserCmd.ReadUsercmd(buf, ref cmd, ref UserCmd.NULL);
	}

	public void Init() {
		cl = Singleton<ClientState>();
	}

	bool CameraInterceptingMouse;

	public int KeyEvent(int down, ButtonCode code, ReadOnlySpan<char> currentBinding) {
		if ((code == ButtonCode.MouseLeft) || (code == ButtonCode.MouseRight)) {
			if (CameraInterceptingMouse)
				return 0;
		}

		ClientMode?.KeyInput(down, code, currentBinding);

		return 1;
	}
}
