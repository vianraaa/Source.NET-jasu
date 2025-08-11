using Source.Common.GUI;

namespace Source.GUI.Controls;
public class Panel : IPanel
{
	IPanel? parent;
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
	bool isTOpmostPopup;
	List<IPanel> children = [];

	public void DeletePanel() {
		throw new NotImplementedException();
	}

	public void GetAbsPos(out int x, out int y) {
		throw new NotImplementedException();
	}

	public IPanel GetChild(int index) {
		throw new NotImplementedException();
	}

	public int GetChildCount() {
		throw new NotImplementedException();
	}

	public IEnumerable<IPanel> GetChildren() {
		throw new NotImplementedException();
	}

	public ReadOnlySpan<char> GetClassName() {
		throw new NotImplementedException();
	}

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
		throw new NotImplementedException();
	}

	public IPanel? GetParent() {
		throw new NotImplementedException();
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
		throw new NotImplementedException();
	}

	public bool HasParent(IPanel potentialParent) {
		throw new NotImplementedException();
	}

	public void InternalFocusChanged(bool lost) {
		throw new NotImplementedException();
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

	public bool IsKeyBoardInputEnabled() {
		throw new NotImplementedException();
	}

	public bool IsMouseInputEnabled() {
		throw new NotImplementedException();
	}

	public bool IsPopup() {
		throw new NotImplementedException();
	}

	public bool IsProportional() {
		throw new NotImplementedException();
	}

	public bool IsTopmostPopup() {
		throw new NotImplementedException();
	}

	public bool IsVisible() {
		throw new NotImplementedException();
	}

	public IPanel IsWithinTraverse(int x, int y, bool traversePopups) {
		throw new NotImplementedException();
	}

	public void MoveToBack() {
		throw new NotImplementedException();
	}

	public void MoveToFront() {
		throw new NotImplementedException();
	}

	public void OnChildAdded(IPanel child) {
		throw new NotImplementedException();
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
		throw new NotImplementedException();
	}

	public void RequestFocus(int direction = 0) {
		throw new NotImplementedException();
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

	public void SetEnabled(bool state) {
		throw new NotImplementedException();
	}

	public void SetInset(int left, int top, int right, int bottom) {
		throw new NotImplementedException();
	}

	public void SetKeyBoardInputEnabled(bool state) {
		throw new NotImplementedException();
	}

	public void SetMinimumSize(int wide, int tall) {
		minW = (short)wide;
		minH = (short)tall;

		int currentWidth = w;
		if (currentWidth < wide)
			currentWidth = wide;

		int currentHeight = h;
		if(currentHeight < tall)
			currentHeight = tall;

		if (currentWidth != w || currentHeight != h)
			SetSize(currentWidth, currentHeight);
	}

	public void SetMouseInputEnabled(bool state) {
		throw new NotImplementedException();
	}

	public void SetParent(IPanel? newParent) {
		throw new NotImplementedException();
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
		throw new NotImplementedException();
	}

	public void SetZPos(int z) {
		throw new NotImplementedException();
	}

	public void Solve() {
		throw new NotImplementedException();
	}

	public void Think() {
		throw new NotImplementedException();
	}
}