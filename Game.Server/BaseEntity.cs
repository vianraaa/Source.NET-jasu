using Game.Shared;

using Source.Common;
using Source.Common.Engine;

namespace Game.Server;

public partial class BaseEntity : IServerEntity
{
	public static SendTable DT_BaseEntity= new([

	]);

	public static readonly ServerClass ServerClass = new ServerClass("BaseEntity", DT_BaseEntity)
																		.WithManualClassID(StaticClassIndices.CBaseEntity);
	public Source.Common.Server.BaseEntity? GetBaseEntity() {
		throw new NotImplementedException();
	}

	public ICollideable? GetCollideable() {
		throw new NotImplementedException();
	}

	public int GetModelIndex() {
		throw new NotImplementedException();
	}

	public ReadOnlySpan<char> GetModelName() {
		throw new NotImplementedException();
	}

	public IServerNetworkable? GetNetworkable() {
		throw new NotImplementedException();
	}

	public BaseHandle? GetRefEHandle() {
		throw new NotImplementedException();
	}

	public void SetModelIndex(int index) {
		throw new NotImplementedException();
	}

	public void SetRefEHandle(BaseHandle handle) {
		throw new NotImplementedException();
	}
}
