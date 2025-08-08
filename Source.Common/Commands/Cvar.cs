using Microsoft.Extensions.DependencyInjection;

using Source.Common.Client;
using Source.Common.Server;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Source.Common.Commands;

public class Cvar(ICommandLine CommandLine, IServiceProvider services) : ICvar
{
	public event FnChangeCallback? Changed;
	List<IConsoleDisplayFunc> DisplayFuncs = [];
	Assembly? NextDLLIdentifier;
	ConCommandBase? ConCommandList;

	IBaseClientDLL? clientDLL;
	IServerGameDLL? serverDLL;

	FCvar assemblyFlags = FCvar.None;
	public void SetAssemblyIdentifier(Assembly assembly) {
		NextDLLIdentifier = assembly;

		// Pull in dependencies if they weren't resolved already
		// done independently from ctor to avoid any headaches in the client/game dlls
		clientDLL ??= services.GetService<IBaseClientDLL>();
		serverDLL ??= services.GetService<IServerGameDLL>();
		assemblyFlags = FCvar.None;

		if (assembly != null) {
			if (clientDLL != null && assembly == clientDLL.GetType().Assembly)
				assemblyFlags |= FCvar.ClientDLL;
			else if (serverDLL != null && assembly == serverDLL.GetType().Assembly)
				assemblyFlags |= FCvar.GameDLL;
		}
	}

	public void ConsoleColorPrintf(in Color clr, [StringSyntax("CompositeFormat")] string format, params object?[]? args) {
		foreach (var func in DisplayFuncs)
			func.ColorPrint(in clr, args == null ? format : string.Format(format, args));
	}

	public void ConsoleDPrintf([StringSyntax("CompositeFormat")] string format, params object?[]? args) {
		foreach (var func in DisplayFuncs)
			func.DPrint(args == null ? format : string.Format(format, args));
	}

	public void ConsolePrintf([StringSyntax("CompositeFormat")] string format, params object?[]? args) {
		foreach (var func in DisplayFuncs)
			func.Print(args == null ? format : string.Format(format, args));
	}

	public ConCommand? FindCommand(ReadOnlySpan<char> name) {
		ConCommandBase? var = FindCommandBase(name);
		if (var == null || !var.IsCommand())
			return null;

		return (ConCommand)var!;
	}

	public ConCommandBase? FindCommandBase(ReadOnlySpan<char> name) {
		foreach (var cmd in GetCommands())
			if (cmd.Name.AsSpan().Equals(name, StringComparison.OrdinalIgnoreCase))
				return cmd;

		return null;
	}

	public ConVar? FindVar(ReadOnlySpan<char> name) {
		ConCommandBase? var = FindCommandBase(name);
		if (var == null || var.IsCommand())
			return null;

		return (ConVar)var!;
	}

	public string? GetCommandLineValue(ReadOnlySpan<char> variableName) {
		return CommandLine.ParmValue($"+{variableName}", null);
	}

	public IEnumerable<ConCommandBase> GetCommands() {
		ConCommandBase? command = ConCommandList;
		while (command != null) {
			yield return command;
			command = command.Next;
		}
	}

	public void InstallConsoleDisplayFunc(IConsoleDisplayFunc displayFunc) {
		DisplayFuncs.Add(displayFunc);
	}

	public void RegisterConCommand(ConCommandBase variable) {
		if (variable.IsRegistered())
			return;

		variable.Registered = true;

		string name = variable.GetName();
		if (string.IsNullOrEmpty(name)) {
			variable.Next = null;
			return;
		}

		variable.Flags |= assemblyFlags;

		ConCommandBase? other = FindVar(variable.GetName());
		if (other != null) {
			if (variable.IsCommand() || other.IsCommand()) {
				Dbg.Warning($"WARNING: unable to link {variable.GetName()} and {other.GetName()} because one or more is a ConCommand.\n");
			}
			else {
				ConVar childVar = (ConVar)variable;
				ConVar parentVar = (ConVar)other;
				Dbg.Warning("Linking ConVar's is currently not implemented...\n");
				throw new Exception();
			}

			variable.Next = null;
			return;
		}

		variable.Next = ConCommandList;
		ConCommandList = variable;

		variable.Assembly = NextDLLIdentifier;
	}

	public void RemoveConsoleDisplayFunc(IConsoleDisplayFunc displayFunc) {
		DisplayFuncs.Remove(displayFunc);
	}

	public void UnregisterConCommand(ConCommandBase commandToRemove) {
		if (!commandToRemove.IsRegistered())
			return;

		commandToRemove.Registered = false;

		ConCommandBase? prev = null;
		for (ConCommandBase? command = ConCommandList; command != null; command = command.Next) {
			if (command != commandToRemove) {
				prev = command;
				continue;
			}

			if (prev == null)
				ConCommandList = command.Next;
			else
				prev.Next = command.Next;

			command.Next = null;
			break;
		}
	}

