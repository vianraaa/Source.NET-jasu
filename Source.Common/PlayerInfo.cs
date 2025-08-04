namespace Source.Common;

public struct PlayerInfo {
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
