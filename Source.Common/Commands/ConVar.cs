using System.Diagnostics;

namespace Source.Common.Commands;

public class ConVar : ConCommandBase {
	internal ConVar? parent;
	
	string defaultValue = "";
	
	string value;
	double doubleValue;
	int intValue;

	bool hasMin;
	double minVal;
	bool hasMax;
	double maxVal;

	FnChangeCallback? changeCallback;

	public void Create(string name, string defaultValue, FCvar flags, string? helpString = null, 
		bool useMin = false, double min = 0, bool useMax = false, double max = 0,
		FnChangeCallback? callback = null) {
		parent = this;
		SetDefault(defaultValue);
		value = defaultValue;

		hasMin = useMin; 
		hasMax = useMax;
		minVal = min;
		maxVal = max;

		changeCallback = callback;

		doubleValue = double.TryParse(value, out var dRes) ? dRes : 0;
		intValue = int.TryParse(value, out var iRes) ? iRes : 0;

		Dbg.Assert(!hasMin || doubleValue >= minVal);
		Dbg.Assert(!hasMax || doubleValue <= maxVal);

		CreateBase(name, helpString, flags);
	}

	public ConVar(string name, string defaultValue, FCvar flags) {
		Create(name, defaultValue, flags);
	}

	public void SetDefault(string? def) {
		defaultValue = def ?? "";
		Dbg.Assert(defaultValue != null);
	}
}