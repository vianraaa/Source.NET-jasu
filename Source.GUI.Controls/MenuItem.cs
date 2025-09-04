using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;

namespace Source.GUI.Controls;

public class MenuItem : Button {
	static MenuItem() => ChainToAnimationMap<MenuItem>();
	KeyValues? userData;
	public MenuItem(Panel parent, string name, string text) : base(parent, name, text){
		ContentAlignment = Alignment.West;
		SetParent(parent);
	}
	public KeyValues? GetUserData() => userData;
	public void SetUserData(KeyValues? kv) {
		userData = null;
		userData = kv?.MakeCopy();
	}
	public override void PaintBackground() {
		
	}

	public Menu? GetParentMenu() => GetParent() is Menu menu ? menu : null;

	public override void ApplySchemeSettings(IScheme scheme) {
		base.ApplySchemeSettings(scheme);
		SetTextInset(int.TryParse(scheme.GetResourceString("Menu.TextInset"), out int r) ? r : 0, 0);

		Menu? parent = GetParentMenu();
		if (parent != null)
			parent.ForceCalculateWidth();
	}
}
