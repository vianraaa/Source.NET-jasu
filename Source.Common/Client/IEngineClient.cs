namespace Source.Common.Client;

/// <summary>
/// Engine player info. (replica of player_info_s)
/// </summary>
public struct EnginePlayerInfo {
	public string Name;
	public int UserID;
	public string GUID;
	public uint FriendsID;
	public string FriendsName;
	public bool FakePlayer;
	public bool IsHLTV;
	public bool IsReplay;
	public uint[] CustomFiles;
	public byte FilesDownloaded;
}

public enum SkyboxVisibility {
	NotVisible,
	Skybox3D,
	Skybox2D
}

public enum ClientFrameStage
{
	Undefined = -1,
	Start,
	NetUpdateStart,
	NetUpdatePostDataUpdateStart,
	NetUpdatePostDataUpdateEnd,
	NetUpdateEnd,
	RenderStart,
	RenderEnd
}

/// <summary>
/// Interface the engine exposes to the client DLL
/// </summary>
public interface IEngineClient
{
	ReadOnlySpan<char> Key_LookupBinding(ReadOnlySpan<char> binding);
	void GetUILanguage(Span<char> dest);
	void GetMainMenuBackgroundName(Span<char> dest);
	int GetMaxClients();
}