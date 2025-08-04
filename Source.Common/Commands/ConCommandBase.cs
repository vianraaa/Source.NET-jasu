using System.Reflection;

namespace Source.Common.Commands;

public class ConCommandBase
{
	public ConCommandBase() {
		Registered = false;
		Name = null;
		HelpString = null;
		Flags = 0;
		Next = null;
	}

	public ConCommandBase(string name, string? helpString = null, ConVarFlags flags = 0) {
		CreateBase(name, helpString, flags);
	}

	public virtual bool IsCommand() => false;
	public bool IsFlagSet(ConVarFlags flag) => (Flags & flag) == flag;
	public void AddFlags(ConVarFlags flags) => Flags |= flags;
	public string GetName() => Name;
	public string? GetHelpText() => HelpString;
	public ConCommandBase? GetNext() => Next;
	public bool IsRegistered() => Registered;

	protected virtual void CreateBase(string name, string? helpString = null, ConVarFlags flags = 0) {
		Registered = false;
		Name = name;
		HelpString = helpString;
		Flags = flags;
		if ((flags & ConVarFlags.Unregistered) != ConVarFlags.Unregistered) {
			Next = ConCommandBases;
			ConCommandBases = this;
		}
		else {
			Next = null;
		}
	}

	internal ConCommandBase? Next;
	internal bool Registered;
	internal string Name;
	internal string? HelpString;
	internal ConVarFlags Flags;
	internal Assembly? Assembly;
	protected static ConCommandBase? ConCommandBases;
}