	public void UnregisterConCommands(Assembly sourceAssembly) {
		ConCommandBase? newList = null;
		ConCommandBase? command = ConCommandList;
		while (command != null) {
			ConCommandBase? next = command.Next;
			if (command.Assembly != sourceAssembly) {
				command.Next = newList;
				newList = command;
			}
			else {
				command.Registered = false;
				command.Next = null;
			}

			command = next;
		}

		ConCommandList = newList;
	}

	[ConCommand(helpText: "Show the list of convars/concommands.")]
	void cvarlist() {
		Dbg.ConMsg("cvar list\n--------------\n");
		int count = 0;
		ConCommandBase? command = ConCommandList;
		List<ConCommandBase> cmds = [];
		while (command != null) {
			cmds.Add(command);
			command = command.Next;
		}

		cmds.Sort((x, y) => x.Name.CompareTo(y.Name));
		foreach (var cmd in cmds) {
			if (cmd.IsCommand())
				PrintCommand((ConCommand)cmd);
			else
				PrintCvar((ConVar)cmd);
			count++;
		}

		Dbg.ConMsg($"--------------\n{count} total convars/concommands\n");
	}

	[ConCommand(helpText: "Find help about a convar/concommand.")]
	void help(in TokenizedCommand args) {
		if(args.ArgC() != 2) {
			Dbg.ConMsg("Usage:  help <cvarname>\n");
			return;
		}

		ConCommandBase? var = FindCommandBase(args[1]);
		if(var == null) {
			Dbg.ConMsg($"help:  no cvar or command named {args[1]}\n");
			return;
		}

		ConVar.PrintDescription(var);
	}

	struct ConVarFlagsDesc
	{
		public FCvar bit;
		public string desc;
		public string shortdesc;
		public ConVarFlagsDesc(FCvar bit, string desc, string shortdesc) {
			this.bit = bit;
			this.desc = desc;
			this.shortdesc = shortdesc;
		}
	}
	static ConVarFlagsDesc[] conVarFlags =
	{
		new(FCvar.Archive,  "ARCHIVE", "a" ),
		new(FCvar.SingleplayerOnly,  "SPONLY", "sp" ),
		new(FCvar.GameDLL,  "GAMEDLL", "sv" ),
		new(FCvar.Cheat,  "CHEAT", "cheat" ),
		new(FCvar.UserInfo,  "USERINFO", "user" ),
		new(FCvar.Notify,  "NOTIFY", "nf" ),
		new(FCvar.Protected,  "PROTECTED", "prot" ),
		new(FCvar.PrintableOnly,  "PRINTABLEONLY", "print" ),
		new(FCvar.Unlogged,  "UNLOGGED", "log" ),
		new(FCvar.NeverAsString,  "NEVER_AS_STRING", "numeric" ),
		new(FCvar.Replicated,  "REPLICATED", "rep" ),
		new(FCvar.Demo,  "DEMO", "demo" ),
		new(FCvar.DontRecord,  "DONTRECORD", "norecord" ),
		new(FCvar.ServerCanExecute,  "SERVER_CAN_EXECUTE", "server_can_execute" ),
		new(FCvar.ClientCmdCanExecute,  "CLIENTCMD_CAN_EXECUTE", "clientcmd_can_execute" ),
		new(FCvar.ClientDLL, "CLIENTDLL", "cl" ),
	};

	private void PrintCvar(ConVar var) {
		string[] flagstr = new string[conVarFlags.Length];
		int i = 0;
		foreach (var entry in conVarFlags) {
			if (var.IsFlagSet(entry.bit)) {
				flagstr[i++] = entry.shortdesc;
			}
		}
		string flagstrF = string.Join(", ", flagstr[0..i]);
		string valstr;
		if (var.GetInt() == Convert.ToInt32(var.GetDouble()))
			valstr = $"{var.GetInt()}";
		else
			valstr = $"{Math.Round(var.GetDouble(), 3)}";

		Dbg.ConMsg($"{var.Name.PadRight(40)} : {valstr.PadRight(8)} : {flagstrF.PadRight(16)} : {var.GetHelpText()?.Replace("\t", "")?.Replace("\n", "")}\n");
	}

	private void PrintCommand(ConCommand cmd) {
		Dbg.ConMsg($"{cmd.Name.PadRight(40)} : {"cmd".PadRight(8)} : {"".PadRight(16)} : {cmd.GetHelpText()?.Replace("\t", "")?.Replace("\n", "")}\n");
	}
}
