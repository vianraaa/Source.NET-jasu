using Game.Shared;

using Source;
using Source.Common;
using Source.Common.Engine;
using Source.Common.Mathematics;

using System.Numerics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Game.Server;

public partial class BaseEntity : IServerEntity
{
	public const int TEAMNUM_NUM_BITS = 15; // < gmod increased 6 -> 15

	private static void SendProxy_AnimTime(SendProp prop, object instance, IFieldAccessor field, ref DVariant outData, int element, int objectID)
		=> throw new NotImplementedException();
	private static void SendProxy_SimulationTime(SendProp prop, object instance, IFieldAccessor field, ref DVariant outData, int element, int objectID)
		=> throw new NotImplementedException();

	public static SendTable DT_AnimTimeMustBeFirst = new(nameof(DT_AnimTimeMustBeFirst), [
		SendPropInt (FIELDOF(nameof(AnimTime)), 8, PropFlags.Unsigned|PropFlags.ChangesOften|PropFlags.EncodedAgainstTickCount, proxyFn: SendProxy_AnimTime),
	]);
	public static object? SendProxy_ClientSideAnimation(SendProp prop, object instance, IFieldAccessor data, SendProxyRecipients recipients, int objectID) {
		throw new NotImplementedException();
	}
	public static SendTable DT_PredictableId = new(nameof(DT_PredictableId), [
		SendPropPredictableId(FIELDOF(nameof(PredictableID))),
		SendPropInt(FIELDOF(nameof(IsPlayerSimulated)), 1, PropFlags.Unsigned)
	]);

	public static SendTable DT_BaseEntity = new([
		SendPropDataTable("AnimTimeMustBeFirst", DT_AnimTimeMustBeFirst, SendProxy_ClientSideAnimation),

		SendPropInt(FIELDOF(nameof(SimulationTime)), SIMULATION_TIME_WINDOW_BITS, PropFlags.Unsigned | PropFlags.ChangesOften | PropFlags.EncodedAgainstTickCount, proxyFn: SendProxy_SimulationTime /* todo */),
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
		SendPropFloat(FIELDOF(nameof(Elasticity)), 0, PropFlags.Coord | PropFlags.NoScale),
		SendPropFloat(FIELDOF(nameof(ShadowCastDistance)), 12, PropFlags.Unsigned),
		SendPropEHandle(FIELDOF(nameof(OwnerEntity))),
		SendPropEHandle(FIELDOF(nameof(EffectEntity))),
		SendPropEHandle(FIELDOF(nameof(MoveParent))),
		SendPropInt(FIELDOF(nameof(ParentAttachment)), NUM_PARENTATTACHMENT_BITS, PropFlags.Unsigned),
		SendPropInt(FIELDOF(nameof(MoveType)), (int)Source.MoveType.MaxBits, PropFlags.Unsigned ),
		SendPropInt(FIELDOF(nameof(MoveCollide)), (int)Source.MoveCollide.MaxBits, PropFlags.Unsigned ),
		SendPropQAngles (FIELDOF(nameof(AngRotation)), 24, PropFlags.ChangesOften | PropFlags.RoundDown, SendProxy_Angles ),
		SendPropInt( FIELDOF(nameof( TextureFrameIndex) ),     8, PropFlags.Unsigned ),
		SendPropDataTable( "predictable_id", DT_PredictableId, SendProxy_SendPredictableId ),
		SendPropInt(FIELDOF(nameof(SimulatedEveryTick)),       1, PropFlags.Unsigned ),
		SendPropInt(FIELDOF(nameof(AnimatedEveryTick)),        1, PropFlags.Unsigned ),
		SendPropBool( FIELDOF(nameof( AlternateSorting ))),

		// The rest of this is Garry's Mod specific in order
		SendPropInt(FIELDOF(nameof(TakeDamage)), 8),
		SendPropInt(FIELDOF(nameof(RealClassName)), 16, PropFlags.Unsigned),

		SendPropInt(FIELDOF(nameof(OverrideMaterial)), 16, PropFlags.Unsigned, SendProxy_OverrideMaterial),

		SendPropInt(FIELDOF_ARRAYINDEX(nameof(OverrideSubMaterials), 0), 16, PropFlags.Unsigned),
		SendPropArray2(null, 32, "OverrideSubMaterials"),

		SendPropInt(FIELDOF(nameof(Health)), 32, PropFlags.Normal | PropFlags.ChangesOften | PropFlags.VarInt),
		SendPropInt(FIELDOF(nameof(MaxHealth)), 32),
		SendPropInt(FIELDOF(nameof(SpawnFlags)), 32),
		SendPropInt(FIELDOF(nameof(GModFlags)), 7),
		SendPropBool(FIELDOF(nameof(OnFire))),
		SendPropFloat(FIELDOF(nameof(CreationTime)), 0, PropFlags.NoScale),

		SendPropFloat(FIELDOF_ARRAYINDEX(nameof(Velocity), 0), 0, PropFlags.NoScale | PropFlags.ChangesOften),
		SendPropFloat(FIELDOF_ARRAYINDEX(nameof(Velocity), 1), 0, PropFlags.NoScale | PropFlags.ChangesOften),
		SendPropFloat(FIELDOF_ARRAYINDEX(nameof(Velocity), 2), 0, PropFlags.NoScale | PropFlags.ChangesOften),

		SendPropGModTable(FIELDOF(nameof(GMOD_DataTable))),

		// Addon exposed data tables
		SendPropArray3(FIELDOF_ARRAY(nameof(GMOD_bool)), SendPropBool(FIELDOF_ARRAYINDEX(nameof(GMOD_bool), 0))),
		SendPropArray3(FIELDOF_ARRAY(nameof(GMOD_float)), SendPropFloat(FIELDOF_ARRAYINDEX(nameof(GMOD_float), 0))),
		SendPropArray3(FIELDOF_ARRAY(nameof(GMOD_int)), SendPropInt(FIELDOF_ARRAYINDEX(nameof(GMOD_int), 0))),
		SendPropArray3(FIELDOF_ARRAY(nameof(GMOD_Vector)),SendPropVector(FIELDOF_ARRAYINDEX(nameof(GMOD_Vector), 0))),
		SendPropArray3(FIELDOF_ARRAY(nameof(GMOD_QAngle)), SendPropQAngles(FIELDOF_ARRAYINDEX(nameof(GMOD_QAngle), 0))),
		SendPropArray3(FIELDOF_ARRAY(nameof(GMOD_EHANDLE)), SendPropEHandle(FIELDOF_ARRAYINDEX(nameof(GMOD_EHANDLE), 0))),
		SendPropString(FIELDOF(nameof(GMOD_String0))),
		SendPropString(FIELDOF(nameof(GMOD_String1))),
		SendPropString(FIELDOF(nameof(GMOD_String2))),
		SendPropString(FIELDOF(nameof(GMOD_String3))),

		// Creation IDs
		SendPropInt(FIELDOF(nameof(CreationID)), 24),
		SendPropInt(FIELDOF(nameof(MapCreatedID)), 16),
	]);

