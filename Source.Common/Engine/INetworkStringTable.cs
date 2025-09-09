namespace Source.Common.Engine;

public delegate void StringChangedDelegate(
	object? context,
	INetworkStringTable stringTable,
	int stringNumber,
	ReadOnlySpan<char> newString,
	ReadOnlySpan<byte> newData
);

public interface INetworkStringTable
{
	public const int INVALID_STRING_TABLE = -1;
	public const ushort INVALID_STRING_INDEX = ushort.MaxValue;
	public const uint MAX_TABLES = 32;

	public ReadOnlySpan<char> GetTableName();
	public int GetTableId();
	public int GetNumStrings();
	public int GetMaxStrings();
	public int GetEntryBits();
	public void SetTick(int tick);
	public bool ChangedSinceTick(int tick);
	public int AddString(bool isServer, ReadOnlySpan<char> value, int length = -1, ReadOnlySpan<byte> userData = default);
	public ReadOnlySpan<char> GetString(int stringNumber);
	public void SetStringUserData(int stringNumber, int length, ReadOnlySpan<byte> userData);
	public Span<byte> GetStringUserData(int stringNumber);
	public int FindStringIndex(ReadOnlySpan<char> value);
	public void SetStringChangedCallback(object? context, StringChangedDelegate callback);
}

public interface INetworkStringTableContainer
{
	public INetworkStringTable? CreateStringTable(
		ReadOnlySpan<char> tableName,
		int maxEntries,
		int userDataFixedSize = 0,
		int userDataNetworkBits = 0
	);
	public INetworkStringTable? CreateStringTableEx(
		ReadOnlySpan<char> tableName,
		int maxEntries,
		int userDataFixedSize = 0,
		int userDataNetworkBits = 0,
		bool isFilenames = false
	);
	public void RemoveAllTables();
	public INetworkStringTable? FindTable(ReadOnlySpan<char> tableName);
	public INetworkStringTable? GetTable(int tableId);
	public int GetNumTables();
	public void SetAllowClientSideAddString(INetworkStringTable table, bool allowClientSideAddString);
}