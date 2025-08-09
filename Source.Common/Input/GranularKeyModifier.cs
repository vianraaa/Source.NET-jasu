namespace Source.Common.Input;

/// <summary>
/// Granular (sided) key modifiers. Is a bit-field.
/// </summary>
public enum GranularKeyModifier
{
	CapsLock = 1 << 0,
	ShiftR = 1 << 1,
	ShiftL = 1 << 2,
	ControlR = 1 << 3,
	ControlL = 1 << 4,
	AltR = 1 << 5,
	AltL = 1 << 6,
	OptionR = AltR,
	OptionL = AltL,
	CommandR = 1 << 7,
	CommandL = 1 << 8
}