	private static void SendProxy_OverrideMaterial(SendProp prop, object instance, IFieldAccessor field, ref DVariant outData, int element, int objectID) {
		Warning("SendProxy_OverrideMaterial not yet implemented\n");
	}
	private static void SendProxy_Angles(SendProp prop, object instance, IFieldAccessor field, ref DVariant outData, int element, int objectID) {
		Warning("SendProxy_Angles not yet implemented\n");
	}
	private static object? SendProxy_SendPredictableId(SendProp prop, object instance, IFieldAccessor data, SendProxyRecipients recipients, int objectID) {
		Warning("SendProxy_SendPredictableId not yet implemented\n");
		return null;
	}

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
	public byte MoveType;
	public byte MoveCollide;
	public QAngle AngRotation;
	public bool TextureFrameIndex;
	public bool SimulatedEveryTick;
	public bool AnimatedEveryTick;
	public bool AlternateSorting;

	public byte TakeDamage;
	public ushort RealClassName;
	public ushort OverrideMaterial;
	public InlineArray32<ushort> OverrideSubMaterials;
	public int Health;
	public int MaxHealth;
	public int SpawnFlags;
	public int GModFlags;
	public bool OnFire;
	public float CreationTime;
	public Vector3 Velocity;
	public int CreationID;
	public int MapCreatedID;

	public readonly PredictableId PredictableID = new();
	public readonly bool IsPlayerSimulated;

	public readonly GModTable GMOD_DataTable = new();

	public readonly EHANDLE OwnerEntity = new();
	public readonly EHANDLE EffectEntity = new();
	public readonly EHANDLE MoveParent = new();

	InlineArray32<bool> GMOD_bool;
	InlineArray32<float> GMOD_float;
	InlineArray32<int> GMOD_int;
	InlineArray32<Vector3> GMOD_Vector;
	InlineArray32<QAngle> GMOD_QAngle;
	InlineArray32<EHANDLE> GMOD_EHANDLE; // << ENSURE THESE ARE INITIALIZED!!!!
	InlineArray512<char> GMOD_String0;
	InlineArray512<char> GMOD_String1;
	InlineArray512<char> GMOD_String2;
	InlineArray512<char> GMOD_String3;


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
