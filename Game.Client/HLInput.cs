using Microsoft.Extensions.DependencyInjection;

using Source.Common.Bitbuffers;
using Source.Common.Client;
using Source.Common.Mathematics;
using Source.Common.Networking;
using Source.Engine.Client;

using System.Runtime.CompilerServices;

using static Source.Dbg;

namespace Game.Client;

public class HLInput(IServiceProvider provider, ClientState cl) : IInput {
	readonly Lazy<IBaseClientDLL> clientDLLLzy = new(provider.GetRequiredService<IBaseClientDLL>);
	IBaseClientDLL clientDLL => clientDLLLzy.Value;


	public const int MULTIPLAYER_BACKUP = 90;
	UserCmd[] pCommands = new UserCmd[MULTIPLAYER_BACKUP];

	public unsafe ref UserCmd GetUserCmd(int sequenceNumber) {
		ref UserCmd usercmd = ref pCommands[MathLib.Modulo(sequenceNumber, MULTIPLAYER_BACKUP)];
		if (usercmd.CommandNumber != sequenceNumber)
			return ref Unsafe.NullRef<UserCmd>();

		return ref usercmd;
	}

	public void CreateMove(int sequenceNumber, double inputSampleFrametime, bool active) {
		int nextcmdnr = cl.LastOutgoingCommand + cl.ChokedCommands + 1;
		ref UserCmd cmd = ref pCommands[MathLib.Modulo(nextcmdnr, MULTIPLAYER_BACKUP)];
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
}
