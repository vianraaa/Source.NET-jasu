using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;
using Source.GUI.Controls;

namespace Game.UI;

public class GameMenuItem : MenuItem
{
	public GameMenuItem(Panel panel, string name) : base(panel, name) {

	}
}

public class GameMenu : Menu {
	public virtual int AddMenuItem(ReadOnlySpan<char> itemName, ReadOnlySpan<char> itemText, ReadOnlySpan<char> command, Panel? target, KeyValues? userData = null) {
		MenuItem item = EngineAPI.New<GameMenuItem>(this, new string(itemName));
		item.AddActionSignalTarget(target);
		item.SetCommand(command);
		item.SetText(itemText);
		item.SetUserData(userData);
		return base.AddMenuItem(item);
	}
}

public class BasePanel : Panel {
	GameMenu GameMenu;

	[Imported] public IFileSystem FileSystem;

	public BasePanel() : base(null, "BaseGameUIPanel") {
		CreateGameMenu();
		CreateGameLogo();
	}
	private void CreateGameMenu() {
		KeyValues datafile = new KeyValues("GameMenu");
		datafile.UsesEscapeSequences(true);
		if (datafile.LoadFromFile(FileSystem, "Resource/GameMenu.res")) 
			GameMenu = RecursiveLoadGameMenu(datafile);
		
		if (GameMenu == null)
			Error("Could not load file Resource/GameMenu.res\n");
		
		else {
			GameMenu.MakeReadyForUse();
			GameMenu.SetAlpha(0);
		}
	}

	private GameMenu RecursiveLoadGameMenu(KeyValues datafile) {
		GameMenu menu = EngineAPI.New<GameMenu>();
		for(KeyValues? dat = datafile.GetFirstSubKey(); dat != null; dat = dat.GetNextKey()) {
			ReadOnlySpan<char> label = dat.GetString("label", "<unknown>");
			ReadOnlySpan<char> cmd = dat.GetString("command", null);
			ReadOnlySpan<char> name = dat.GetString("name", label);

			menu.AddMenuItem(name, label, cmd, this, dat);
		}
		return menu;
	}

	private void CreateGameLogo() {

	}
}
