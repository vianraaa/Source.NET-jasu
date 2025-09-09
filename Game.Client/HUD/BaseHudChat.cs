using Source;
using Source.Common.Bitbuffers;
using Source.Common.Client;
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

public enum ChatFilters {
	None,
	JoinLeave = 0x000001,
	NameChange = 0x000002,
	PublicChat = 0x000004,
	ServerMsg = 0x000008,
	TeamChange = 0x000010,
	Achievement = 0x000020,
}

public enum TextColor
{
	Normal = 1,
	UseOldColors = 2,
	PlayerName = 3,
	Location = 4,
	Achievement = 5,
	Custom = 6,       
	HexCode = 7,      
	HexCodeAlpha = 8,
	Max
}

public struct TextRange {
	public int Start;
	public int End;
	public ColorChange Color;
	public bool PreserveAlpha;
}

public class BaseHudChat : EditableHudElement {
	readonly IEngineClient engine = Singleton<IEngineClient>();
	public const int CHAT_INTERFACE_LINES = 6;
	public const int MAX_CHARS_PER_LINE = 128;

	public BaseHudChat(string? elementName) : base("HudChat", elementName) {
		
	}

	protected void SayText(bf_read msg) {
		Span<char> str = stackalloc char[256];

		int client = msg.ReadByte();
		msg.ReadString(str);
		bool wantsToChat = msg.ReadByte() != 0;

		if (wantsToChat) 
			ChatPrintf(client, ChatFilters.None, str);
		else 
			Printf(ChatFilters.None, str);
	}

	private void Printf(ChatFilters none, ReadOnlySpan<char> str) {

	}

	private void ChatPrintf(int playerIndex, ChatFilters none, ReadOnlySpan<char> str) {
		Span<char> msg = stackalloc char[4096];

		PlayerInfo playerInfo = new();
		if(playerIndex == 0) 
			strcpy(playerInfo.Name, "Console");
		else
			engine.GetPlayerInfo(playerIndex, out playerInfo);

		Msg($"{((ReadOnlySpan<char>)playerInfo.Name).SliceNullTerminatedString()}: {str}\n");
	}

	protected void SayText2(bf_read msg) {

	}

	protected void TextMsg(bf_read msg) {

	}
}

[DeclareHudElement(Name = "CHudChat")]
public class HudChat : BaseHudChat
{
	public HudChat(string? panelName) : base(panelName) {

	}
	public override void Init() {
		base.Init();

		IHudElement.HookMessage("SayText", SayText);
		IHudElement.HookMessage("SayText2", SayText2);
		IHudElement.HookMessage("TextMsg", TextMsg);
	}
}