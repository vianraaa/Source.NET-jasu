using Source.Common;

using static Source.Common.Engine.IStaticPropMgrEngine;

namespace Game.Shared;

public class BaseEntityList
{
	public BaseHandle AddNetworkableEntity(IHandleEntity pEnt, int index, int iForcedSerialNum = -1) {
		throw new NotImplementedException();
	}
	public BaseHandle AddNonNetworkableEntity(IHandleEntity pEnt) {
		throw new NotImplementedException();
	}
	public void RemoveHandle(BaseHandle handle) {
		throw new NotImplementedException();
	}

	// These are notifications to the derived class. It can cache info here if it wants.
	protected virtual void OnAddEntity(IHandleEntity? pEnt, BaseHandle handle) { }
	// It is safe to delete the entity here. We won't be accessing the pointer after
	// calling OnRemoveEntity.
	protected virtual void OnRemoveEntity(IHandleEntity? pEnt, BaseHandle handle) { }
}
