using Source.Common;
using Source.Common.Bitbuffers;
using Source.Common.Networking;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;
public class EntityInfo {
	public bool AsDelta;
	public ClientFrame? From;
	public ClientFrame? To;
	public UpdateType UpdateType;
	public int OldEntity = -1;
	public int NewEntity = -1;
	public int HeaderBase = -1;
	public int HeaderCount;


	public void NextOldEntity() {
		if (From != null) {
			OldEntity = From.TransmitEntity.FindNextSetBit(OldEntity + 1);

			if (OldEntity < 0)
				OldEntity = int.MaxValue;
		}
		else
			OldEntity = int.MaxValue;
	}

	public void NextNewEntity() {
		NewEntity = To!.TransmitEntity.FindNextSetBit(NewEntity + 1);
		if (NewEntity < 0)
			NewEntity = int.MaxValue;
	}
}

public struct PostDataUpdateCall {
	public int Ent;
	public DataUpdateType UpdateType;
}

public class EntityReadInfo : EntityInfo {
	public bf_read? Buf;
	public DeltaEncodingFlags UpdateFlags;
	public bool IsEntity;
	public int Baseline;
	public bool UpdateBaselines;
	public int LocalPlayerBits;
	public int OtherPlayerBits;
	public InlineArrayMaxEdicts<PostDataUpdateCall> PostDataUpdateCalls;
	public int NumPostDataUpdateCalls;
}