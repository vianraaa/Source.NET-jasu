using Source.Common.Engine;
using Source.Common.GUI;

namespace Source.GUI.Controls;
public class FrameSystemButton : MenuButton
{
	public FrameSystemButton(Panel parent, string name, string text) : base(parent, name, text) {
	}

	IImage? Enabled, Disabled;
	Color EnabledColor, DisabledColor;
	bool Responsive;
	string? EnabledImage;
	string? DisabledImage;

	public void SetResponsive(bool state) => Responsive = state;
}

public class FrameButton : Button
{
	public FrameButton(Panel parent, string name, string text) : base(parent, name, text) {

	}
}

public class Frame : EditablePanel
{
	TextImage? Title;
	bool Moveable;
	bool Sizeable;
	bool HasFocus;
	bool FlashWindow;
	bool DrawTitleBar;
	bool PreviouslyVisible;
	bool FadingOut;
	bool DisableFadeEffect;
	double TransitionEffectTime;
	double FocusTransitionEffectTime;
	bool DeleteSelfOnClose;
	int ClientInsetX;
	int ClientInsetY;
	bool ClientInsetXOverridden;
	int TitleTextInsetX;
	bool ClipToParent;
	bool SmallCaption;
	bool ChainKeysToParent;
	bool Primed;
	IFont? CustomTitleFont;

	Color TitleBarBgColor;
	Color TitleBarDisabledBgColor;
	Color TitleBarFgColor;
	Color TitleBarDisabledFgColor;
	Color InFocusBgColor;
	Color OutOfFocusBgColor;

	Panel TopGrip;
	Panel BottomGrip;
	Panel LeftGrip;
	Panel RightGrip;
	Panel TopLeftGrip;
	Panel TopRightGrip;
	Panel BottomLeftGrip;
	Panel BottomRightGrip;
	Panel CaptionGrip;
	FrameButton MinimizeButton;
	FrameButton MaximizeButton;
	FrameButton MinimizeToSysTrayButton;
	FrameButton CloseButton;
	FrameSystemButton MenuButton;
	Menu SysMenu;

	public void SetDeleteSelfOnClose(bool state) => DeleteSelfOnClose = state;

	public Frame(Panel? parent, string? name, bool showTaskbarIcon = true, bool popup = true) : base(parent, name, showTaskbarIcon) {
		SetVisible(false);
		if (popup)
			MakePopup(showTaskbarIcon);

		Title = null;
		Moveable = true;
		Sizeable = true;
		HasFocus = false;
		FlashWindow = false;
		DrawTitleBar = true;
		PreviouslyVisible = false;
		FadingOut = false;
		DisableFadeEffect = false;
		TransitionEffectTime = 0.0f;
		FocusTransitionEffectTime = 0.0f;
		DeleteSelfOnClose = false;
		ClientInsetX = 5;
		ClientInsetY = 5;
		ClientInsetXOverridden = false;
		TitleTextInsetX = 28;
		ClipToParent = false;
		SmallCaption = false;
		ChainKeysToParent = false;
		Primed = false;
		CustomTitleFont = null;

		SetTitle("#Frame_Untitled", parent != null ? false : true);
	}

	public void SetMenuButtonResponsive(bool state) => MenuButton?.SetResponsive(state);
	public void SetMinimizeButtonVisible(bool state) => MinimizeButton?.SetVisible(state);
	public void SetMaximizeButtonVisible(bool state) => MaximizeButton?.SetVisible(state);
	public void SetCloseButtonVisible(bool state) => CloseButton?.SetVisible(state);

	public void SetTitle(ReadOnlySpan<char> title, bool surfaceTitle) {
		if (Title == null) {
			Title = EngineAPI.New<TextImage>("");
		}

		bool localized = false;
		if (title != null && title.Length > 0 && title[0] == '#') {
			ulong unlocalizedTextSymbol = Localize.FindIndex(title[1..]);
			localized = unlocalizedTextSymbol != ulong.MaxValue;
			if (localized)
				Title.SetText(Localize.GetValueByIndex(unlocalizedTextSymbol));
		}

		if(!localized)
			Title.SetText(title);

		if(surfaceTitle)
			Surface.SetTitle(this, title);

		Repaint();
	}

	public bool IsSizeable() => Sizeable;
	public void SetSizeable(bool state) => Sizeable = state;
	public bool IsMoveable() => Moveable;
	public void SetMoveable(bool state) => Moveable = state;
	public bool GetClipToParent() => ClipToParent;
	public void SetClipToParent(bool state) => ClipToParent = state;
}
