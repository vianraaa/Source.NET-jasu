using Source.Common;
using Source.Common.Bitbuffers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;


public struct DeltaBitsReader
{
	bf_read? Buffer;
	int LastProp;
	public DeltaBitsReader(bf_read? buf) {
		Buffer = buf;
		LastProp = -1;
	}
	
	public void ForceFinished() {
		Buffer = null;
	}

	public uint ReadNextPropIndex() {
		Assert(Buffer != null);

		if (Buffer.BitsLeft >= 7) {
			uint bits = Buffer.ReadUBitLong(7);
			if ((bits & 1) != 0) {
				uint delta = bits >> 3;
				if ((bits & 6) != 0)
					delta = Buffer.ReadUBitVarInternal((int)((bits & 6) >> 1));

				LastProp = (int)(LastProp + 1 + delta);
				Assert(LastProp < Constants.MAX_DATATABLE_PROPS);
				return (uint)LastProp;
			}
			Buffer.BitsRead -= 6;
		}
		else {
			if (Buffer.ReadOneBit() != 0)
				Buffer.Seek(-1);
		}
		ForceFinished();
		return ~0u;
	}
	public bf_read GetBitBuf() => Buffer;
}

public struct DeltaBitsWriter
{
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
