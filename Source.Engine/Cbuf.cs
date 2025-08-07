using CommunityToolkit.HighPerformance;

using Microsoft.Extensions.DependencyInjection;

using Source.Common.Commands;
using Source.Common.Networking;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;

public class CommandBuffer
{
	public const int ARGS_BUFFER_LENGTH = 2 << 13;

	public class Command
	{
		public long Tick;
		public int FirstArgS;
		public int BufferSize;
	}

	char[] argsBuffer = new char[ARGS_BUFFER_LENGTH];
	int lastUsedArgSize;
	int argBufferSize;
	LinkedList<Command> Commands = [];
	long currentTick;
	long lastTickToProcess;
	long waitDelayTicks;
	LinkedListNode<Command>? nextCommand;
	int maxArgBufferLength;
	bool isProcessingCommands;
	bool waitEnabled;

	TokenizedCommand currentCommand;

	public unsafe bool DequeueNextCommand() {
		currentCommand.Reset();
		Dbg.Assert(isProcessingCommands);
		if (Commands.Count == 0)
			return false;

		LinkedListNode<Command>? command = Commands.First;
		if (command == null)
			return false;

		Command cmd = command.Value;

		currentTick = cmd.Tick;
		if (cmd.BufferSize > 0) {
			fixed (char* buff = argsBuffer) {
				currentCommand.Tokenize(new Span<char>((char*)((nint)buff + (cmd.FirstArgS * sizeof(char))), cmd.BufferSize));
			}
		}

		Commands.Remove(cmd);
		nextCommand = Commands.First;
		return true;
	}

	public CommandBuffer() {
		lastUsedArgSize = 0;
		argBufferSize = 0;
		currentTick = 0;
		lastTickToProcess = -1;
		waitDelayTicks = 1;
		nextCommand = null;
		maxArgBufferLength = ARGS_BUFFER_LENGTH;
		isProcessingCommands = false;
		waitEnabled = false;
	}

	public ref TokenizedCommand GetCommand() => ref currentCommand;

	public unsafe bool AddText(ReadOnlySpan<char> text, long tickDelay = 0) {
		int len = text.Length;
		long tick = currentTick + tickDelay;

		ReadOnlySpan<char> currentCommand = text;
		int offsetToNextCommand = 0;
		for (; len > 0; len -= offsetToNextCommand + 1, currentCommand = currentCommand[(offsetToNextCommand)..]) {
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

			if (!InsertCommand(currentCommand[..commandLength], tick))
				return false;
		}

		return true;
	}

	unsafe void Compact() {
		argBufferSize = 0;
		Span<char> tempBuffer = stackalloc char[ARGS_BUFFER_LENGTH];
		fixed (char* ar = argsBuffer) {
			foreach (Command command in Commands) {
				new ReadOnlySpan<char>(ar, argsBuffer.Length)[command.FirstArgS..].CopyTo(tempBuffer[argBufferSize..]);
				command.FirstArgS = argBufferSize;
				argBufferSize += command.BufferSize;
			}

			tempBuffer[..argBufferSize].CopyTo(new Span<char>(ar, argsBuffer.Length));
		}
	}

	private unsafe bool InsertCommand(ReadOnlySpan<char> argS, long tick) {
		int commandSize = argS.Length;

		if (commandSize > TokenizedCommand.MaxCommandLength) {
			Dbg.Warning($"WARNING: Command too long... ignoring!\n{argS}\n");
			return false;
		}

		if (argBufferSize + commandSize + 1 > maxArgBufferLength) {
			Compact();
			if (argBufferSize + commandSize + 1 > maxArgBufferLength)
				return false;
		}

		fixed (char* pArgSBuffer = argsBuffer) 
			argS.CopyTo(new Span<char>((void*)(((nint)pArgSBuffer) + (argBufferSize * sizeof(char))), argsBuffer.Length));
		
		argsBuffer[argBufferSize + commandSize] = '\0';
		++commandSize;

		Command command = new();
		command.Tick = tick;
		command.FirstArgS = argBufferSize;
		command.BufferSize = commandSize;
		argBufferSize += commandSize;

		if (!isProcessingCommands || (tick > currentTick)) {
			InsertCommandAtAppropriateTime(command);
		}
		else {
			InsertImmediateCommand(command);
		}

		return true;
	}

	private void InsertImmediateCommand(Command command) {
		if (nextCommand == null)
			Commands.AddLast(command);
		else
			Commands.AddAfter(nextCommand, command);
	}

	private void InsertCommandAtAppropriateTime(Command command) {
		LinkedListNode<Command>? i;
		for (i = Commands.First; i != null; i = i.Next) {
			if (i.Value.Tick > command.Tick)
				break;
		}
		if (i == null)
			Commands.AddFirst(command);
		else
			Commands.AddBefore(i, command);
	}

	private bool ParseArgV0(StringReader buf, Span<char> argV0, out ReadOnlySpan<char> argS) {
		argV0[0] = '\0';
		argS = null;

		int size = buf.ParseToken(TokenizedCommand.DefaultBreakSet, argV0);
		if (size <= 0)
			return false;

		int argsLen = buf.TellMaxPut();
		argS = (argsLen > 0) ? buf.PeekToEnd() : null;
		return true;
	}

	private void GetNextCommandLength(ReadOnlySpan<char> text, int maxLen, out int commandLength, out int nextCommandOffset) {
		commandLength = 0;
		bool isQuoted = false;
		bool isCommented = false;
		for (nextCommandOffset = 0; nextCommandOffset < maxLen; ++nextCommandOffset, commandLength += isCommented ? 0 : 1) {
			char c = text[nextCommandOffset];
			if (!isCommented) {
				if (c == '"') {
					isQuoted = !isQuoted;
					continue;
				}

				if (!isQuoted && c == '/') {
					isCommented = (nextCommandOffset < maxLen - 1) && text[nextCommandOffset + 1] == '/';
					if (isCommented) {
						++nextCommandOffset;
						continue;
					}
				}

				if (!isQuoted && c == ';')
					break;
			}

			if (c == '\n') {
				nextCommandOffset++;
				break;
			}
		}
	}

	public void BeginProcessingCommands(int deltaTicks) {
		Dbg.Assert(!isProcessingCommands);
		isProcessingCommands = true;
		lastTickToProcess = currentTick + deltaTicks - 1;
		nextCommand = Commands.First;
	}

	public void EndProcessingCommands() {
		isProcessingCommands = false;
		currentTick = lastTickToProcess + 1;
		nextCommand = null;

		// todo; better error handling here.
	}

	public bool IsProcessingCommands() => isProcessingCommands;

	internal LinkedListNode<Command>? GetNextCommandHandle() {
		return nextCommand;
	}
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
			Buffer.BeginProcessingCommands(1);
			while (Buffer.DequeueNextCommand()) {
				ExecuteCommand(in Buffer.GetCommand(), CommandSource.Command);
			}
			Buffer.EndProcessingCommands();
		}
	}
}