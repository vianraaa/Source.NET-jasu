namespace Source.Common.Commands;

/// <summary>
/// Indicates the method will be constructed as a concommand.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ConCommandAttribute : Attribute {
	public readonly string Name = "";
	public readonly string? HelpText;
	public readonly FCvar Flags;
	public readonly string? AutoCompleteMethod;
	public ConCommandAttribute(string name, string? helpText = null, FCvar flags = 0, string? autoCompleteMethod = null) {
		Name = name;
		HelpText = helpText;
		Flags = flags;
		AutoCompleteMethod = autoCompleteMethod;
	}	
}