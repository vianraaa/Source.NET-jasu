using Source;
using Source.Common;
using Source.Common.Bitbuffers;
using Source.Common.Client;
using Source.Common.Commands;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Common.Input;
using Source.Engine;
using Source.GUI;
using Source.GUI.Controls;

namespace Game.Client.HUD;

public class BaseHudChatLine : RichText
{
	protected IFont? Font;

	public BaseHudChatLine(Panel? parent, string? name) : base(parent, name) {

	}

	internal IFont? GetFont() {
		return Font;
	}
}

public class HudChatHistory : RichText
{
	public HudChatHistory(Panel? parent, string? name) : base(parent, name) {

	}

	internal void ResetAllFades(bool hold, bool onlyExpired = false, float newSustain = -1f) {
		throw new NotImplementedException();
	}
}

public class BaseHudChatInputLine : Panel
{
	public Label GetPrompt() => Prompt;
	protected Label Prompt;
	protected BaseHudChatEntry Input;
	public BaseHudChatInputLine(Panel? parent, ReadOnlySpan<char> panelName) : base(parent, panelName) {
		SetMouseInputEnabled(false);
	}

	internal void ClearEntry() {
		SetEntry("");
	}

	public override void PerformLayout() {
		base.PerformLayout();

		GetSize(out int wide, out int tall);

		Prompt.GetContentSize(out int w, out int h);
		Prompt.SetBounds(0, 0, w, tall);
		Input.SetBounds(w + 2, 0, wide - w - 2, tall);
	}

	internal void SetEntry(ReadOnlySpan<char> entry) {
		Input.SetText(entry);
	}

	internal void SetPrompt(ReadOnlySpan<char> prompt) {
		Prompt.SetText(prompt);
		InvalidateLayout();
	}
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
	Panel? HudChat;
	public BaseHudChatEntry(Panel? parent, string? name, Panel chat) : base(parent, name) {
		SetCatchEnterKey(true);
		SetAllowNonAsciiCharacters(true);
		// set draw language id
		HudChat = chat;
	}

	public override void ApplySchemeSettings(IScheme scheme) {
		base.ApplySchemeSettings(scheme);
		SetPaintBorderEnabled(false);
	}

	public override void OnKeyCodeTyped(ButtonCode code) {
		if (code == ButtonCode.KeyEnter || code == ButtonCode.KeyPadEnter || code == ButtonCode.KeyEscape) {
			if (code != ButtonCode.KeyEscape && HudChat != null)
				PostMessage(HudChat, new KeyValues("ChatEntrySend"));

			if (HudChat != null)
				PostMessage(HudChat, new KeyValues("ChatEntryStopMessageMode"));
		}
		else if (code == ButtonCode.KeyTab)
			return;
		else
			base.OnKeyCodeTyped(code);
	}
}

public enum ChatFilters
{
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

public struct TextRange
{
	public int Start;
	public int End;
	public ColorChange Color;
	public bool PreserveAlpha;
}

public class BaseHudChat : EditableHudElement
{
	readonly IEngineClient engine = Singleton<IEngineClient>();
	public const int CHAT_INTERFACE_LINES = 6;
	public const int MAX_CHARS_PER_LINE = 128;

	public const float CHATLINE_NUM_FLASHES = 8;
	public const float CHATLINE_FLASH_TIME = 5;
	public const float CHATLINE_FADE_TIME = 1;

	public const float CHAT_HISTORY_FADE_TIME = 0.25f;
	public const float CHAT_HISTORY_IDLE_TIME = 15;
	public const float CHAT_HISTORY_IDLE_FADE_TIME = 2.5f;
	public const float CHAT_HISTORY_ALPHA = 127;

	static ConVar hud_saytext_time = new("12", 0);
	static ConVar cl_showtextmsg = new("1", 0, "Enable/disable text messages printing on the screen.");
	static ConVar cl_chatfilters = new("63", FCvar.ClientDLL | FCvar.Archive, "Stores the chat filter settings ");
	static ConVar cl_chatfilter_version = new("0", FCvar.ClientDLL | FCvar.Archive | FCvar.Hidden, "Stores the chat filter version");
	static ConVar cl_mute_all_comms = new("1", FCvar.Archive, "If 1, then all communications from a player will be blocked when that player is muted, including chat messages.");

