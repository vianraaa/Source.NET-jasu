using System.Diagnostics;

namespace Source.Common.Commands;

public class ConVar : ConCommandBase, IConVar
{
	internal ConVar? parent;
	
	string defaultValue = "";
	
	string? value;
	double doubleValue;
	int intValue;

	bool hasMin;
	double minVal;
	bool hasMax;
	double maxVal;

	public event FnChangeCallback? Changed;

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

		Changed += callback;

		doubleValue = double.TryParse(value, out var dRes) ? dRes : 0;
		intValue = int.TryParse(value, out var iRes) ? iRes : 0;

		Dbg.Assert(!hasMin || doubleValue >= minVal);
		Dbg.Assert(!hasMax || doubleValue <= maxVal);

		CreateBase(name, helpString, flags);
	}

	public ConVar(string name, string defaultValue, FCvar flags) {
		Create(name, defaultValue, flags);
	}
	public ConVar(string name, string defaultValue, FCvar flags, string helpText) {
		Create(name, defaultValue, flags, helpText);
	}
	public ConVar(string name, string defaultValue, FCvar flags, string helpText, double? min = null, double? max = null, FnChangeCallback? callback = null) {
		Create(name, defaultValue, flags, helpText, min.HasValue, min ?? 0, max.HasValue, max ?? 0, callback);
	}

	public void SetDefault(string? def) {
		defaultValue = def ?? "";
		Dbg.Assert(defaultValue != null);
	}

	public double GetDouble() => parent!.doubleValue;
	public int GetInt() => parent!.intValue;
	public string GetString() => parent!.value ?? "";

	public override bool IsFlagSet(FCvar flag) {
		return (flag & parent!.Flags) == flag;
	}

	public override string? GetHelpText() {
		return parent!.HelpString;
	}

	public override void AddFlags(FCvar flags) {
		parent!.Flags |= flags;
	}

	public override bool IsRegistered() {
		return parent!.Registered;
	}

	public override string GetName() {
		return parent!.Name;
	}

	public override bool IsCommand() {
		return false;
	}

	public bool ClampValue(ref double d) {
		if (hasMin && (d < minVal)) {
			d = minVal;
			return true;
		}
		if (hasMax && (d > maxVal)) {
			d = maxVal;
			return true;
		}
		return false;
	}

	void InternalSetValue(string? value) {
		double dNewValue = double.TryParse(value, out double d) ? d : 0;
		if (ClampValue(ref dNewValue)) {
			value = $"{dNewValue:.4}";
		}

		double oldValue = doubleValue;
		doubleValue = dNewValue;
		intValue = (int)(float)(dNewValue); // tryparse later???

		if ((Flags & FCvar.NeverAsString) != FCvar.NeverAsString)
			ChangeStringValue(value, oldValue);
	}

	private void ChangeStringValue(string? tempValue, double dbOldValue) {
		string? oldValue = value;

		value = tempValue;
		if (oldValue?.Equals(value) ?? (oldValue != value)) {
			Changed?.Invoke(this, new() {
				Old = oldValue,
				New = value
			});

			// How do we link up global change callbacks? An interesting dilemma...
		}
	}

	public void SetValue(string value) {

	}
	public void SetValue(int value) {

	}
	public void SetValue(float value) {

	}
	public void SetValue(double value) {

	}
}