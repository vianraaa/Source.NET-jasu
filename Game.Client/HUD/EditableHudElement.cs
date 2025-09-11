using Source.GUI.Controls;

namespace Game.Client.HUD;

public class EditableHudElement : EditablePanel, IHudElement
{
	public string? ElementName { get; set; }
	public HideHudBits HiddenBits { get; set; }
	public bool Active { get; set; }
	public bool NeedsRemove { get; set; }
	public bool IsParentedToClientDLLRootPanel { get; set; }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="panelName">Panel name comes from the overall name</param>
	/// <param name="elementName">Element name comes from the constructor as its single argument</param>
	public EditableHudElement(string? panelName, string? elementName) : base(null, panelName) {
		ElementName = elementName;
	}

	public virtual void Init() { }
}

public class HudNumericDisplay : Panel, IHudElement
{
	public string? ElementName { get; set; }
	public HideHudBits HiddenBits { get; set; }
	public bool Active { get; set; }
	public bool NeedsRemove { get; set; }
	public bool IsParentedToClientDLLRootPanel { get; set; }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="panelName">Panel name comes from the overall name</param>
	/// <param name="elementName">Element name comes from the constructor as its single argument</param>
	public HudNumericDisplay(string? panelName, string? elementName) : base(null, panelName) {
		ElementName = elementName;
	}

	public virtual void Init() { }
}
