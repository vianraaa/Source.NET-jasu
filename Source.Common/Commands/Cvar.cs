using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Source.Common.Commands;

public class Cvar(ICommandLine CommandLine) : ICvar
{
	public event FnChangeCallback? Changed;
	List<IConsoleDisplayFunc> DisplayFuncs = [];
	Assembly? NextDLLIdentifier;
	ConCommandBase? ConCommandList;

	public void SetAssemblyIdentifier(Assembly assembly) {
		NextDLLIdentifier = assembly;
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
}
