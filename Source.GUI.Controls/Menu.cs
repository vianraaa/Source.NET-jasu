using Source.Common.GUI;

namespace Source.GUI.Controls;

public class MenuSeparator : Panel
{

}

public class Menu : Panel
{
	public const int MENU_SEPARATOR_HEIGHT = 3;
	public Alignment Alignment;

	protected ScrollBar? Scroller;

	protected List<MenuItem> MenuItems = [];
	protected List<int> SortedItems = [];
	protected List<int> Separators = [];
	protected List<int> VisibleSortedItems = [];
	protected List<MenuSeparator> SeparatorPanels = [];

	protected bool recalculateWidth = true;
	int MenuItemHeight;
	bool SizedForScrollBar;
	int FixedWidth;
	int MinimumWidth;
	int MenuWide;
	int NumVisibleLines;

	public virtual int AddMenuItem(MenuItem panel) {
		panel.SetParent(this);
		MenuItems.Add(panel);
		InvalidateLayout(false);
		recalculateWidth = true;
		panel.SetContentAlignment(Alignment);
		// the rest is todo
		return MenuItems.Count - 1;
	}

	MenuItem? GetParentMenuItem() => GetParent() is MenuItem mi ? mi : null;

	public int CountVisibleItems() {
		int i = 0;
		foreach (var child in GetChildren())
			if (child.IsVisible())
				i++;

		return i;
	}

	public override void PerformLayout() {
		MenuItem? parent = GetParentMenuItem();
		bool cascading = parent != null ? true : false;

		GetInset(out int ileft, out int iright, out int itop, out int ibottom);
		ComputeWorkspaceSize(out int workWide, out int workTall);

		int fullHeightWouldRequire = ComputeFullMenuHeightWithInsets();
		bool needScrollbar = fullHeightWouldRequire >= workTall;
		int maxVisibleItems = CountVisibleItems();

		if (NumVisibleLines > 0 &&
			maxVisibleItems > NumVisibleLines) {
			needScrollbar = true;
			maxVisibleItems = NumVisibleLines;
		}

		if (needScrollbar) {
			AddScrollBar();
			MakeItemsVisibleInScrollRange(NumVisibleLines, Math.Min(fullHeightWouldRequire, workTall));
		}
		else {
			RemoveScrollBar();
			VisibleSortedItems.Clear();
			int ip;
			int c = SortedItems.Count();
			for (ip = 0; ip < c; ++ip) {
				int itemID = SortedItems[ip];
				MenuItem child = MenuItems[itemID];
				if (child == null || !child.IsVisible())
					continue;

				VisibleSortedItems.Add(itemID);
			}

			c = SeparatorPanels.Count();
			for (ip = 0; ip < c; ++ip)
				SeparatorPanels[ip]?.SetVisible(false);
		}

		// get the appropriate menu border
		LayoutMenuBorder();

		int trueW = GetWide();
		if (needScrollbar) 
			trueW -= Scroller!.GetWide();
		
		int separatorHeight = MENU_SEPARATOR_HEIGHT;

		int menuTall = 0;
		int totalTall = itop + ibottom;
		int i;
		for (i = 0; i < VisibleSortedItems.Count(); i++)
		{
			int itemId = VisibleSortedItems[i];

			MenuItem? child = MenuItems[itemId];
			Assert(child != null);
			if (child == null)
				continue;

			if (!child.IsVisible())
				continue;

			if (totalTall >= workTall)
				break;

			//  if (INVALID_FONT != m_hItemFont) {
			//  	child->SetFont(m_hItemFont);
			//  }

			child.SetPos(0, menuTall);
			child.SetTall(MenuItemHeight); 
			menuTall += MenuItemHeight;
			totalTall += MenuItemHeight;

			// TODO: checkable

			int sepIndex = Separators.FindIndex(0, (x) => x == itemId);
			if (sepIndex != -1) {
				MenuSeparator? sep = SeparatorPanels[sepIndex];
				Assert(sep != null);
				sep.SetVisible(true);
				sep.SetBounds(0, menuTall, trueW, separatorHeight);
				menuTall += separatorHeight;
				totalTall += separatorHeight;
			}
		}

		if (FixedWidth == 0) {
			recalculateWidth = true;
			CalculateWidth();
		}
		else if (FixedWidth > 0) {
			MenuWide = FixedWidth;
			if (SizedForScrollBar) 
				MenuWide -= Scroller!.GetWide();
		}

		SizeMenuItems();

		int extraWidth = 0;
		if (SizedForScrollBar) 
			extraWidth = Scroller!.GetWide();

		int mwide = MenuWide + extraWidth;
		if (mwide > workWide) 
			mwide = workWide;

		int mtall = menuTall + itop + ibottom;
		if (mtall > workTall) 
			mtall = workTall;
		
		SetSize(mwide, mtall);

		if (cascading) 
			PositionCascadingMenu();

		if (Scroller!.IsVisible()) 
			LayoutScrollBar();

		foreach(var menuItem in MenuItems)
			menuItem.InvalidateLayout();
		Repaint();
	}

	private void PositionCascadingMenu() {
		throw new NotImplementedException();
	}

	private void LayoutScrollBar() {
		throw new NotImplementedException();
	}

	private void SizeMenuItems() {
		throw new NotImplementedException();
	}

	private void CalculateWidth() {
		throw new NotImplementedException();
	}

	private void LayoutMenuBorder() {
		throw new NotImplementedException();
	}

	private void MakeItemsVisibleInScrollRange(int numVisibleLines, int v) {
		throw new NotImplementedException();
	}

	private void RemoveScrollBar() {
		throw new NotImplementedException();
	}

	private void AddScrollBar() {
		throw new NotImplementedException();
	}

	private void ComputeWorkspaceSize(out int workWide, out int workTall) {
		GetInset(out _, out _, out int top, out int bottom);

		Surface.GetWorkspaceBounds(out int workX, out int workY, out workWide, out workTall);
		workTall -= 20;
		workTall -= top;
		workTall -= bottom;
	}

	private int ComputeFullMenuHeightWithInsets() {
		GetInset(out int left, out int right, out int top, out int bottom);

		int separatorHeight = 3;

		int totalTall = top + bottom;
		int i;
		for (i = 0; i < SortedItems.Count(); i++) {
			int itemId = SortedItems[i];

			MenuItem child = MenuItems[itemId];
			Assert(child != null);
			if (child == null)
				continue;

			if (!child.IsVisible())
				continue;

			totalTall += MenuItemHeight;

			int sepIndex = Separators.FindIndex(0, (i) => i == itemId);
			if (sepIndex != -1)
				totalTall += separatorHeight;
		}

		return totalTall;
	}
}