	public static readonly Color ColorBlue = new(153, 204, 255, 255);
	public static readonly Color ColorRed = new(255, 63, 63, 255);
	public static readonly Color ColorGreen = new(153, 255, 153, 255);
	public static readonly Color ColorDarkGreen = new(64, 255, 64, 255);
	public static readonly Color ColorYellow = new(255, 178, 0, 255);
	public static readonly Color ColorGrey = new(204, 204, 204, 255);

	protected BaseHudChatLine? FindUnusedChatLine() {
		return null;
	}

	protected BaseHudChatInputLine ChatInput;
	protected BaseHudChatLine ChatLine;
	protected int FontHeight;
	protected HudChatHistory ChatHistory;
	protected HudChatFilterButton FiltersButton;
	protected HudChatFilterPanel FilterPanel;
	protected Color ColorCustom;

	int ComputeBreakChar(int width, ReadOnlySpan<char> text) {
		BaseHudChatLine line = ChatLine;
		IFont? font = line.GetFont();

		int currentlen = 0;
		int lastbreak = text.Length;
		for (int i = 0; i < text.Length; i++) {
			char ch = text[i];

			if (ch <= 32)
				lastbreak = i;

			Surface.GetCharABCwide(font, ch, out int a, out int b, out int c);
			currentlen += a + b + c;

			if (currentlen >= width) {
				if (lastbreak == text.Length)
					lastbreak = Math.Max(0, i - 1);

				break;
			}
		}

		if (currentlen >= width)
			return lastbreak;

		return text.Length;
	}

	public override void ApplySchemeSettings(IScheme scheme) {
		LoadControlSettings("resource/UI/BaseChat.res");
		base.ApplySchemeSettings(scheme);

		SetPaintBackgroundType(PaintBackgroundType.Box);
		SetPaintBorderEnabled(true);
		SetPaintBackgroundEnabled(true);

		SetKeyboardInputEnabled(false);
		SetMouseInputEnabled(false);

		VisibleHeight = 0;

		Color dullWhite = scheme.GetColor("DullWhite", GetBgColor());
		SetBgColor(dullWhite with { A = (byte)CHAT_HISTORY_ALPHA });

		GetChatHistory().SetVerticalScrollbar(false);
	}

	MessageModeType MessageMode;
	int VisibleHeight;
	IFont? ChatFont;
	int FilterFlags;
	bool EnteringVoice;
	double HistoryFadeTime;
	double HistoryIdleTime;

	readonly IEngineVGui EngineVGui = Singleton<IEngineVGui>();
	readonly ClientGlobalVariables gpGlobals = Singleton<ClientGlobalVariables>();

	public BaseHudChat(string? elementName) : base("HudChat", elementName) {
		Panel parent = HLClient.ClientMode.GetViewport();
		SetParent(parent);

		IScheme? scheme = SchemeManager.LoadSchemeFromFileEx(EngineVGui.GetPanel(VGuiPanelType.ClientDll), "resource/ChatScheme.res", "ChatScheme");
		SetScheme(scheme);

		Localize.AddFile("resource/chat_%language%.txt");

		MessageMode = 0;
		VGui.AddTickSignal(this);

		MakePopup();
		SetZPos(-30);

		HiddenBits = HideHudBits.Chat;

		FiltersButton = new HudChatFilterButton(this, "ChatFiltersButton", "Filters");

		FiltersButton.SetScheme(scheme);
		FiltersButton.SetVisible(true);
		FiltersButton.SetEnabled(true);
		FiltersButton.SetMouseInputEnabled(true);
		FiltersButton.SetKeyboardInputEnabled(false);

		ChatHistory = new HudChatHistory(this, "HudChatHistory");

		CreateChatLines();
		CreateChatInputLine();
		GetChatFilterPanel();

		FilterFlags = cl_chatfilters.GetInt();
	}

	private void CreateChatLines() {
		ChatLine = new BaseHudChatLine(this, "ChatLine1");
		ChatLine.SetVisible(false);
	}

	private void CreateChatInputLine() {
		ChatInput = new BaseHudChatInputLine(this, "ChatInputLine");
		ChatInput.SetVisible(false);

		if (GetChatHistory() != null) {
			GetChatHistory().SetMaximumCharCount(127 * 100);
			GetChatHistory().SetVisible(true);
		}
	}

