using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common;

public class ServerClass
{
	public static ServerClass? Head;

	public string NetworkName;
	public SendTable Table;
	public ServerClass? Next;
	public int ClassID;
	public int InstanceBaselineIndex;
	public ServerClass(ReadOnlySpan<char> networkName, SendTable table, [CallerArgumentExpression(nameof(table))] string? nameOfTable = null) {
		NetworkName = new(networkName);
		Table = table;
		if (nameOfTable != null)
			table.NetTableName = nameOfTable;

		Next = Head;
		Head = this;

	}

	public ServerClass WithManualClassID(int classID) {
		ClassID = classID;
		return this;
	}
}
