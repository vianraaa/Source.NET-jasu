using System.Diagnostics;

namespace Source.Common.Commands;

public class ConVar : ConCommandBase, IConVar
{
	public override bool Equals(object? obj) {
		if(obj == null) return false;
		return obj is ConVar cv ? cv.Name == Name : false;
	}
	public override int GetHashCode() => Name.GetHashCode();
	public override string ToString() {
		return $"ConVar '{Name}'"
			+ (IsFlagSet(FCvar.NeverAsString) ? "" : 
			($"[{value}" + ((defaultValue != value) ? $", default {defaultValue}" : "]")));
	}

	internal ConVar? parent;
	public bool HasChangeCallback => Changed != null;

	internal string defaultValue = "";

	string? value;
	double doubleValue;
	int intValue;

	bool hasMin;
	double minVal;
	bool hasMax;
	double maxVal;

	public event FnChangeCallback? Changed;

	/// <summary>
	/// Initializes the convar parameters.
	/// </summary>
	/// <param name="name">Convar name, if null, means that it MUST be resolved by the DI system later.</param>
	/// <param name="defaultValue"></param>
	/// <param name="flags"></param>
	/// <param name="helpString"></param>
	/// <param name="useMin"></param>
	/// <param name="min"></param>
	/// <param name="useMax"></param>
	/// <param name="max"></param>
	/// <param name="callback"></param>
	public void Create(string? name, string defaultValue, FCvar flags, string? helpString = null,
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

#pragma warning disable CS8604 // Possible null reference argument. 
		// Note: the warning is suppressed here because null is only expected as an argument
		// in this specific context, and any other context should make things complain.
		CreateBase(name, helpString, flags);
#pragma warning restore CS8604
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

	public ConVar(string defaultValue, FCvar flags) {
		Create(null, defaultValue, flags);
	}
	public ConVar(string defaultValue, FCvar flags, string helpText) {
		Create(null, defaultValue, flags, helpText);
	}
	public ConVar(string defaultValue, FCvar flags, string helpText, double? min = null, double? max = null, FnChangeCallback? callback = null) {
		Create(null, defaultValue, flags, helpText, min.HasValue, min ?? 0, max.HasValue, max ?? 0, callback);
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

	void InternalSetValue(ReadOnlySpan<char> value) {
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

	private void ChangeStringValue(ReadOnlySpan<char> tempValue, double dbOldValue) {
		string? oldValue = value;

		value = new(tempValue);
		if (oldValue?.Equals(value) ?? (oldValue != value)) {
			Changed?.Invoke(this, new() {
				Old = oldValue,
				New = value
			});

			// How do we link up global change callbacks? An interesting dilemma...
		}
	}

	public void SetValue(ReadOnlySpan<char> value) {
		parent!.InternalSetValue(value);
	}
	public void SetValue(int value) {
		parent!.InternalSetIntValue(value);
	}
	public void SetValue(float value) {
		parent!.InternalSetDoubleValue(value);
	}
	public void SetValue(double value) {
		parent!.InternalSetDoubleValue(value);
	}

	private void InternalSetIntValue(int value) {
		if (value == intValue)
			return;

		Debug.Assert(parent == this);
		double dbValue = value;
		if (ClampValue(ref dbValue))
			value = Convert.ToInt32(dbValue);

		double oldValue = doubleValue;
		doubleValue = dbValue;
		intValue = value;

		if ((Flags & FCvar.NeverAsString) != FCvar.NeverAsString) {
			Span<char> tempVal = stackalloc char[64];
			intValue.TryFormat(tempVal, out int charsWritten);
			ChangeStringValue(tempVal[..charsWritten], oldValue);
		}
	}

	private void InternalSetDoubleValue(double value) {
		if (value == intValue)
			return;

		Debug.Assert(parent == this);

		ClampValue(ref value);
		double oldValue = doubleValue;
		doubleValue = value;
		intValue = Convert.ToInt32(doubleValue);

		if ((Flags & FCvar.NeverAsString) != FCvar.NeverAsString) {
			Span<char> tempVal = stackalloc char[64];
			intValue.TryFormat(tempVal, out int charsWritten);
			ChangeStringValue(tempVal[..charsWritten], oldValue);
		}
	}

	public bool GetBool() => GetInt() != 0;

	public static void PrintDescription(ConCommandBase pvar) {
		bool hasMin, hasMax;
		double min, max;
		string? str;

		Color clr = new(255, 100, 100, 255);

		if (!pvar.IsCommand()) {
			ConVar var = (ConVar)pvar;
			// Server bounded convar? need to implement later

			hasMin = var.GetMin(out min);
			hasMax = var.GetMin(out max);

			string value;
			if (false) {

			}
			else {
				value = var.GetString();
			}

			if(value != null) {
				Dbg.ConColorMsg(clr, $"\"{var.GetName()}\" = \"{value}\"");
				if(!value.Equals(var.GetDefault(), StringComparison.OrdinalIgnoreCase)) 
					Dbg.ConMsg($" ( def. \"{var.GetDefault()}\" )");

			}

			if (hasMin) Dbg.ConMsg($" min. {min:.4}");
			if (hasMax) Dbg.ConMsg($" min. {max:.4}");


			Dbg.ConMsg("\n");

			// bounded stuff...
		}
		else {
			ConCommand var = (ConCommand)pvar;
			Dbg.ConColorMsg(clr, $"\"{var.GetName()}\"\n");
		}

		PrintFlags(pvar);

		str = pvar.GetHelpText();
		if (!string.IsNullOrEmpty(str))
			Dbg.ConMsg($" - {str}\n");
	}

	public string GetDefault() {
		return parent!.defaultValue;
	}

	private static void PrintFlags(ConCommandBase var) {
		bool any = false;

		if (var.IsFlagSet(FCvar.GameDLL)) { Dbg.ConMsg(" game"); any = true; }
		if (var.IsFlagSet(FCvar.ClientDLL)) { Dbg.ConMsg(" client"); any = true; }
		if (var.IsFlagSet(FCvar.Archive)) { Dbg.ConMsg(" archive"); any = true; }
		if (var.IsFlagSet(FCvar.Notify)) { Dbg.ConMsg(" notify"); any = true; }
		if (var.IsFlagSet(FCvar.SingleplayerOnly)) { Dbg.ConMsg(" singleplayer"); any = true; }
		if (var.IsFlagSet(FCvar.NotConnected)) { Dbg.ConMsg(" notconnected"); any = true; }
		if (var.IsFlagSet(FCvar.Cheat)) { Dbg.ConMsg(" cheat"); any = true; }
		if (var.IsFlagSet(FCvar.Replicated)) { Dbg.ConMsg(" replicated"); any = true; }
		if (var.IsFlagSet(FCvar.ServerCanExecute)) { Dbg.ConMsg(" server_can_execute"); any = true; }
		if (var.IsFlagSet(FCvar.ClientCmdCanExecute)) { Dbg.ConMsg(" clientcmd_can_execute"); any = true; }

		if (any)
			Dbg.ConMsg("\n");
	}

	private bool GetMin(out double min) {
		min = this.minVal;
		return this.hasMin;
	}
	private bool GetMax(out double max) {
		max = this.maxVal;
		return this.hasMax;
	}

	public void Revert() {
		parent!.SetValue(parent.defaultValue);
	}

	public void SyncChangeTo(ConVar childVar) {
		Changed = childVar.Changed;
	}
}