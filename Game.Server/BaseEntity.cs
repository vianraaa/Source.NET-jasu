using Game.Shared;

using Source;
using Source.Common;
using Source.Common.Engine;

using System.Numerics;
using System.Reflection;

namespace Game.Server;

public partial class BaseEntity : IServerEntity
{
	public const int TEAMNUM_NUM_BITS = 15; // < gmod increased 6 -> 15

	private static void SendProxy_AnimTime(SendProp prop, object instance, FieldInfo field, ref DVariant outData, int element, int objectID)
		=> throw new NotImplementedException();
	private static void SendProxy_ClientSideAnimation(SendProp prop, object instance, FieldInfo field, ref DVariant outData, int element, int objectID)
		=> throw new NotImplementedException();

	public static SendTable DT_AnimTimeMustBeFirst = new(nameof(DT_AnimTimeMustBeFirst), [
		SendPropInt (FIELDOF(nameof(AnimTime)), 8, PropFlags.Unsigned|PropFlags.ChangesOften|PropFlags.EncodedAgainstTickCount, proxyFn: SendProxy_AnimTime),
	]);

	public static SendTable DT_PredictableId = new(nameof(DT_PredictableId), [

	]);

	public static SendTable DT_BaseEntity = new([
		SendPropDataTable("AnimTimeMustBeFirst", DT_AnimTimeMustBeFirst),

		SendPropInt(FIELDOF(nameof(SimulationTime)), SIMULATION_TIME_WINDOW_BITS, PropFlags.Unsigned | PropFlags.ChangesOften | PropFlags.EncodedAgainstTickCount, proxyFn: SendProxy_ClientSideAnimation /* todo */),
		SendPropVector(FIELDOF(nameof(NetworkOrigin)), -1, PropFlags.Coord | PropFlags.ChangesOften, 0, Constants.HIGH_DEFAULT, proxyFn: null /* todo */),
		SendPropInt(FIELDOF(nameof(InterpolationFrame)), NOINTERP_PARITY_MAX_BITS, PropFlags.Unsigned),
		SendPropModelIndex(FIELDOF(nameof(ModelIndex))),
		SendPropDataTable(nameof(Collision), FIELDOF(nameof(Collision)), CollisionProperty.DT_CollisionProperty),
		SendPropInt(FIELDOF(nameof(RenderFX)), 8, PropFlags.Unsigned),
		SendPropInt(FIELDOF(nameof(RenderMode)), 8, PropFlags.Unsigned),
		SendPropInt(FIELDOF(nameof(Effects)), (int)EntityEffects.MaxBits, PropFlags.Unsigned),
		SendPropInt(FIELDOF(nameof(ColorRender)), 32, PropFlags.Unsigned),
		SendPropInt(FIELDOF(nameof(TeamNum)), TEAMNUM_NUM_BITS, 0),
		SendPropInt(FIELDOF(nameof(CollisionGroup)), 5, PropFlags.Unsigned),
		SendPropFloat(FIELDOF(nameof(Elasticity)), 0, PropFlags.Coord),
		SendPropFloat(FIELDOF(nameof(ShadowCastDistance)), 12, PropFlags.Unsigned),
		SendPropEHandle(FIELDOF(nameof(OwnerEntity))),
		SendPropEHandle(FIELDOF(nameof(EffectEntity))),
		SendPropEHandle(FIELDOF(nameof(MoveParent))),
		SendPropInt(FIELDOF(nameof(ParentAttachment)), NUM_PARENTATTACHMENT_BITS, PropFlags.Unsigned),
	]);

	public byte RenderFX;
	public byte RenderMode;
	public byte OldRenderMode;
	public int Effects;
	public Color ColorRender;
	public int TeamNum;
	public int CollisionGroup;
	public float Elasticity;
	public float ShadowCastDistance;
	public byte ParentAttachment;

	public readonly EHANDLE OwnerEntity = new();
	public readonly EHANDLE EffectEntity = new();
	public readonly EHANDLE MoveParent = new();


	public static readonly ServerClass ServerClass = new ServerClass("BaseEntity", DT_BaseEntity)
																		.WithManualClassID(StaticClassIndices.CBaseEntity);

	float AnimTime;
	float SimulationTime;
	Vector3 NetworkOrigin;
	Vector3 NetworkAngles;
	byte InterpolationFrame;
	int ModelIndex;
	CollisionProperty Collision = new();


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
