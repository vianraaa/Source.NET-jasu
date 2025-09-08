using Source.GUI.Controls;

namespace Game.Client.HUD;

public class EditableHudElement : EditablePanel, IHudElement {
	public string? ElementName { get; set; }
	public int HiddenBits { get; set; }
	public bool Active { get; set; }
	public bool NeedsRemove { get; set; }
	public bool IsParentedToClientDLLRootPanel { get; set; }

	public EditableHudElement(string? panelName, string? elementName) : base(null, panelName) {
		ElementName = elementName;
	}

	public virtual void Init() { }
}
