using Source.Common;
using Source.Common.Bitbuffers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;


public struct DeltaBitsReader : IDisposable
{
	bf_read? Buffer;
	int LastProp;

	public readonly bf_read GetBitBuf() => Buffer!;

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

	public void SkipPropData(SendProp prop) => BasePropTypeFns.Get(prop.Type).SkipProp(prop, Buffer!);
	public void CopyPropData(bf_write outWrite, SendProp prop) {
		int start = Buffer!.BitsRead;
		BasePropTypeFns.Get(prop.Type).SkipProp(prop, Buffer!);
		int len = Buffer!.BitsRead - start;
		Buffer!.Seek(start);
		outWrite.WriteBitsFromBuffer(Buffer!, len);
	}
	public void ComparePropData(ref DeltaBitsReader inReader, SendProp prop) => BasePropTypeFns.Get(prop.Type).CompareDeltas(prop, Buffer!, inReader.Buffer!);

	public void Dispose() {
		Assert(Buffer == null);
	}
}

public struct DeltaBitsWriter : IDisposable
{
	bf_write? buf;
	int LastProp;
	public DeltaBitsWriter(bf_write buf) {
		this.buf = buf;
		LastProp = -1;
	}

	public readonly bf_write GetBitBuf() => buf!;

	public void WritePropIndex(int prop) {
		Assert(prop >= 0 && prop < Constants.MAX_DATATABLE_PROPS);
		uint diff = (uint)(prop - LastProp);
		LastProp = prop;
		Assert(diff > 0 && diff <= Constants.MAX_DATATABLE_PROPS);
		int n = ((diff < 0x11u) ? -1 : 0) + ((diff < 0x101u) ? -1 : 0);
		buf!.WriteUBitLong((uint)(diff * 8 - 8 + 4 + n * 2 + 1), 8 + n * 4 + 4 + 2 + 1);
	}

	public readonly void Dispose() {
		buf!.WriteOneBit(0);
	}
}



[EngineComponent]
public class EngineRecvTable
{
	public bool Init() => false;

	public bool Decode(RecvTable table, object instance, bf_read inRead, int objectID, bool updateDTI = true) {
		return false;
	}
	public int MergeDeltas(RecvTable table, bf_read? oldState, bf_read newState, bf_write outState, int objectID = -1, Span<int> changedProps = default, bool updateDTI = false) {
		using DeltaBitsReader oldStateReader = new(oldState);
		using DeltaBitsReader newStateReader = new(newState);

		using DeltaBitsWriter deltaBitsWriter = new(outState);



		return 0;
	}
}
