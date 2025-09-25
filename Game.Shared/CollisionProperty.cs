#if CLIENT_DLL
using Game.Client;

using Source;

#endif

#if GAME_DLL
using Game.Server;

using Source;

#endif

using Source.Common;
using Source.Common.Hashing;

using System.Numerics;
using System.Reflection;

namespace Game.Shared;

public enum SurroundingBoundsType {
	UseOBBCollisionBounds = 0,
	UseBestCollisionBounds,      
	UseHitboxes,
	UseSpecifiedBounds,
	UseGameCode,
	UseRotationExpandedBounds,
	UseCollisionBoundsNeverVPhysics,

	BitCount = 3
}

public class CollisionProperty
{
#if CLIENT_DLL

	private static void RecvProxy_VectorDirtySurround(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
		Vector3 vecold = field.GetValue<Vector3>(instance);
		Vector3 vecnew = data.Value.Vector;
		if (vecold != vecnew) {
			field.SetValue(instance, in vecnew);
			((CollisionProperty)instance)!.MarkSurroundingBoundsDirty();
		}
	}


	private static void RecvProxy_SolidFlags(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
		field.SetValue(instance, data.Value.Int);
	}

	private static void RecvProxy_Solid(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
		field.SetValue(instance, data.Value.Int);
	}

	private static void RecvProxy_OBBMinsPreScaled(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
		Warning($"RecvProxy_OBBMinsPreScaled not yet implemented\n");
	}

	private static void RecvProxy_OBBMaxPreScaled(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
		Warning($"RecvProxy_OBBMaxPreScaled not yet implemented\n");
	}

	private static void RecvProxy_IntDirtySurround(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
		Warning($"RecvProxy_IntDirtySurround not yet implemented\n");
	}
#else
	private static void SendProxy_SolidFlags(SendProp prop, object instance, IFieldAccessor field, ref DVariant outData, int element, int objectID) {
		throw new NotImplementedException();
	}

	private static void SendProxy_Solid(SendProp prop, object instance, IFieldAccessor field, ref DVariant outData, int element, int objectID) {
		throw new NotImplementedException();
	}
#endif

	Vector3 MinsPreScaled;
	Vector3 MaxsPreScaled;
	Vector3 Mins;
	Vector3 Maxs;
	float Radius;
	ushort SolidFlags;
	SpatialPartitionHandle_t Partition;
	byte SurroundType;
	byte SolidType;

	byte TriggerBloat;
	Vector3 SpecifiedSurroundingMinsPreScaled;
	Vector3 SpecifiedSurroundingMaxsPreScaled;
	Vector3 SpecifiedSurroundingMins;
	Vector3 SpecifiedSurroundingMaxs;


	private void MarkSurroundingBoundsDirty() {

	}

#if CLIENT_DLL
	public static RecvTable DT_CollisionProperty = new([
		RecvPropVector(FIELDOF(nameof(MinsPreScaled)), 0, RecvProxy_OBBMinsPreScaled),
		RecvPropVector(FIELDOF(nameof(MaxsPreScaled)), 0, RecvProxy_OBBMaxPreScaled),
		RecvPropVector(FIELDOF(nameof(Mins)), 0),
		RecvPropVector(FIELDOF(nameof(Maxs)), 0),
		RecvPropInt(FIELDOF(nameof(SolidType)), 0, RecvProxy_Solid),
		RecvPropInt(FIELDOF(nameof(SolidFlags)), 0, RecvProxy_SolidFlags),
		RecvPropInt(FIELDOF(nameof(SurroundType)), 0, RecvProxy_IntDirtySurround),
		RecvPropInt(FIELDOF(nameof(TriggerBloat)), 0, RecvProxy_IntDirtySurround),
		RecvPropVector(FIELDOF(nameof(SpecifiedSurroundingMinsPreScaled)), 0, RecvProxy_VectorDirtySurround),
		RecvPropVector(FIELDOF(nameof(SpecifiedSurroundingMaxsPreScaled)), 0, RecvProxy_VectorDirtySurround),
		RecvPropVector(FIELDOF(nameof(SpecifiedSurroundingMins)), 0, RecvProxy_VectorDirtySurround),
		RecvPropVector(FIELDOF(nameof(SpecifiedSurroundingMaxs)), 0, RecvProxy_VectorDirtySurround),
	]);

	public static readonly ClientClass CC_CollisionProperty = new ClientClass("CollisionProperty", null, null, DT_CollisionProperty);
#else
	public static SendTable DT_CollisionProperty = new([
		SendPropVector(FIELDOF(nameof(MinsPreScaled)), 0, PropFlags.NoScale),
		SendPropVector(FIELDOF(nameof(MaxsPreScaled)), 0, PropFlags.NoScale),
		SendPropVector(FIELDOF(nameof(Mins)), 0, PropFlags.NoScale),
		SendPropVector(FIELDOF(nameof(Maxs)), 0, PropFlags.NoScale),
		SendPropInt(FIELDOF(nameof(SolidType)), 3, PropFlags.Unsigned, SendProxy_Solid),
		SendPropInt(FIELDOF(nameof(SolidFlags)), (int)Source.SolidFlags.MaxBits, PropFlags.Unsigned, SendProxy_SolidFlags),
		SendPropInt(FIELDOF(nameof(SurroundType)), (int)SurroundingBoundsType.BitCount, PropFlags.Unsigned),
		SendPropInt(FIELDOF(nameof(TriggerBloat)), 0, PropFlags.Unsigned),
		SendPropVector(FIELDOF(nameof(SpecifiedSurroundingMinsPreScaled)), 0, PropFlags.NoScale),
		SendPropVector(FIELDOF(nameof(SpecifiedSurroundingMaxsPreScaled)), 0, PropFlags.NoScale),
		SendPropVector(FIELDOF(nameof(SpecifiedSurroundingMins)), 0, PropFlags.NoScale),
		SendPropVector(FIELDOF(nameof(SpecifiedSurroundingMaxs)), 0, PropFlags.NoScale),
	]);


	public static readonly ServerClass CC_CollisionProperty = new ServerClass("CollisionProperty", DT_CollisionProperty)
																		.WithManualClassID(StaticClassIndices.CBaseEntity);
#endif
}
