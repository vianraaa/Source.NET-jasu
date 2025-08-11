using Source.Common.GUI;

namespace Source.GUI.Controls;


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
	public void Init(int x, int y, int w, int h) {
		panelName = null;
		tooltipText = null;
		SetPos(x, y);
		SetSize(w, h);
		flags |= PanelFlags.NeedsLayout | PanelFlags.NeedsSchemeUpdate | PanelFlags.NeedsDefaultSettingsApplied;
		flags |= PanelFlags.AutoDeleteEnabled | PanelFlags.PaintBorderEnabled | PanelFlags.PaintBackgroundEnabled | PanelFlags.PaintEnabled;
		flags |= PanelFlags.AllowChainKeybindingToParent;
		alpha = 255.0f;
	}

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
	public Panel(Panel? parent, ReadOnlySpan<char> panelName, IScheme scheme) {
		Init(0, 0, 64, 24);
		SetName(panelName);
		SetParent(parent);
		SetScheme(scheme);
	}

	private void SetScheme(IScheme scheme) {
		throw new NotImplementedException();
	}

	Panel? parent;

	string? panelName;
	string? tooltipText;
	short x, y;
	short w, h;
	short minW, minH;
	short insetX, insetY, insetW, insetH;
	short clipRectX, clipRectY, clipRectW, clipRectH;
	short absX, absY;
	short zpos;

	bool visible;
	bool enabled;
	bool popup;
	bool mouseInput;
	bool kbInput;
	bool isTopmostPopup;
	float alpha;
	PanelFlags flags;
	List<Panel> children = [];

	public void DeletePanel() {
		throw new NotImplementedException();
	}

	public void GetAbsPos(out int x, out int y) {
		throw new NotImplementedException();
	}

	public Panel GetChild(int index) {
		return children[index];
	}

	public int GetChildCount() {
		return children.Count;
	}

	public IEnumerable<IPanel> GetChildren() => children;

	public ReadOnlySpan<char> GetClassName() => GetType().Name;

	public void GetClipRect(out int x0, out int y0, out int x1, out int y1) {
		throw new NotImplementedException();
	}

	public IPanel GetCurrentKeyFocus() {
		throw new NotImplementedException();
	}

	public void GetInset(out int left, out int top, out int right, out int bottom) {
		throw new NotImplementedException();
	}

	public void GetMinimumSize(out int wide, out int tall) {
		wide = minW;
		tall = minH;
	}

	public ReadOnlySpan<char> GetName() {
		if (panelName != null)
			return panelName;

		return "";
	}

	public IPanel? GetParent() {
		return parent;
	}

	public void GetPos(out int x, out int y) {
		x = this.x;
		y = this.y;
	}

	public IScheme? GetScheme() {
		throw new NotImplementedException();
	}

	public void GetSize(out int wide, out int tall) {
		wide = w;
		tall = h;
	}

	public int GetTabPosition() {
		throw new NotImplementedException();
	}

	public int GetZPos() {
		return zpos;
	}

	public bool HasParent(IPanel potentialParent) {
		IPanel? parent = this.parent;

		while(parent != null) {
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

	public bool IsKeyboardInputEnabled() => kbInput;

	public bool IsMouseInputEnabled() => mouseInput;

	public bool IsPopup() {
		return popup;
	}

	public bool IsProportional() {
		throw new NotImplementedException();
	}

	public bool IsTopmostPopup() => isTopmostPopup;

	public bool IsVisible() {
		return visible;
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

	}

	public void PaintTraverse(bool forceRepaint, bool allowForce = true) {
		throw new NotImplementedException();
	}

	public void PerformApplySchemeSettings() {
		throw new NotImplementedException();
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
		kbInput = state;
	}

	public void SetMinimumSize(int wide, int tall) {
		minW = (short)wide;
		minH = (short)tall;

		int currentWidth = w;
		if (currentWidth < wide)
			currentWidth = wide;

		int currentHeight = h;
		if (currentHeight < tall)
			currentHeight = tall;

		if (currentWidth != w || currentHeight != h)
			SetSize(currentWidth, currentHeight);
	}

	public void SetMouseInputEnabled(bool state) {
		mouseInput = state;
	}

	public void SetName(ReadOnlySpan<char> panelName) {
		if (this.panelName != null && panelName != null && !panelName.Equals(this.panelName, StringComparison.Ordinal))
			return;

		if (this.panelName != null)
			panelName = null;

		if (panelName != null)
			this.panelName = new(panelName);
	}

	public void SetParent(IPanel? newParent) {
		parent?.children.Remove(this);

		parent = (Panel)newParent!;
	}

	public void SetPopup(bool state) {
		throw new NotImplementedException();
	}

	public void SetPos(int x, int y) {
		this.x = (short)x;
		this.y = (short)y;
	}

	public void SetSize(int wide, int tall) {
		if (wide < minW)
			wide = minW;
		if (tall < minH)
			tall = minH;

		if (w == wide && h == tall)
			return;

		w = (short)wide;
		h = (short)tall;

		OnSizeChanged(wide, tall);
	}

	public void SetTopmostPopup(bool state) {
		throw new NotImplementedException();
	}

	public void SetVisible(bool state) {
		if (visible == state)
			return;
		
		// need to tell the surface later... UGH... HOW DO WE GET THE SURFACE RELIABLY HERE??
		visible = state;
	}

	public void SetZPos(int z) {
		zpos = (short)z;
		if(parent != null) {
			int childCount = parent.GetChildCount();
			int i;
			for(i = 0; i < childCount; i++) {
				if (parent.GetChild(i) == this)
					break;
			}

			if (i == childCount)
				return;

			while (true) {
				Panel? prevChild = null, nextChild = null;
				if (i > 0)
					prevChild = parent.GetChild(i - 1);
				if (i < (childCount - 1))
					nextChild = parent.GetChild(i + 1);

				if(i > 0 && prevChild != null && prevChild.zpos > zpos) {
					// Swap with lower
					parent.children[i] = prevChild;
					parent.children[i - 1] = this;
				}
				else if(i < (childCount - 1) && nextChild != null && nextChild.zpos < zpos) {
					parent.children[i] = nextChild;
					parent.children[i + 1] = this;
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
		throw new NotImplementedException();
	}

	public void SetPanelBorderEnabled(bool enabled) => flags = enabled ? flags |= PanelFlags.PaintEnabled : flags &= ~PanelFlags.PaintEnabled;
	public void SetPaintBackgroundEnabled(bool enabled) => flags = enabled ? flags |= PanelFlags.PaintBackgroundEnabled : flags &= ~PanelFlags.PaintBackgroundEnabled;
	public void SetPaintBorderEnabled(bool enabled) => flags = enabled ? flags |= PanelFlags.PaintBorderEnabled : flags &= ~PanelFlags.PaintBorderEnabled;
	public void SetPaintEnabled(bool enabled) => flags = enabled ? flags |= PanelFlags.PaintEnabled : flags &= ~PanelFlags.PaintEnabled;
	public void SetPostChildPaintEnabled(bool enabled) => flags = enabled ? flags |= PanelFlags.PostChildPaintEnabled : flags &= ~PanelFlags.PostChildPaintEnabled;

	IPanel IPanel.GetChild(int index) => GetChild(index);
}