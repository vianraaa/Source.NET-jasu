using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common;

public class ServerClass
{
	public string NetworkName;
	public SendTable Table;
	public ServerClass? Next;
	public int ClassID;
	public int InstanceBaselineIndex;
	public ServerClass(ReadOnlySpan<char> networkName, SendTable table) {

	}
}
