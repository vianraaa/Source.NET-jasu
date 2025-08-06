using Microsoft.Extensions.DependencyInjection;

using Source.Common.Commands;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

	public ref TokenizedCommand GetCommand() => ref currentCommand;

	public unsafe bool AddText(ReadOnlySpan<char> text, long tickDelay = 0) {
		int len = text.Length;
		long tick = currentTick + tickDelay;

		ReadOnlySpan<char> currentCommand = text;
		int offsetToNextCommand = 0;
		for (; len > 0; len -= offsetToNextCommand + 1, currentCommand = currentCommand[(offsetToNextCommand + 1)..]) {
			int commandLength;
			GetNextCommandLength(currentCommand, len, out commandLength, out offsetToNextCommand);
			if (commandLength <= 0)
				continue;

			ReadOnlySpan<char> argS;
			Span<char> argV0 = stackalloc char[commandLength];

			StringReader reader = new StringReader(new(currentCommand[..commandLength]));

			ParseArgV0(reader, argV0, out argS);
			if (argV0[0] == '\0')
				continue;

			// wait command later

			if (!InsertCommand(currentCommand, commandLength, tick))
				return false;
		}

		return true;
	}

	private bool InsertCommand(ReadOnlySpan<char> currentCommand, int commandLength, long tick) {
		throw new NotImplementedException();
	}

	private bool ParseArgV0(StringReader buf, Span<char> argV0, out ReadOnlySpan<char> argS) {
		argV0[0] = '\0';
		argS = null;

		int size = buf.ParseToken(TokenizedCommand.DefaultBreakSet, argV0);
		if (size <= 0 || argV0.Length == size)
			return false;

		int argsLen = buf.TellMaxPut();
		argS = (argsLen > 0) ? buf.PeekToEnd() : null;
		return true;
	}

	private void GetNextCommandLength(ReadOnlySpan<char> text, int maxLen, out int commandLength, out int nextCommandOffset) {
		commandLength = 0;
		bool isQuoted = false;
		bool isCommented = false;
		for(nextCommandOffset = 0; nextCommandOffset < maxLen; ++nextCommandOffset, commandLength += isCommented ? 0 : 1) {
			char c = text[nextCommandOffset];
			if (!isCommented) {
				if(c == '"') {
					isQuoted = !isQuoted;
					continue;
				}

				if(!isQuoted && c == '/') {
					isCommented = (nextCommandOffset < maxLen - 1) && text[nextCommandOffset + 1] == '/';
					if (isCommented) {
						++nextCommandOffset;
						continue;
					}
				}

				if (!isQuoted && c == ';')
					break;
			}

			if (c == '\n')
				break;
		}
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
				ExecuteCommand(in Buffer.GetCommand(), CommandSource.Command);
			}
			Buffer.EndProcessingCommands();
		}
	}
}