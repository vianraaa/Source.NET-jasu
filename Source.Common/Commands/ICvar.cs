using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Source.Common.Commands;

public delegate void FnChangeCallback(IConVar var, in ConVarChangeContext ctx);

public interface IConsoleDisplayFunc {
	public void ColorPrint(in Color clr, string message);
	public void Print(string message);
	public void DPrint(string message);
}

public interface ICvar
{
	public void RegisterConCommand(ConCommandBase commandBase);
	public void UnregisterConCommand(ConCommandBase commandBase);
	public void UnregisterConCommands(Assembly sourceAssembly);

	public void SetAssemblyIdentifier(Assembly assembly);

	public string? GetCommandLineValue(string variableName);

	public ConCommandBase? FindCommandBase(string name);
	public ConVar? FindVar(string name);
	public ConCommand FindCommand(string name);

	public IEnumerable<ConCommandBase> GetCommands();

	public event FnChangeCallback? Changed;

	public void InstallConsoleDisplayFunc(IConsoleDisplayFunc displayFunc);
	public void RemoveConsoleDisplayFunc(IConsoleDisplayFunc displayFunc);

	public void ConsoleColorPrintf(in Color clr, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[]? args);
	public void ConsolePrintf([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[]? args);
	public void ConsoleDPrintf([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[]? args);


}
