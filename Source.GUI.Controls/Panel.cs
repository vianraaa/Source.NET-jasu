using Source.Common.Engine;
using Source.Common.GUI;
using Source.Common.MaterialSystem;

using System.Runtime.InteropServices;

using static Source.Common.Networking.svc_ClassInfo;

namespace Source.GUI.Controls;

public struct OverrideableColorEntry
{
	public delegate Color ColorFunc(in OverrideableColorEntry self);
	public string Name;
	public ColorFunc? Func;
	public Color ColorFromScript;
	public bool Overridden;
	public readonly Color Color() => Func == null ? default : Func(in this);
}

[Flags]
public enum PanelFlags
{
	MarkedForDeletion = 0x0001,
	NeedsRepaint = 0x0002,
	PaintBorderEnabled = 0x0004,
	PaintBackgroundEnabled = 0x0008,
	PaintEnabled = 0x0010,
	PostChildPaintEnabled = 0x0020,
	AutoDeleteEnabled = 0x0040,
	NeedsLayout = 0x0080,
	NeedsSchemeUpdate = 0x0100,
	NeedsDefaultSettingsApplied = 0x0200,
	AllowChainKeybindingToParent = 0x0400,
	InPerformLayout = 0x0800,
	IsProportional = 0x1000,
	TriplePressAllowed = 0x2000,
	DragRequiresPanelExit = 0x4000,
	IsMouseDisabledForThisPanelOnly = 0x8000,
	All = 0xFFFF,
}

public class Panel : IPanel
{
	[Imported] public ISurface Surface;
	[Imported] public ISchemeManager SchemeManager;
	[Imported] public IEngineAPI EngineAPI;

	public void Init(int x, int y, int w, int h) {
		PanelName = null;
		TooltipText = null;
		SetPos(x, y);
		SetSize(w, h);
		Flags |= PanelFlags.NeedsLayout | PanelFlags.NeedsSchemeUpdate | PanelFlags.NeedsDefaultSettingsApplied;
		Flags |= PanelFlags.AutoDeleteEnabled | PanelFlags.PaintBorderEnabled | PanelFlags.PaintBackgroundEnabled | PanelFlags.PaintEnabled;
		Flags |= PanelFlags.AllowChainKeybindingToParent;
		Alpha = 255.0f;
		Visible = true;
		Enabled = true;
		Parent = null;
		Popup = false;
		TopmostPopup = false;

		MouseInput = true;
		KbInput = true;
	}

	public void MakeReadyForUse() {
		Surface.SolveTraverse(this, true);
	}
	public float GetAlpha() => Alpha;
	public void SetAlpha(float value) => Alpha = value;


	public Panel() {
		Init(0, 0, 64, 24);
	}

	public Panel(Panel? parent) {
		Init(0, 0, 64, 24);
		SetParent(parent);
	}

	public Panel(Panel? parent, ReadOnlySpan<char> panelName) {
		Init(0, 0, 64, 24);
		SetName(panelName);
		SetParent(parent);
	}
	public Panel(Panel? parent, string panelName) {
		Init(0, 0, 64, 24);
		SetName(panelName);
		SetParent(parent);
	}
	public Panel(Panel? parent, string panelName, IScheme scheme) {
		Init(0, 0, 64, 24);
		SetName(panelName);
		SetParent(parent);
		SetScheme(scheme);
	}
	public Panel(Panel? parent, ReadOnlySpan<char> panelName, IScheme scheme) {
		Init(0, 0, 64, 24);
		SetName(panelName);
		SetParent(parent);
		SetScheme(scheme);
	}

	private void SetScheme(IScheme scheme) {
		throw new NotImplementedException();
	}

	Panel? Parent;

	string? PanelName;
	string? TooltipText;
	short X, Y;
	short W, H;
	short MinW, MinH;
	short InsetLeft, InsetTop, InsetRight, InsetBottom;
	short ClipRectX, ClipRectY, ClipRectW, ClipRectH;
	short AbsX, AbsY;
	short ZPos;

	bool Visible;
	bool Enabled;
	bool Popup;
	bool MouseInput;
	bool KbInput;
	bool TopmostPopup;
	float Alpha;
	IBorder? Border;
	IScheme? Scheme;
	PanelFlags Flags;
	readonly List<IPanel> Children = [];
	readonly List<OverrideableColorEntry> OverrideableColorEntries = [];

