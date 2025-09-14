using Source.Common;
using Source.Common.Bitbuffers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;

public struct DeltaBitsReader {
	bf_read? buf;
	public DeltaBitsReader(bf_read? buf) {
		this.buf = buf;
	}

	internal uint ReadNextPropIndex() {
		throw new NotImplementedException();
	}
	public bf_read GetBitBuf() => buf;
}

public struct DeltaBitsWriter {
	bf_write buf;
	public DeltaBitsWriter(bf_write buf) {
		this.buf = buf;
	}
	public bf_write GetBitBuf() => buf;
}

[EngineComponent]
public class EngineRecvTable
{
	public bool Init() => false;

	public bool Decode(RecvTable table, object instance, bf_read inRead, int objectID, bool updateDTI = true) {
		return false;
	}
	public int MergeDeltas(RecvTable table, bf_read? oldState, bf_read newState, bf_write outState, int objectID = -1, Span<int> changedProps = default, bool updateDTI = false) {
		DeltaBitsReader oldStateReader = new(oldState);
		DeltaBitsReader newStateReader = new(newState);

		DeltaBitsWriter deltaBitsWriter = new(outState);

		return 0;
	}
}
