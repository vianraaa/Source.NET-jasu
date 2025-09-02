using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;

namespace Source.GUI.Controls;

public class MenuItem : Button {
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
}
