namespace Source.GUI.Controls;

public class MenuButton : Button
{
	static MenuButton() => ChainToAnimationMap<MenuButton>();

	public MenuButton(Panel parent, string name, string text) : base(parent, name, text) {
	}
}
