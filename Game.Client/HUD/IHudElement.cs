using Game.Shared;

namespace Game.Client.HUD;

// Since C# can't do multiple inheritance, this interface acts as the bridge, with other sub-panel types "EditableHudElement" for example
// being the actual base class
public interface IHudElement {
	string? ElementName { get; }
	int HiddenBits{ get;  }
	bool Active { get; }
	bool NeedsRemove { get; set; }
	bool IsParentedToClientDLLRootPanel { get; set; }

	void Init();

	public static void HookMessage(ReadOnlySpan<char> message, UserMessageHook hookFn) => Singleton<UserMessages>().HookMessage(message, hookFn);
}