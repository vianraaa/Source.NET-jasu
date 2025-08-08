using System.Diagnostics.CodeAnalysis;

namespace Source.Common.Commands;

public delegate void FnCommandCallbackVoid();
public delegate void FnCommandCallback(in TokenizedCommand command);
public delegate void FnCommandCallbackSourced(in TokenizedCommand command, CommandSource source, int clientSlot = -1);
public interface ICommandCallback {
	public void CommandCallback(in TokenizedCommand command);
}

public delegate IEnumerable<string> FnCommandCompletionCallback(string partial);
public interface ICommandCompletionCallback {
	public IEnumerable<string> CommandCompletionCallback(string partial);
}

public class ConCommand : ConCommandBase {
	// Source does this stuff with a union but this will work I guess
	// just switch case where appropriate...
	object? callback;
	object? autocomplete;


	public bool HasDispatchCallback => callback != null;
	public bool HasCompletionCallback => autocomplete != null;

	public ConCommand(string name, FnCommandCallbackVoid callback, string? helpString = null, FCvar flags = 0, FnCommandCompletionCallback? completionFunc = null) {
		this.Name = name;
		this.callback = callback;
		this.HelpString = helpString;
		this.Flags = flags;
		this.autocomplete = completionFunc;
	}
	public ConCommand(string name, FnCommandCallback callback, string? helpString = null, FCvar flags = 0, FnCommandCompletionCallback? completionFunc = null) {
		this.Name = name;
		this.callback = callback;
		this.HelpString = helpString;
		this.Flags = flags;
		this.autocomplete = completionFunc;
	}
	public ConCommand(string name, FnCommandCallbackSourced callback, string? helpString = null, FCvar flags = 0, FnCommandCompletionCallback? completionFunc = null) {
		this.Name = name;
		this.callback = callback;
		this.HelpString = helpString;
		this.Flags = flags;
		this.autocomplete = completionFunc;
	}
	public ConCommand(string name, ICommandCallback callback, string? helpString = null, FCvar flags = 0, ICommandCompletionCallback? completionFunc = null) {
		this.Name = name;
		this.callback = callback;
		this.HelpString = helpString;
		this.Flags = flags;
		this.autocomplete = completionFunc;
	}

	public override bool IsCommand() => true;
	public virtual IEnumerable<string> AutoCompleteSuggest(string partial) {
		if (!CanAutoComplete())
			yield break;

		switch (autocomplete) {
			case FnCommandCompletionCallback cb:
				var fnRes = cb(partial);
				foreach (var p in fnRes)
					yield return p;
				break;
			case ICommandCompletionCallback cb:
				var iRes = cb.CommandCompletionCallback(partial);
				foreach (var p in iRes)
					yield return p;
				break;
		}
	}

	[MemberNotNullWhen(true, nameof(callback))]
	public bool CanDispatch() => callback != null;
	[MemberNotNullWhen(true, nameof(autocomplete))]
	public bool CanAutoComplete() => autocomplete != null;

	public void Dispatch(in TokenizedCommand command, CommandSource source, int clientSlot) {
		if (!CanDispatch())
			return;

		switch (callback) {
			case FnCommandCallbackVoid cb: 
				cb(); 
				return;
			case FnCommandCallback cb:
				cb(in command);
				return;
			case FnCommandCallbackSourced cb:
				cb(in command, source, clientSlot);
				return;
			case ICommandCallback cb:
				cb.CommandCallback(in command); 
				return;
		}
	}
}