	private HudChatFilterPanel GetChatFilterPanel() {
		if (FilterPanel == null) {
			FilterPanel = new HudChatFilterPanel(this, "HudChatFilterPanel");

			IScheme? scheme = SchemeManager.LoadSchemeFromFileEx(EngineVGui.GetPanel(VGuiPanelType.ClientDll), "resource/ChatScheme.res", "ChatScheme");

			FilterPanel.SetScheme(scheme);
			FilterPanel.InvalidateLayout(true, true);
			FilterPanel.SetMouseInputEnabled(true);
			FilterPanel.SetPaintBackgroundType(PaintBackgroundType.Box);
			FilterPanel.SetPaintBorderEnabled(true);
			FilterPanel.SetVisible(false);
		}

		return FilterPanel;
	}

	protected void SayText(bf_read msg) {
		Span<char> str = stackalloc char[256];

		int client = msg.ReadByte();
		str = str[..msg.ReadString(str)];
		bool wantsToChat = msg.ReadByte() != 0;

		if (wantsToChat)
			ChatPrintf(client, ChatFilters.None, str);
		else
			Printf(ChatFilters.None, str);
	}

	public HudChatHistory GetChatHistory() => ChatHistory;

	private void Printf(ChatFilters none, ReadOnlySpan<char> str) {

	}

	private void ChatPrintf(int playerIndex, ChatFilters none, ReadOnlySpan<char> str) {
		Span<char> msg = stackalloc char[4096];

		PlayerInfo playerInfo = new();
		if (playerIndex == 0)
			strcpy(playerInfo.Name, "Console");
		else
			engine.GetPlayerInfo(playerIndex, out playerInfo);

		Msg($"{((ReadOnlySpan<char>)playerInfo.Name).SliceNullTerminatedString()}: {str}\n");
	}

	protected void SayText2(bf_read msg) {

	}

	protected void TextMsg(bf_read msg) {

	}

	internal void StartMessageMode(MessageModeType messageModeType) {
		MessageMode = messageModeType;
		ChatInput.ClearEntry();

		ReadOnlySpan<char> prompt = MessageMode == MessageModeType.Say ? Localize.Find("#chat_say") : Localize.Find("#chat_say_team");
		if (prompt != null)
			ChatInput.SetPrompt(prompt);
		else
			ChatInput.SetPrompt(MessageMode == MessageModeType.Say ? "Say : " : "Say (TEAM) :");

		if (GetChatHistory() != null) {
			GetChatHistory().SetMouseInputEnabled(true);
			GetChatHistory().SetKeyboardInputEnabled(false);
			GetChatHistory().SetVerticalScrollbar(true);
			GetChatHistory().ResetAllFades(true);
			GetChatHistory().SetPaintBorderEnabled(true);
			GetChatHistory().SetVisible(true);
		}

		MakeReadyForUse();
		SetKeyboardInputEnabled(true);
		SetMouseInputEnabled(true);

		ChatInput.SetVisible(true);
		Surface.CalculateMouseVisible();
		ChatInput.RequestFocus();
		ChatInput.SetPaintBorderEnabled(true);
		ChatInput.SetMouseInputEnabled(true);

		GetChatHistory().GetBounds(out int x, out int y, out int w, out int h);
		Input.SetCursorPos(x + (w / 2), y + (h / 2));

		HistoryFadeTime = gpGlobals.CurTime + CHAT_HISTORY_FADE_TIME;

		FilterPanel.SetVisible(false);

		engine.ClientCmd_Unrestricted("gameui_preventescapetoshow\n");
	}

	public void StopMessageMode() {
		engine.ClientCmd_Unrestricted("gameui_allowescapetoshow\n");

		SetKeyboardInputEnabled(false);
		SetMouseInputEnabled(false);

		if (GetChatHistory() != null) {
			GetChatHistory().SetPaintBorderEnabled(false);
			GetChatHistory().GotoTextEnd();
			GetChatHistory().SetMouseInputEnabled(false);
			GetChatHistory().SetVerticalScrollbar(false);
			GetChatHistory().ResetAllFades(false, true, CHAT_HISTORY_FADE_TIME);
			GetChatHistory().SelectNoText();
		}

		ChatInput.ClearEntry();
		FilterPanel.SetVisible(false);
		HistoryFadeTime = gpGlobals.CurTime + CHAT_HISTORY_FADE_TIME;
		MessageMode = MessageModeType.None;
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