	IPanel? SkipChild;

	Color BgColor;
	Color FgColor;

	PaintBackgroundType PaintBackgroundType;
	public PaintBackgroundType GetPaintBackgroundType() => PaintBackgroundType;
	public void SetPaintBackgroundType(PaintBackgroundType type) => PaintBackgroundType = type;

	public void DeletePanel() {
		throw new NotImplementedException();
	}

	public void GetAbsPos(out int x, out int y) {
		throw new NotImplementedException();
	}

	public Panel GetChild(int index) {
		return (Panel)Children[index];
	}

	public int GetChildCount() {
		return Children.Count;
	}

	public IEnumerable<IPanel> GetChildren() => Children;

	public ReadOnlySpan<char> GetClassName() => GetType().Name;

	public void GetClipRect(out int x0, out int y0, out int x1, out int y1) {
		x0 = ClipRectX;
		y0 = ClipRectY;
		x1 = ClipRectW;
		y1 = ClipRectH;
	}

	public IPanel GetCurrentKeyFocus() {
		throw new NotImplementedException();
	}

	public void GetInset(out int left, out int top, out int right, out int bottom) {
		left = InsetLeft;
		top = InsetTop;
		right = InsetRight;
		bottom = InsetBottom;
	}

	public void GetMinimumSize(out int wide, out int tall) {
		wide = MinW;
		tall = MinH;
	}

	public ReadOnlySpan<char> GetName() {
		if (PanelName != null)
			return PanelName;

		return "";
	}

	public IPanel? GetParent() {
		return Parent;
	}

	public void GetPos(out int x, out int y) {
		x = this.X;
		y = this.Y;
	}

	public IScheme? GetScheme() {
		if (Scheme != null)
			return Scheme;
		if (Parent != null)
			return Parent.GetScheme();

		return SchemeManager.GetDefaultScheme();
	}

	public void GetSize(out int wide, out int tall) {
		wide = W;
		tall = H;
	}

	public int GetTabPosition() {
		throw new NotImplementedException();
	}

	public void SetSkipChildDuringPainting(Panel child) => SkipChild = child;

	public int GetZPos() {
		return ZPos;
	}

	public bool HasParent(IPanel potentialParent) {
		IPanel? parent = this.Parent;

		while (parent != null) {
			if (parent == potentialParent)
				return true;
			parent = parent.GetParent();
		}

		return false;
	}

	public virtual void InternalFocusChanged(bool lost) {

	}

	public bool IsAutoDeleteSet() {
		throw new NotImplementedException();
	}

	public bool IsEnabled() {
		throw new NotImplementedException();
	}

	public bool IsFullyVisible() {
		throw new NotImplementedException();
	}

	public bool IsKeyboardInputEnabled() => KbInput;

	public bool IsMouseInputEnabled() => MouseInput;

	public bool IsPopup() {
		return Popup;
	}

	public bool IsProportional() {
		throw new NotImplementedException();
	}

	public bool IsTopmostPopup() => TopmostPopup;

	public bool IsVisible() {
		return Visible;
	}

	public IPanel IsWithinTraverse(int x, int y, bool traversePopups) {
		throw new NotImplementedException();
	}

	public void MoveToBack() {
		throw new NotImplementedException();
	}

	public void MoveToFront() {
		// todo... ugh
	}

	public virtual void OnChildAdded(IPanel child) {

	}

	public virtual void OnSizeChanged(int newWide, int newTall) {
		InvalidateLayout();
	}

	public void InvalidateLayout(bool layoutNow = false, bool reloadScheme = false) {
		Flags |= PanelFlags.NeedsLayout;

		if (reloadScheme) {
			// make all our children reload the scheme
			Flags |= PanelFlags.NeedsSchemeUpdate;

			for (int i = 0; i < GetChildCount(); i++) {
				IPanel? panel = GetChild(i);
				if (panel != null) {
					panel.InvalidateLayout(layoutNow, true);
				}
			}

			PerformApplySchemeSettings();
		}

		if (layoutNow) {
			InternalPerformLayout();
			Repaint();
		}
	}

