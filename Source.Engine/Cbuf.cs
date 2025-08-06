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
	public bool DequeueNextCommand() {
		return true;
	}

	public TokenizedCommand GetCommand() {
		return new();
	}

	public void BeginProcessingCommands() {

	}

	public void EndProcessingCommands() {

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
		Cmd.ExecuteCommand(command, source);
	}

	public void InsertText(ReadOnlySpan<char> text) {

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