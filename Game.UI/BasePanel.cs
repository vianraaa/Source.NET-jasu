using Source.Common;
using Source.Common.Client;
using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;
using Source.Common.GameUI;
using Source.Common.GUI;
using Source.Common.Launcher;
using Source.GUI.Controls;

using System.Numerics;

namespace Game.UI;

public class GameMenuItem : MenuItem
{
	public GameMenuItem(Panel panel, string name) : base(panel, name) {

	}
}

public enum BackgroundState
{
	Initial,
	Loading,
	MainMenu,
	Level,
	Disconnected,
	Exiting,      
}

public class GameMenu(Panel parent, string name) : Menu(parent, name)
{
	public virtual int AddMenuItem(ReadOnlySpan<char> itemName, ReadOnlySpan<char> itemText, ReadOnlySpan<char> command, Panel? target, KeyValues? userData = null) {
		MenuItem item = EngineAPI.New<GameMenuItem>(this, new string(itemName));
		item.AddActionSignalTarget(target);
		item.SetCommand(command);
		item.SetText(itemText);
		item.SetUserData(userData);
		return base.AddMenuItem(item);
	}
	public void UpdateMenuItemState(bool isInGame, bool isMultiplayer) {
		for (int i = 0; i < GetChildCount(); i++) {
			Panel child = GetChild(i);
			if (child is MenuItem menuItem) {
				bool shouldBeVisible = true;
				// filter the visibility
				KeyValues? kv = menuItem.GetUserData();
				if (kv == null)
					continue;

				if (!isInGame && kv.GetInt("OnlyInGame") != 0)
					shouldBeVisible = false;
				else if (isMultiplayer && kv.GetInt("notmulti") != 0)
					shouldBeVisible = false;

				menuItem.SetVisible(shouldBeVisible);
			}
		}

		InvalidateLayout();
	}


	IPanel? MainMenuOverridePanel;

	public override void SetVisible(bool state) {
		if (MainMenuOverridePanel != null) {
			MainMenuOverridePanel.SetVisible(true);
			if (!state) 
				MainMenuOverridePanel.MoveToBack();
		}

		base.SetVisible(true);

		if (!state) 
			MoveToBack();
	}
}

public class BasePanel : Panel
{
	GameMenu GameMenu;

	[Imported] public IFileSystem FileSystem;
	[Imported] public IGameUI GameUI;
	[Imported] public IEngineClient engine;

	TextureID BackgroundImageID = TextureID.INVALID;

	public BasePanel() : base(null, "BaseGameUIPanel") {
		CreateGameMenu();
		CreateGameLogo();

		SetMenuAlpha(255);
	}

	int BackgroundFIllAlpha;

	IFont? FontTest;

	public override void PaintBackground() {
		DrawBackgroundImage();

		if(BackgroundFIllAlpha > 0) {
			Surface.GetScreenSize(out int wide, out int tall);
			Surface.DrawSetColor(0, 0, 0, BackgroundFIllAlpha);
			Surface.DrawFilledRect(0, 0, wide, tall);
		}

		Surface.DrawSetTextFont(FontTest);
		Surface.DrawSetTextPos(820, 120);
		Surface.DrawPrintText("Hello ISurface!");
	}

	Vector2 GameMenuPos;
	int GameMenuInset;

	public override void PerformLayout() {
		base.PerformLayout();

		Surface.GetScreenSize(out int wide, out int tall);
		GameMenu.GetSize(out int menuWide, out int menuTall);
		int idealMenuY = (int)GameMenuPos.Y;
		if(idealMenuY + menuTall + GameMenuInset > tall) 
			idealMenuY = tall - menuTall - GameMenuInset;

		int yDiff = idealMenuY - (int)GameMenuPos.Y;
		GameMenu.SetPos((int)GameMenuPos.X, idealMenuY);
	}

	public override void ApplySchemeSettings(IScheme scheme) {
		base.ApplySchemeSettings(scheme);

		FontTest = scheme.GetFont("Default");

		Surface.GetScreenSize(out int screenWide, out int screenTall);
		float aspectRatio = (float)screenWide / screenTall;
		bool isWidescreen = aspectRatio >= 1.5999f;

		Span<char> filename = stackalloc char[MAX_PATH];
		Span<char> background = stackalloc char[MAX_PATH];
		engine.GetMainMenuBackgroundName(background); background = background[..background.IndexOf('\0')];
		Span<char> finalFilename = filename[..sprintf(filename, "console/%s", new string(background))];

		if (BackgroundImageID == TextureID.INVALID)
			BackgroundImageID = Surface.CreateNewTextureID();

		Surface.DrawSetTextureFile(BackgroundImageID, finalFilename, 0, false);
	}

	private void DrawBackgroundImage() {
		int alpha = 255;

		GetSize(out int wide, out int tall);

		TextureID imageID = BackgroundImageID;

		Surface.DrawSetColor(255, 255, 255, alpha);
		Surface.DrawSetTexture(imageID);
		Surface.DrawTexturedRect(0, 0, wide, tall);
	}

	private void SetMenuAlpha(int alpha) {
		GameMenu.SetAlpha(alpha);
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
		GameMenu menu = EngineAPI.New<GameMenu>(this, new string(datafile.Name));
		for (KeyValues? dat = datafile.GetFirstSubKey(); dat != null; dat = dat.GetNextKey()) {
			ReadOnlySpan<char> label = dat.GetString("label", "<unknown>");
			ReadOnlySpan<char> cmd = dat.GetString("command", null);
			ReadOnlySpan<char> name = dat.GetString("name", label);

			menu.AddMenuItem(name, label, cmd, this, dat);
		}
		return menu;
	}

	private void CreateGameLogo() {

	}

	bool EverActivated;

	internal void OnGameUIActivated() {
		// Map load failed?

		if (!EverActivated) {
			UpdateGameMenus();
			EverActivated = true;
		}
	}

	private void UpdateGameMenus() {
		bool isInGame = GameUI.IsInLevel();
		bool isMulti = isInGame && engine.GetMaxClients() > 1;
		GameMenu.UpdateMenuItemState(isInGame, isMulti);

		InvalidateLayout();
		GameMenu.SetVisible(true);
	}
}
