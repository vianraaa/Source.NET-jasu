using Microsoft.Extensions.DependencyInjection;

using Source.Common.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;

public class CommandBuffer
{
	public const int ARGS_BUFFER_LENGTH = 2 << 13;

	struct Command {
		int Tick;
		int FirstArgS;
		nint BufferSize;
	}

	char[] argsBuffer = new char[ARGS_BUFFER_LENGTH];

	nint lastUsedArgSize;
	nint argBufferSize;
	long currentTick;
	long lastTickToProcess;
	long waitDelayTicks;
	nint nextCommand;
	bool isProcessingCommands;
	bool waitEnabled;

	TokenizedCommand currentCommand;

	public bool DequeueNextCommand() {
		return true;
	}

	public TokenizedCommand GetCommand() {
		return new();
	}

	public bool AddText(ReadOnlySpan<char> text, int tickDelay = 0) {

	}

	public void BeginProcessingCommands() {
		isProcessingCommands = true;
	}

	public void EndProcessingCommands() {
		isProcessingCommands = false;
	}

	public bool IsProcessingCommands() => isProcessingCommands;
}

public class Cbuf(IServiceProvider provider)
{
	public readonly CommandBuffer Buffer = new();

	Cmd Cmd;

	public void Init() {
		Cmd = provider.GetRequiredService<Cmd>();
	}
	public void Shutdown() { }

	public void ExecuteCommand(in TokenizedCommand command, CommandSource source, int clientSlot = -1) {
		Cmd.ExecuteCommand(in command, source);
	}

	public void AddText(ReadOnlySpan<char> text) {
		lock (Buffer) {
			if (!Buffer.AddText(text))
				Dbg.ConMsg("CBuf.AddText: buffer overflow;\n");
		}
	}

	public void InsertText(ReadOnlySpan<char> text) {
		lock (Buffer) {
			Dbg.Assert(Buffer.IsProcessingCommands());
			AddText(text);
		}
	}

	public void Execute() {
		lock (Buffer) {
			Buffer.BeginProcessingCommands();
			while (Buffer.DequeueNextCommand()) {
				ExecuteCommand(Buffer.GetCommand(), CommandSource.Command);
			}
			Buffer.EndProcessingCommands();
		}
	}
}