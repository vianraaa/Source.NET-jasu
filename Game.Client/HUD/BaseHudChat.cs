using Source.GUI.Controls;

namespace Game.Client.HUD;

public class BaseHudChatLine : RichText
{
	public BaseHudChatLine(Panel? parent, string? name) : base(parent, name) {

	}
}

public class HudChatHistory : RichText
{
	public HudChatHistory(Panel? parent, string? name) : base(parent, name) {

	}
}

public class BaseHudChatInputLine : Panel {

}

public class HudChatFilterButton : Button
{
	public HudChatFilterButton(Panel parent, string name, string text) : base(parent, name, text) {
	}
}

public class HudChatFilterPanel : EditablePanel
{
	public HudChatFilterPanel(Panel? parent, string? panelName) : base(parent, panelName) {

	}
}

public class BaseHudChatEntry : TextEntry
{
	public BaseHudChatEntry(Panel? parent, string? name, Panel chat) : base(parent, name) {

	}
}

public class BaseHudChat : EditableHudElement {
	public const int CHAT_INTERFACE_LINES = 6;
	public const int MAX_CHARS_PER_LINE = 128;

	public BaseHudChat(string? elementName) : base("HudChat", elementName) {
		
	}
}

[DeclareHudElement(Name = "CHudChat")]
public class HudChat : BaseHudChat
{
	public HudChat(string? panelName) : base(panelName) {
	}
}