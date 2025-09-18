using Game.Shared;

using Source;
using Source.Common;
using Source.Common.Engine;

using System.Numerics;
using System.Reflection;

namespace Game.Server;

public partial class BaseEntity : IServerEntity
{
	private static void SendProxy_AnimTime(SendProp prop, object instance, FieldInfo field, ref DVariant outData, int element, int objectID) {}
	
	public static SendTable DT_AnimTimeMustBeFirst = new(nameof(DT_AnimTimeMustBeFirst), [
		SendPropInt (FIELDOF(nameof(AnimTime)), 8, PropFlags.Unsigned|PropFlags.ChangesOften|PropFlags.EncodedAgainstTickCount, proxyFn: SendProxy_AnimTime),
	]);

	public static SendTable DT_PredictableId = new(nameof(DT_PredictableId), [

	]);

	public static SendTable DT_BaseEntity = new([
		SendPropDataTable("AnimTimeMustBeFirst", FIELDOF(nameof(DT_AnimTimeMustBeFirst))),

		SendPropInt(FIELDOF(nameof(SimulationTime)), SIMULATION_TIME_WINDOW_BITS, PropFlags.Unsigned | PropFlags.ChangesOften | PropFlags.EncodedAgainstTickCount, proxyFn: null /* todo */),
		SendPropVector(FIELDOF(nameof(NetworkOrigin)), -1, PropFlags.Coord | PropFlags.ChangesOften, 0, Constants.HIGH_DEFAULT, proxyFn: null /* todo */),
		SendPropInt(FIELDOF(nameof(InterpolationFrame)), NOINTERP_PARITY_MAX_BITS, PropFlags.Unsigned),
		SendPropModelIndex(FIELDOF(nameof(ModelIndex))),
	]);

	public static readonly ServerClass ServerClass = new ServerClass("BaseEntity", DT_BaseEntity)
																		.WithManualClassID(StaticClassIndices.CBaseEntity);

	float AnimTime;
	float SimulationTime;
	Vector3 NetworkOrigin;
	Vector3 NetworkAngles;
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
