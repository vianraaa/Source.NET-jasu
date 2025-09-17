using Game.Shared;

using Source.Common;
using Source.Common.Engine;

using System.Numerics;

namespace Game.Server;

public partial class BaseEntity : IServerEntity
{
	public static SendTable DT_AnimTimeMustBeFirst = new(nameof(DT_AnimTimeMustBeFirst), [

	]);

	public static SendTable DT_PredictableId = new(nameof(DT_PredictableId), [

	]);

	public static SendTable DT_BaseEntity = new([
		SendPropDataTable("AnimTimeMustBeFirst", FIELDOF(nameof(DT_AnimTimeMustBeFirst))),
	]);

	public static readonly ServerClass ServerClass = new ServerClass("BaseEntity", DT_BaseEntity)
																		.WithManualClassID(StaticClassIndices.CBaseEntity);

	float AnimTime;
	float SimulationTime;
	Vector3 Origin;
	byte InterpolationFrame;
	int ModelIndex;

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
