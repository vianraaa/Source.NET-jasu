namespace Source.Common.Input;

/// <summary>
/// Non-granular key modifier (no side information encoded)
/// </summary>
public enum KeyModifier : uint
{
	CapsLock = 1 << 0,
	Shift = 1 << 1,
	Control = 1 << 2,
	Alt = 1 << 3,
	Option = Alt,
	Command = 1 << 4
}
