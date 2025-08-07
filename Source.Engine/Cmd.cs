using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Common.Commands;
using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Engine.Client;
using Source.Engine.Server;

using System;
using System.Text;

namespace Source.Engine;

public class Cmd(IEngineAPI provider, IFileSystem fileSystem)
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

		if (command[0].Equals(CMDSTR_ADD_EXECUTION_MARKER, StringComparison.OrdinalIgnoreCase)) {
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

				if (commandBase.IsFlagSet(FCvar.Cheat)) {
					if (!Host.IsSinglePlayerGame() && !Host.CanCheat()) {
						// TODO; allow server to run it...
						Dbg.Msg($"Can't use cheat command {commandBase.GetName()} in multiplayer, unless the server has sv_cheats set to 1.\n");
						return null;
					}
				}

				if (commandBase.IsFlagSet(FCvar.SingleplayerOnly)) {
					if (!Host.IsSinglePlayerGame()) {
						Dbg.Msg($"Can't use command {commandBase.GetName()} in multiplayer.\n");
						return null;
					}
				}

				// development only stuff

				Dispatch(commandBase, in command);
				return commandBase;
			}
		}

		if (commandBase != null && source == CommandSource.Command && CommandLine.CheckParm("-default") && !commandBase.IsFlagSet(FCvar.ExecDespiteDefault)) {
			Dbg.Msg($"Ignoring cvar \"{commandBase.GetName()}\" due to -default on command line\n");
			return null;
		}

		if (cv.IsCommand(command))
			return commandBase;

		if (source == CommandSource.Command) {
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

	private void Dispatch(ConCommandBase commandBase, in TokenizedCommand command) {
		ConCommand conCommand = (ConCommand)commandBase;
		conCommand.Dispatch(in command);
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

	[ConCommand(helpText: "Parses and stuffs command line + commands to command buffer.")]
	void stuffcmds(in TokenizedCommand args) {
		if (args.ArgC() != 1) {
			Dbg.ConMsg("stuffcmds: execute command line parameters\n");
			return;
		}

		string build = "";
		for (int i = 1; i < CommandLine.ParmCount(); i++) {
			ReadOnlySpan<char> parm = CommandLine.GetParm(i);
			if (parm == null || parm.Length == 0) continue;

			if (parm[0] == '-') {
				ReadOnlySpan<char> value = CommandLine.ParmValueByIndex(i);
				if (value != null)
					i++;
				continue;
			}
			if (parm[0] == '+') {
				ReadOnlySpan<char> value = CommandLine.ParmValueByIndex(i);
				if (value != null && value.Length > 0) {
					build += ($"{parm[1..]} {value}\n");
					i++;
				}
				else {
					build += new string(parm[1..]);
					build += ('\n');
				}
			}
			else {
				ReadOnlySpan<char> translated = TranslateFileAssociation(CommandLine.GetParm(i));
				if (translated != null) {
					build += new string(translated);
					build += ('\n');
				}
			}
		}
		build += ('\0');
		if (build.Length > 0)
			Cbuf.InsertText(build);
	}

	static bool IsValidFileExtension(ReadOnlySpan<char> filename) {
		if (filename == null)
			return false;

		if (
			filename.Contains(".exe", StringComparison.OrdinalIgnoreCase) ||
			filename.Contains(".vbs", StringComparison.OrdinalIgnoreCase) ||
			filename.Contains(".com", StringComparison.OrdinalIgnoreCase) ||
			filename.Contains(".bat", StringComparison.OrdinalIgnoreCase) ||
			filename.Contains(".dll", StringComparison.OrdinalIgnoreCase) ||
			filename.Contains(".ini", StringComparison.OrdinalIgnoreCase) ||
			filename.Contains(".gcf", StringComparison.OrdinalIgnoreCase) ||
			filename.Contains(".sys", StringComparison.OrdinalIgnoreCase) ||
			filename.Contains(".blob", StringComparison.OrdinalIgnoreCase)
		)
			return false;

		return true;
	}

	[ConCommand(helpText: "Execute script file.")]
	void exec(in TokenizedCommand args) {
		lock (Cbuf.Buffer) {
			int argc = args.ArgC();
			if (argc != 2) {
				Dbg.ConMsg("exec <filename>: execute a script file\n");
				return;
			}

			ReadOnlySpan<char> file = $"cfg/{args[1]}";
			ReadOnlySpan<char> pathID = "MOD";

			if (!COM.IsValidPath(file)) {
				Dbg.ConMsg("exec %s: invalid path.\n");
				return;
			}

			if (!IsValidFileExtension(file)) {
				Dbg.ConMsg("exec %s: invalid file type.\n");
				return;
			}

			if (true) {
				if (fileSystem.FileExists(file)) {
					long size = fileSystem.Size(file);
					if (size > 1 * 1024 * 1024) {
						Dbg.ConMsg($"exec {file}: file size larger than 1 MiB!\n");
						return;
					}
				}
				else {
					Dbg.ConMsg($"'{file}' not present; not executing.\n");
					return;
				}
			}

			using var execFile = fileSystem.Open(file, FileOpenOptions.Read, pathID);
			using StreamReader reader = new StreamReader(execFile!.Stream);
			Dbg.ConDMsg($"execing {file}\n");
			LinkedListNode<CommandBuffer.Command>? hCommand = Cbuf.Buffer.GetNextCommandHandle();
			// check to make sure we're not going to overflow later
			while (!reader.EndOfStream) {
				string? line = reader.ReadLine().Trim() + "\0";
				if (line == null)
					break;

				Cbuf.InsertText(line);

				while (Cbuf.Buffer.GetNextCommandHandle() != hCommand) {
					if (Cbuf.Buffer.DequeueNextCommand()) {
						Cbuf.ExecuteCommand(Cbuf.Buffer.GetCommand(), CommandSource.Command);
					}
					else {
						Dbg.Assert(false);
						break;
					}
				}
			}
		}
	}

	private ReadOnlySpan<char> TranslateFileAssociation(string v) {
		// do later!
		return null;
	}
}
