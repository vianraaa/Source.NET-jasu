using Game.Shared;

using Source;
using Source.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client;

public class ClientEntityList : BaseEntityList, IClientEntityList
{
	public IClientEntity? GetClientEntity(int entNum) {
		throw new NotImplementedException();
	}

	public IClientEntity? GetClientEntityFromHandle(BaseHandle ent) {
		throw new NotImplementedException();
	}

	public IClientNetworkable? GetClientNetworkable(int entnNum) {
		return EntityCacheInfo[entnNum].Networkable;
	}

	public IClientNetworkable? GetClientNetworkableFromHandle(BaseHandle ent) {
		throw new NotImplementedException();
	}

	public IClientUnknown? GetClientUnknownFromHandle(BaseHandle ent) {
		throw new NotImplementedException();
	}

	public int GetHighestEntityIndex() {
		return MaxUsedServerIndex;
	}

	public int GetMaxEntities() {
		throw new NotImplementedException();
	}

	public int NumberOfEntities(bool includeNonNetworkable) {
		throw new NotImplementedException();
	}

	public void SetMaxEntities(int maxEnts) {
		throw new NotImplementedException();
	}

	int NumServerEnts;
	int MaxServerEnts;
	int NumClientNonNetworkable;
	int MaxUsedServerIndex;

	struct EntityCacheInfo_t {
		public IClientNetworkable Networkable;
		public LinkedListNode<C_BaseEntity> LinkedListNode;
	}

	InlineArrayNumEntEntries<EntityCacheInfo_t> EntityCacheInfo;
	readonly LinkedList<C_BaseEntity> BaseEntities = [];
}
