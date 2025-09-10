using Game.Shared;

namespace Game.Client.HUD;


public enum HideHudBits
{
	WeaponSelection = 1 << 0,
	Flashlight = 1 << 1,
	All = 1 << 2,
	Health = 1 << 3,
	PlayerDead = 1 << 4,
	NeedSuit = 1 << 5,
	MiscStatus = 1 << 6,
	Chat = 1 << 7,
	Crosshair = 1 << 8,
	VehicleCrosshair = 1 << 9,
	InVehicle = 1 << 10,
	BonusProgress = 1 << 11
}

// Since C# can't do multiple inheritance, this interface acts as the bridge, with other sub-panel types "EditableHudElement" for example
// being the actual base class
public interface IHudElement {
	string? ElementName { get; }
	HideHudBits HiddenBits { get;  }
	bool Active { get; }
	bool NeedsRemove { get; set; }
	bool IsParentedToClientDLLRootPanel { get; set; }

	void Init();

	public static void HookMessage(ReadOnlySpan<char> message, UserMessageHook hookFn) => Singleton<UserMessages>().HookMessage(message, hookFn);
}