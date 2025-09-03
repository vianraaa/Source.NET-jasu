using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common;

public class ModInfo(IFileSystem fileSystem)
{
	KeyValues modData = new();
	string? GameTitle;
	string? GameTitle2;

	public bool IsSinglePlayerOnly() => modData.GetString("type", "").Equals("singleplayer_only", StringComparison.OrdinalIgnoreCase);
	public bool IsMultiPlayerOnly() => modData.GetString("type", "").Equals("multiplayer_only", StringComparison.OrdinalIgnoreCase);
	public ReadOnlySpan<char> GetFallbackDir() =>modData.GetString("fallback_dir");
	public ReadOnlySpan<char> GetGameName() => modData.GetString("game");
	public KeyValues? GetHiddenMaps() => modData.FindKey("hidden_maps");
	public bool HasPortals() => modData.GetString("hasportals", "0").Equals("1", StringComparison.OrdinalIgnoreCase);
	public bool HasHDContent() => modData.GetString("hashdcontent", "0").Equals("1", StringComparison.OrdinalIgnoreCase);
	public bool NoDifficulty() => modData.GetString("nodifficulty", "0").Equals("1", StringComparison.OrdinalIgnoreCase);
	public bool NoModels() => modData.GetString("nomodels", "0").Equals("1", StringComparison.OrdinalIgnoreCase);
	public bool NoHiModel() => modData.GetString("nohimodel", "0").Equals("1", StringComparison.OrdinalIgnoreCase);
	public bool NoCrosshair() => modData.GetString("nocrosshair", "1").Equals("1", StringComparison.OrdinalIgnoreCase);
	public bool UseGameLogo() => modData.GetString("nocrosshair", "0").Equals("1", StringComparison.OrdinalIgnoreCase);
	public bool UseBots() => modData.GetString("bots", "0").Equals("1", StringComparison.OrdinalIgnoreCase);
	public bool SupportsVR() => modData.GetInt("supportsvr") > 0;
	public bool AdvCrosshair() => modData.GetInt("advcrosshair") > 0;
	public int AdvCrosshairLevel() => modData.GetInt("advcrosshair");
	public string GetGameTitle() {
		if (GameTitle == null) {
			if (modData == null)
				return "";

			GameTitle = new(modData.GetString("title"));
		}

		return GameTitle;
	}
	public string GetGameTitle2() {
		if (GameTitle2 == null) {
			if (modData == null)
				return "";

			GameTitle2 = new(modData.GetString("title2"));
		}

		return GameTitle2;
	}

	public void LoadCurrentGameInfo() => modData.LoadFromFile(fileSystem, "gameinfo.txt");
	public void LoadGameInfoFromBuffer(ReadOnlySpan<char> buffer) => modData.LoadFromBuffer("", buffer);
	public void LoadGameInfoFromKeyValues(KeyValues keyValues) => modData = keyValues!;
}
