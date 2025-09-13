using Game.Shared;

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
		throw new NotImplementedException();
	}

	public IClientNetworkable? GetClientNetworkableFromHandle(BaseHandle ent) {
		throw new NotImplementedException();
	}

	public IClientUnknown? GetClientUnknownFromHandle(BaseHandle ent) {
		throw new NotImplementedException();
	}

	public int GetHighestEntityIndex() {
		throw new NotImplementedException();
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
}