	private void InternalPerformLayout() {
		if (Flags.HasFlag(PanelFlags.NeedsSchemeUpdate))
			return;

		Flags |= PanelFlags.InPerformLayout;
		Flags &= ~PanelFlags.NeedsLayout;
		PerformLayout();
		Flags &= ~PanelFlags.InPerformLayout;
	}

	public virtual void PerformLayout() {

	}

	public void AddActionSignalTarget(Panel? messageTarget) {
		// What does this do...?
	}

	public void PaintTraverse(bool repaint, bool allowForce = true) {
		if (!IsVisible())
			return;

		float oldAlphaMultiplier = Surface.DrawGetAlphaMultiplier();
		float newAlphaMultiplier = oldAlphaMultiplier * Alpha * 1.0f / 255.0f;

		if (!repaint && allowForce && Flags.HasFlag(PanelFlags.NeedsRepaint)) {
			repaint = true;
			Flags &= ~PanelFlags.NeedsRepaint;
		}

		bool bPushedViewport = false;

		Span<int> clipRect = stackalloc int[4];
		GetClipRect(out clipRect[0], out clipRect[1], out clipRect[2], out clipRect[3]);
		if ((clipRect[2] <= clipRect[0]) || (clipRect[3] <= clipRect[1]))
			repaint = false;

		Surface.DrawSetAlphaMultiplier(newAlphaMultiplier);

		bool bBorderPaintFirst = Border != null ? Border.PaintFirst() : false;

		if (bBorderPaintFirst && repaint && Flags.HasFlag(PanelFlags.PaintBorderEnabled) && (Border != null)) {
			Surface.PushMakeCurrent(this, false);
			PaintBorder();
			Surface.PopMakeCurrent(this);
		}

		if (repaint) {
			if (Flags.HasFlag(PanelFlags.PaintBackgroundEnabled)) {
				Surface.PushMakeCurrent(this, false);
				PaintBackground();
				Surface.PopMakeCurrent(this);
			}

			if (Flags.HasFlag(PanelFlags.PaintEnabled)) {
				Surface.PushMakeCurrent(this, true);
				Paint();
				Surface.PopMakeCurrent(this);
			}
		}

		for (int i = 0, childCount = Children.Count; i < childCount; i++) {
			IPanel child = Children[i];
			bool bVisible = child.IsVisible();

			if (Surface.ShouldPaintChildPanel(child)) {
				if (bVisible) {
					child.PaintTraverse(repaint, allowForce);
				}
			}
			else {
				Surface.Invalidate(child);

				if (bVisible)
					child.PaintTraverse(false, false);
			}
		}

		if (repaint) {
			if (!bBorderPaintFirst && Flags.HasFlag(PanelFlags.PaintBorderEnabled) && (Border != null)) {
				Surface.PushMakeCurrent(this, false);
				PaintBorder();
				Surface.PopMakeCurrent(this);
			}

			if (Flags.HasFlag(PanelFlags.PostChildPaintEnabled)) {
				Surface.PushMakeCurrent(this, false);
				PostChildPaint();
				Surface.PopMakeCurrent(this);
			}
		}

		Surface.DrawSetAlphaMultiplier(oldAlphaMultiplier);

		Surface.SwapBuffers(this);

		if (bPushedViewport) {
			// surface()->PopFullscreenViewport();
			// ^^ todo: later
		}
	}

	public virtual void PostChildPaint() {

	}

	public virtual void Paint() {

	}

	public void GetBgColor(in Color c) => BgColor = c;
	public void GetFgColor(in Color c) => FgColor = c;
	public Color GetBgColor() => BgColor;
	public Color GetFgColor() => FgColor;

