using Game.Shared;

using Source.Common.Bitbuffers;

namespace Game.Client.HUD;

/// <summary>
/// Declares that this class is to be a HUD element.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DeclareHudElementAttribute : Attribute
{
	/// <summary>
	/// Optional class name override. By default, the name is pulled from the type - but in some instances where HL2/original Source names are needed,
	/// this will override the element name.
	/// </summary>
	public string? Name { get; set; }
}