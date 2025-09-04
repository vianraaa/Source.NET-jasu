using Source.Common.Engine;
using Source.Common.GUI;

using static System.Net.Mime.MediaTypeNames;

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

	internal void GetImageSize(out int w, out int h) {
		w = h = 0;

		int tw = 0, th = 0;
		Enabled?.GetSize(out w, out h);
		Disabled?.GetSize(out tw, out th);
		if (tw > w) 
			w = tw;
		
		if (th > h) 
			h = th;
	}
}

public class FrameButton : Button
{
	public IBorder? BrightBorder, DepressedBorder, DisabledBorder;
	public Color EnabledFgColor, EnabledBgColor, DisabledFgColor, DisabledBgColor;
	public bool DisabledLook;
	public FrameButton(Panel parent, string name, string text) : base(parent, name, text) {

	}

	public virtual void SetDisabledLook(bool state) {
		DisabledLook = state;
		if (!DisabledLook) {
			SetDefaultColor(EnabledFgColor, EnabledBgColor);
			SetArmedColor(EnabledFgColor, EnabledBgColor);
			SetDepressedColor(EnabledFgColor, EnabledBgColor);
		}
		else {
			SetDefaultColor(DisabledFgColor, DisabledBgColor);
			SetArmedColor(DisabledFgColor, DisabledBgColor);
			SetDepressedColor(DisabledFgColor, DisabledBgColor);
		}
	}
	public override void PerformLayout() {
		base.PerformLayout();
		Repaint();
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

	Panel? TopGrip;
	Panel? BottomGrip;
	Panel? LeftGrip;
	Panel? RightGrip;
	Panel? TopLeftGrip;
	Panel? TopRightGrip;
	Panel? BottomLeftGrip;
	Panel? BottomRightGrip;
	Panel? CaptionGrip;
	FrameButton? MinimizeButton;
	FrameButton? MaximizeButton;
	FrameButton? MinimizeToSysTrayButton;
	FrameButton? CloseButton;
	FrameSystemButton? MenuButton;
	Menu? SysMenu;

	public void SetDeleteSelfOnClose(bool state) => DeleteSelfOnClose = state;

	IPanel? PreviousModal;

	public void Activate() {
		MoveToFront();
		if (IsKeyboardInputEnabled())
			RequestFocus();

		SetVisible(true);
		SetEnabled(true);
		if (FadingOut) {
			FadingOut = false;
			PreviouslyVisible = false;
		}
		Surface.SetMinimized(this, false);
	}

	public virtual void DoModal() {
		MoveToCenterOfScreen();
		InvalidateLayout();
		Activate();
		PreviousModal = Input.GetAppModalSurface();
		Input.SetAppModalSurface(this);
	}

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

	public override void ApplySchemeSettings(IScheme scheme) {
		base.ApplySchemeSettings(scheme);

		SetOverridableColor(out TitleBarFgColor, GetSchemeColor("FrameTitleBar.TextColor", scheme));
		SetOverridableColor(out TitleBarBgColor, GetSchemeColor("FrameTitleBar.BgColor", scheme));
		SetOverridableColor(out TitleBarDisabledFgColor, GetSchemeColor("FrameTitleBar.DisabledTextColor", scheme));
		SetOverridableColor(out TitleBarDisabledBgColor, GetSchemeColor("FrameTitleBar.DisabledBgColor", scheme));

		ReadOnlySpan<char> font = null;
		if (SmallCaption)
			font = scheme.GetResourceString("FrameTitleBar.SmallFont");
		else
			font = scheme.GetResourceString("FrameTitleBar.Font");


		IFont? titlefont;
		if (CustomTitleFont != null)
			titlefont = CustomTitleFont;
		else
			titlefont = scheme.GetFont((font != null && font.Length > 0) ? font : "Default", IsProportional());

		Title?.SetFont(titlefont);
		Title?.ResizeImageToContent();

		IFont? marfont = null;
		if (SmallCaption)
			marfont = scheme.GetFont("MarlettSmall", IsProportional());
		else
			marfont = scheme.GetFont("Marlett", IsProportional());

		MinimizeButton?.SetFont(marfont);
		MaximizeButton?.SetFont(marfont);
		MinimizeToSysTrayButton?.SetFont(marfont);
		CloseButton?.SetFont(marfont);

		TransitionEffectTime = float.TryParse(scheme.GetResourceString("Frame.TransitionEffectTime"), out float r) ? r : 0;
		FocusTransitionEffectTime = float.TryParse(scheme.GetResourceString("Frame.FocusTransitionEffectTime"), out r) ? r : 0;

		SetOverridableColor(out InFocusBgColor, scheme.GetColor("Frame.BgColor", GetBgColor()));
		SetOverridableColor(out OutOfFocusBgColor, scheme.GetColor("Frame.OutOfFocusBgColor", InFocusBgColor));

		ReadOnlySpan<char> resourceString = scheme.GetResourceString("Frame.ClientInsetX");
		if (resourceString != null)
			ClientInsetX = int.TryParse(resourceString, out int i1) ? i1 : 0;

		resourceString = scheme.GetResourceString("Frame.ClientInsetY");
		if (resourceString != null)
			ClientInsetY = int.TryParse(resourceString, out int i1) ? i1 : 0;

		resourceString = scheme.GetResourceString("Frame.TitleTextInsetX");
		if (resourceString != null)
			TitleTextInsetX = int.TryParse(resourceString, out int i1) ? i1 : 0;

		SetBgColor(InFocusBgColor);
		SetBorder(scheme.GetBorder("FrameBorder"));

		OnFrameFocusChanged(HasFocus);
	}

	const int DEFAULT_SNAP_RANGE = 10;
	const int CAPTION_TITLE_BORDER = 7;
	const int CAPTION_TITLE_BORDER_SMALL = 0;

	public void GetClientArea(out int x, out int y, out int wide, out int tall) {
		x = ClientInsetX;
		y = 0;

		GetSize(out wide, out tall);

		if (DrawTitleBar) {
			int captionTall = Surface.GetFontTall(Title?.GetFont());

			int border = SmallCaption ? CAPTION_TITLE_BORDER_SMALL : CAPTION_TITLE_BORDER;
			int yinset = SmallCaption ? 0 : ClientInsetY;

			yinset += TitleTextInsetYOverride;

			y = yinset + captionTall + border + 1;
			tall = (tall - yinset) - y;
		}

		if (SmallCaption) 
			tall -= 5;

		wide = (wide - ClientInsetX) - x;
	}


	public override void OnClose() {
		base.OnClose();

		if (Input.GetAppModalSurface() == this) {
			Input.ReleaseAppModalSurface();
			if (PreviousModal != null) {
				Input.SetAppModalSurface(PreviousModal);
				PreviousModal = null;
			}
		}

		base.OnClose();

		if (TransitionEffectTime != 0 && !DisableFadeEffect) {
			GetAnimationController().RunAnimationCommand(this, "alpha", 0.0f, 0.0f, TransitionEffectTime, Interpolators.Linear);
			FadingOut = true;
			Surface.MovePopupToBack(this);
		}
		else 
			FinishClose();
		
		FinishClose();
	}

	public override void OnCommand(ReadOnlySpan<char> command) {
		switch (command) {
			case "Close":
				Close();
				return;
			case "CloseModal":
				CloseModal();
				return;
			default:
				base.OnCommand(command);
				break;
		}
	}

	private void Close() {
		OnClose();
	}

	private void CloseModal() {
		Input.ReleaseAppModalSurface();
		if(PreviousModal != null) {
			Input.SetAppModalSurface(PreviousModal);
			PreviousModal = null;
		}
		PostMessage(this, new("Close"));
	}

	private void FinishClose() {
		SetVisible(false);
		PreviouslyVisible = false;
		FadingOut = false;
		
		OnFinishedClose();

		if (DeleteSelfOnClose)
			MarkForDeletion();
	}
	public override void OnThink() {
		base.OnThink();

		Msg($"{GetAlpha()}");

		if(IsVisible() && TransitionEffectTime > 0 && !DisableFadeEffect) {
			if (FadingOut) {
				if (GetAlpha() < 1)
					FinishClose();
			}
			else if (!PreviouslyVisible) {
				PreviouslyVisible = true;
				SetAlpha(0);
				GetAnimationController().RunAnimationCommand(this, "alpha", 255.0f, 0.0f, TransitionEffectTime, Interpolators.Linear);
			}
		}

		bool hasFocus = false;

		if (Input != null) {
			IPanel? focus = Input.GetFocus();
			if (focus != null && focus.HasParent(this)) {
				if (Input.GetAppModalSurface() == null || Input.GetAppModalSurface() == this) {
					hasFocus = true;
				}
			}
		}
		if (hasFocus != HasFocus) {
			if (!Primed) {
				Primed = true;
				return;
			}
			Primed = false;
			HasFocus = hasFocus;
			OnFrameFocusChanged(HasFocus);
		}
		else 
			Primed = false;
	}

	private void OnFinishedClose() {
	}

	private void OnFrameFocusChanged(bool hasFocus) {
		MinimizeButton?.SetDisabledLook(!hasFocus);
		MaximizeButton?.SetDisabledLook(!hasFocus);
		CloseButton?.SetDisabledLook(!hasFocus);
		MinimizeToSysTrayButton?.SetDisabledLook(!hasFocus);
		MenuButton?.SetEnabled(hasFocus);
		MinimizeButton?.InvalidateLayout();
		MaximizeButton?.InvalidateLayout();
		MinimizeToSysTrayButton?.InvalidateLayout();
		CloseButton?.InvalidateLayout();
		MenuButton?.InvalidateLayout();

		if (HasFocus)
			Title?.SetColor(TitleBarFgColor);
		else
			Title?.SetColor(TitleBarDisabledFgColor);

		if (HasFocus) {
			if (FocusTransitionEffectTime != 0 && (!DisableFadeEffect)) { }
			// TODO: Animation controllers
			else
				SetBgColor(InFocusBgColor);
		}
		else {
			if (FocusTransitionEffectTime != 0 && (!DisableFadeEffect)) { }
			// TODO: Animation controllers
			else
				SetBgColor(OutOfFocusBgColor);
		}

		if (HasFocus && FlashWindow)
			FlashWindowStop();
	}

	private void FlashWindowStop() {
		Surface.FlashWindow(this, false);
		FlashWindow = false;
	}

	private void SetOverridableColor(out Color outColor, Color color) {
		outColor = color; // TODO: How the hell are we going to implement this right...
	}

	public void MoveToCenterOfScreen() {
		Surface.GetWorkspaceBounds(out _, out _, out int w, out int t);
		SetPos((w - GetWide()) / 2, (t - GetTall()) / 2);
	}

	int TitleTextInsetXOverride; // TODO: Animation vars...
	int TitleTextInsetYOverride; // TODO: Animation vars...

	public override void PaintBackground() {
		Color titleColor = TitleBarDisabledBgColor;
		if (HasFocus) {
			titleColor = TitleBarBgColor;
		}

		base.PaintBackground();

		if (DrawTitleBar) {
			int wide = GetWide();
			int tall = Surface.GetFontTall(Title?.GetFont());
			if (tall == 0)
				tall = 24; // temporary

			Surface.DrawSetColor(titleColor);
			int inset = SmallCaption ? 3 : 5;
			int captionHeight = SmallCaption ? 14 : 28;

			Surface.DrawFilledRect(inset, inset, wide - inset, captionHeight);

			{
				int nTitleX = TitleTextInsetXOverride != 0 ? TitleTextInsetXOverride : TitleTextInsetX;
				int nTitleWidth = wide - 72;

				if (MenuButton != null && MenuButton.IsVisible()) {
					MenuButton.GetImageSize(out int mw, out int mh);
					nTitleX += mw;
					nTitleWidth -= mw;
				}

				int nTitleY;
				if (TitleTextInsetYOverride != 0) 
					nTitleY = TitleTextInsetYOverride;
				else 
					nTitleY = SmallCaption ? 2 : 9;

				Title?.SetPos(nTitleX, nTitleY);
				Title?.SetSize(nTitleWidth, tall);
				Title?.Paint();
			}
		}
	}

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

		if (!localized)
			Title.SetText(title);

		if (surfaceTitle)
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
