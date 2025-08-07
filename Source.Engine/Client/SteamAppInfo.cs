using Source.Common.Filesystem;

namespace Source.Engine.Client;

public struct SteamAppInfo
{
	public string PatchVersion;
	public string ProductName;
	public int AppID;

	public static SteamAppInfo GetSteamInf(IFileSystem fileSystem) {
		using var handle = fileSystem.Open("steam.inf", FileOpenOptions.Read, null);
		using StreamReader reader = new(handle!.Stream);
		SteamAppInfo inf = new SteamAppInfo();
		while (!reader.EndOfStream) {
			var line = reader.ReadLine();
			if (line == null) continue;
			var split = line.Split('=');
			if (split.Length < 2) continue;
			switch (split[0].ToLower()) {
				case "patchversion": inf.PatchVersion = split[1]; break;
				case "productname": inf.ProductName = split[1]; break;
				case "appid": inf.AppID = int.Parse(split[1]); break;
			}
			return inf;
		}
		return inf;
	}
}
