using Source;
using Source.Common;

using static Source.Common.Engine.IStaticPropMgrEngine;

namespace Game.Shared;

public class EntInfo {
	public IHandleEntity? Entity;
	public int SerialNumber;
	public EntInfo? Prev;
	public EntInfo? Next;
}

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

	public IHandleEntity? LookupEntity(BaseHandle handle) {
		if (handle.Index == Constants.INVALID_EHANDLE_INDEX)
			return null;

		EntInfo info = EntPtrArray[handle.GetEntryIndex()];
		if (info.SerialNumber == handle.GetSerialNumber())
			return info.Entity;
		else
			return null;
	}

	// These are notifications to the derived class. It can cache info here if it wants.
	protected virtual void OnAddEntity(IHandleEntity? pEnt, BaseHandle handle) { }
	// It is safe to delete the entity here. We won't be accessing the pointer after
	// calling OnRemoveEntity.
	protected virtual void OnRemoveEntity(IHandleEntity? pEnt, BaseHandle handle) { }

	InlineArrayNumEntEntries<EntInfo> EntPtrArray;
}