	public virtual void PaintBackground() {
		GetSize(out int wide, out int tall);
		if (SkipChild != null && SkipChild.IsVisible()) {
			if (GetPaintBackgroundType() == PaintBackgroundType.Box) {
				GetCornerTextureSize(out int cornerWide, out int cornerTall);

				Color col = GetBgColor();
				DrawHollowBox(0, 0, wide, tall, col, 1.0f);

				wide -= 2 * cornerWide;
				tall -= 2 * cornerTall;

				FillRectSkippingPanel(GetBgColor(), cornerWide, cornerTall, wide, tall, SkipChild);
			}
			else {
				FillRectSkippingPanel(GetBgColor(), 0, 0, wide, tall, SkipChild);
			}
		}
		else {
			Color col = GetBgColor();

			switch (PaintBackgroundType) {
				default:
				case PaintBackgroundType.Filled: {
						Surface.DrawSetColor(col);
						Surface.DrawFilledRect(0, 0, wide, tall);
					}
					break;
				case PaintBackgroundType.Textured: {
						DrawTexturedBox(0, 0, wide, tall, col, 1.0f);
					}
					break;
				case PaintBackgroundType.Box: {
						DrawBox(0, 0, wide, tall, col, 1.0f);
					}
					break;
				case PaintBackgroundType.BoxFade: {
						DrawBoxFade(0, 0, wide, tall, col, 1.0f, 255, 0, true);
					}
					break;
			}
		}
	}

	private void DrawBox(int v1, int v2, int wide, int tall, Color col, float v3) {
		throw new NotImplementedException();
	}

	private void DrawBoxFade(int v1, int v2, int wide, int tall, Color col, float v3, int v4, int v5, bool v6) {
		throw new NotImplementedException();
	}

	private void DrawTexturedBox(int v1, int v2, int wide, int tall, Color col, float v3) {
		throw new NotImplementedException();
	}

	private void FillRectSkippingPanel(in Color color, int cornerWide, int cornerTall, int wide, int tall, IPanel skipChild) {
		throw new NotImplementedException();
	}

	private void DrawHollowBox(int v1, int v2, int wide, int tall, Color col, float v3) {
		throw new NotImplementedException();
	}

	private void GetCornerTextureSize(out int cornerWide, out int cornerTall) {
		throw new NotImplementedException();
	}

	private void PaintBorder() {
		Border!.Paint(this);
	}

	public void PerformApplySchemeSettings() {
		if (Flags.HasFlag(PanelFlags.NeedsDefaultSettingsApplied)) {
			// InternalInitDefaultValues(GetAnimMap());
		}

		if (Flags.HasFlag(PanelFlags.NeedsSchemeUpdate)) {
			IScheme? scheme = GetScheme();
			Assert(scheme != null);
			if (scheme != null) {
				ApplySchemeSettings(scheme);
				ApplyOverridableColors();
			}
		}
	}

	public void SetBgColor(in Color color) => BgColor = color;
	public void SetFgColor(in Color color) => FgColor = color;

	// This in theory will replicate the pointer logic?
	private void ApplyOverridableColors() {
		Span<OverrideableColorEntry> entries = CollectionsMarshal.AsSpan(OverrideableColorEntries);
		for (int i = 0, c = entries.Length; i < c; i++) {
			ref OverrideableColorEntry entry = ref entries[i];
			if (entry.Overridden)
				entry.Func = (in OverrideableColorEntry e) => e.ColorFromScript;
		}
	}

	public Color GetSchemeColor(ReadOnlySpan<char> keyName, IScheme scheme) {
		return scheme.GetColor(keyName, new(255, 255, 255, 255));
	}
	public Color GetSchemeColor(ReadOnlySpan<char> keyName, Color defaultColor, IScheme scheme) {
		return scheme.GetColor(keyName, defaultColor);
	}

	private void ApplySchemeSettings(IScheme scheme) {
		SetFgColor(GetSchemeColor("Panel.FgColor", scheme));
		SetBgColor(GetSchemeColor("Panel.BgColor", scheme));

		Flags &= ~PanelFlags.NeedsSchemeUpdate;
	}

	public void Repaint() {
		// todo
	}

	public void RequestFocus(int direction = 0) {

	}

	public bool RequestFocusNext(IPanel existingPanel) {
		throw new NotImplementedException();
	}

	public bool RequestFocusPrev(IPanel existingPanel) {
		throw new NotImplementedException();
	}

	public void SetBounds(int x, int y, int wide, int tall) {
		SetPos(x, y);
		SetSize(wide, tall);
	}

	public void SetCursor(ICursor cursor) {
		// todo
	}

	public void SetEnabled(bool state) {
		throw new NotImplementedException();
	}

	public void SetInset(int left, int top, int right, int bottom) {
		throw new NotImplementedException();
	}

	public void SetKeyboardInputEnabled(bool state) {
		KbInput = state;
	}

