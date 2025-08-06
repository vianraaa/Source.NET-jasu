using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Common.Commands;
using Source.Common.Engine;
using Source.Engine.Client;
using Source.Engine.Server;

using System;

namespace Source.Engine;

public class Cmd(IEngineAPI provider)
{
	public const string CMDSTR_ADD_EXECUTION_MARKER = "[$&*,`]";

	Cbuf Cbuf;
	Host Host;
	ICvar cvar;
	ClientState cl;
	GameServer sv;
	ICommandLine CommandLine;
	CvarUtilities cv;
	public void Init() {
		Cbuf = provider.GetRequiredService<Cbuf>();
		Host = provider.GetRequiredService<Host>();
		cvar = provider.GetRequiredService<ICvar>();
		cl = provider.GetRequiredService<ClientState>();
		sv = provider.GetRequiredService<GameServer>();
		cv = provider.GetRequiredService<CvarUtilities>();
		CommandLine = provider.GetRequiredService<ICommandLine>();
	}
	public void Shutdown() { }

	Dictionary<string, string> aliases = [];

	public CommandSource CommandSource;
	public int ClientSlot;

	private void HandleExecutionMarker(ReadOnlySpan<char> command, ReadOnlySpan<char> markerCodeS) {
		int markerCode = (int.TryParse(markerCodeS, out int i) ? i : -1);
	}

	public ConCommandBase? ExecuteCommand(in TokenizedCommand command, CommandSource source, int clientSlot = -1) {
		if (command.ArgC() == 0)
			return null;

		if (command[0].Equals(CMDSTR_ADD_EXECUTION_MARKER)) {
			if (command.ArgC() == 3)
				HandleExecutionMarker(command[1], command[2]);
			else
				Dbg.Warning("WARNING: INVALID EXECUTION MARKER.\n");

			return null;
		}

		if (aliases.TryGetValue(new(command[0]), out var aliasValue)) {
			Cbuf.InsertText(aliasValue);
			return null;
		}

		CommandSource = source;
		ClientSlot = clientSlot;

		ConCommandBase? commandBase = cvar.FindCommandBase(command[0]);

		if (ShouldPreventServerCommand(commandBase))
			return null;

		if (commandBase != null && commandBase.IsCommand()) {
			if (!ShouldPreventClientCommand(commandBase) && commandBase.IsCommand()) {
				bool isServerCommand = commandBase.IsFlagSet(FCvar.GameDLL) && source == CommandSource.Command && !sv.IsDedicated();

				// Todo: hook to allow game .dll to figure out who typed the message on a listen server

				if (!Host.IsSinglePlayerGame() && !Host.CanCheat()) {
					// TODO; allow server to run it...
					Dbg.Msg($"Can't use cheat command {commandBase.GetName()} in multiplayer, unless the server has sv_cheats set to 1.\n");
					return null;
				}

				if (commandBase.IsFlagSet(FCvar.SingleplayerOnly)) {
					if (!Host.IsSinglePlayerGame()) {
						Dbg.Msg($"Can't use command {commandBase.GetName()} in multiplayer.\n");
						return null;
					}
				}

				// development only stuff

				Dispatch(commandBase, command);
				return commandBase;
			}
		}

		if(commandBase != null && source == CommandSource.Command && CommandLine.CheckParm("-default") && !commandBase.IsFlagSet(FCvar.ExecDespiteDefault)) {
			Dbg.Msg($"Ignoring cvar \"{commandBase.GetName()}\" due to -default on command line\n");
			return null;
		}

		if (cv.IsCommand(command))
			return commandBase;

		if(source == CommandSource.Command) {
			if (cl.IsConnected()) {
				ForwardToServer(command);
			}
		}

		Dbg.Msg($"Unknown command \"{command[0]}\"\n");
		return null;
	}

	private void ForwardToServer(TokenizedCommand command) {
		throw new NotImplementedException();
	}

	private void Dispatch(ConCommandBase commandBase, TokenizedCommand command) {
		
	}

	public bool ShouldPreventClientCommand(ConCommandBase? cmd) {
		return false;
	}

	public bool ShouldPreventServerCommand(ConCommandBase? cmd) {
		if (!Host.IsSinglePlayerGame()) {
			// todo: make this work.
			return false;
		}

		return false;
	}
}
