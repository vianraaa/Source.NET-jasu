using Source.Common.GUI;

namespace Source.GUI.Controls;

public class Menu : Panel
{
	public Alignment Alignment;
	protected List<MenuItem> MenuItems = [];
	protected bool recalculateWidth = true;
	public virtual int AddMenuItem(MenuItem panel) {
		panel.SetParent(this);
		MenuItems.Add(panel);
		InvalidateLayout(false);
		recalculateWidth = true;
		panel.SetContentAlignment(Alignment);
		// the rest is todo
		return MenuItems.Count - 1;
	} 
}