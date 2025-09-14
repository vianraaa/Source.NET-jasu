using Source.Common;
using Source.Common.Bitbuffers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;

[EngineComponent]
public class EngineRecvTable
{
	public bool Init() => false;

	public bool Decode(RecvTable table, object instance, bf_read inREad, int objectID, bool updateDTI = true) {
		return false;
	}
	public int MergeDeltas(RecvTable table, bf_read? oldState, bf_read newState, bf_write outState, int objectID = -1, Span<int> changedProps = default, bool updateDTI = false) {
		return 0;
	}
}
