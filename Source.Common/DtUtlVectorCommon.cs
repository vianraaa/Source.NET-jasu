using Source.Common.Utilities;

namespace Source.Common;

public static class DtUtlVectorCommon {
	static readonly HashSet<UtlSymbol> sendTables = [];
	static readonly HashSet<UtlSymbol> recvTables = [];
	public static string AllocateUniqueDataTableName(bool sendTable, string name) {
		if (sendTable && !sendTables.Add(new(name)))
			AssertMsg(false, "EnsureUniqueDataTableName: Send table duplicate");
		else if (!sendTable && !recvTables.Add(new(name)))
			AssertMsg(false, "EnsureUniqueDataTableName: Recv table duplicate");

		return name;
	}
}