	public void SetMinimumSize(int wide, int tall) {
		MinW = (short)wide;
		MinH = (short)tall;

		int currentWidth = W;
		if (currentWidth < wide)
			currentWidth = wide;

		int currentHeight = H;
		if (currentHeight < tall)
			currentHeight = tall;

		if (currentWidth != W || currentHeight != H)
			SetSize(currentWidth, currentHeight);
	}

	public void SetMouseInputEnabled(bool state) {
		MouseInput = state;
	}

	public void SetName(ReadOnlySpan<char> panelName) {
		if (this.PanelName != null && panelName != null && !panelName.Equals(this.PanelName, StringComparison.Ordinal))
			return;

		if (this.PanelName != null)
			panelName = null;

		if (panelName != null)
			this.PanelName = new(panelName);
	}

	public void SetParent(IPanel? newParent) {
		Parent?.Children.Remove(this);

		Parent = (Panel)newParent!;
	}

	public void SetPopup(bool state) {
		throw new NotImplementedException();
	}

	public void SetPos(int x, int y) {
		this.X = (short)x;
		this.Y = (short)y;
	}

	public void SetSize(int wide, int tall) {
		if (wide < MinW)
			wide = MinW;
		if (tall < MinH)
			tall = MinH;

		if (W == wide && H == tall)
			return;

		W = (short)wide;
		H = (short)tall;

		OnSizeChanged(wide, tall);
	}

	public void SetTopmostPopup(bool state) {
		throw new NotImplementedException();
	}

	public void SetVisible(bool state) {
		if (Visible == state)
			return;

		// need to tell the surface later... UGH... HOW DO WE GET THE SURFACE RELIABLY HERE??
		Visible = state;
	}

	public void SetZPos(int z) {
		ZPos = (short)z;
		if (Parent != null) {
			int childCount = Parent.GetChildCount();
			int i;
			for (i = 0; i < childCount; i++) {
				if (Parent.GetChild(i) == this)
					break;
			}

			if (i == childCount)
				return;

			while (true) {
				Panel? prevChild = null, nextChild = null;
				if (i > 0)
					prevChild = Parent.GetChild(i - 1);
				if (i < (childCount - 1))
					nextChild = Parent.GetChild(i + 1);

				if (i > 0 && prevChild != null && prevChild.ZPos > ZPos) {
					// Swap with lower
					Parent.Children[i] = prevChild;
					Parent.Children[i - 1] = this;
				}
				else if (i < (childCount - 1) && nextChild != null && nextChild.ZPos < ZPos) {
					Parent.Children[i] = nextChild;
					Parent.Children[i + 1] = this;
				}
				else
					break;
			}
		}
	}

	public void Solve() {
		throw new NotImplementedException();
	}

	public void Think() {
		if (IsVisible()) {
			// TODO: Tooltips layout
			if ((Flags & PanelFlags.NeedsLayout) != 0)
				InternalPerformLayout();
		}

		OnThink();
	}

	public virtual void OnThink() {

	}

	public void SetPanelBorderEnabled(bool enabled) => Flags = enabled ? Flags |= PanelFlags.PaintEnabled : Flags &= ~PanelFlags.PaintEnabled;
	public void SetPaintBackgroundEnabled(bool enabled) => Flags = enabled ? Flags |= PanelFlags.PaintBackgroundEnabled : Flags &= ~PanelFlags.PaintBackgroundEnabled;
	public void SetPaintBorderEnabled(bool enabled) => Flags = enabled ? Flags |= PanelFlags.PaintBorderEnabled : Flags &= ~PanelFlags.PaintBorderEnabled;
	public void SetPaintEnabled(bool enabled) => Flags = enabled ? Flags |= PanelFlags.PaintEnabled : Flags &= ~PanelFlags.PaintEnabled;
	public void SetPostChildPaintEnabled(bool enabled) => Flags = enabled ? Flags |= PanelFlags.PostChildPaintEnabled : Flags &= ~PanelFlags.PostChildPaintEnabled;

	IPanel IPanel.GetChild(int index) => GetChild(index);

	public int GetX() => X;
	public int GetY() => Y;
	public int GetWide() => W;
	public int GetTall() => H;
	public void SetWide(int wide) => SetSize(wide, GetTall());
	public void SetTall(int tall) => SetSize(GetWide(), tall);

	public void TraverseLevel(int v) {

	}
}