using Source.Common;
using Source.Common.Bitbuffers;

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
	public int OldEntity;
	public int NewEntity;
	public int HeaderBase;
	public int HeaderCount;
}

public struct PostDataUpdateCall {
	public int Ent;
	public DataUpdateType UpdateType;
}

public class EntityReadInfo : EntityInfo {
	public bf_read? Buf;
	public int UpdateFlags;
	public bool IsEntity;
	public int Baseline;
	public bool UpdateBaselines;
	public int LocalPlayerBits;
	public int OtherPlayerBits;
	public MaxEdictsBitVec PostDataUpdateCalls;
	public int NumPostDataUpdateCalls;
}