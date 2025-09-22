using Game.Shared;

using Source;
using Source.Common;
using Source.Common.Bitbuffers;
using Source.Common.Client;
using Source.Common.Engine;
using Source.Common.Mathematics;

using Steamworks;

using System;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static System.Net.Mime.MediaTypeNames;

namespace Game.Client;
public partial class C_BaseEntity : IClientEntity
{
	private static void RecvProxy_AnimTime(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		C_BaseEntity pEntity = (C_BaseEntity)instance;
		
		long t = gpGlobals.GetNetworkBase(gpGlobals.TickCount, pEntity.EntIndex()) + data.Value.Int;

		while (t < gpGlobals.TickCount - 127)
			t += 256;
		while (t > gpGlobals.TickCount + 127)
			t -= 256;

		pEntity.AnimTime = t * TICK_INTERVAL;
	}

	private static void RecvProxy_EffectFlags(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		// ((C_BaseEntity)instance).SetEffects(data.Value.Int);
	}


	public static RecvTable DT_AnimTimeMustBeFirst = new(nameof(DT_AnimTimeMustBeFirst), [
		RecvPropInt(FIELDOF(nameof(AnimTime)), 0, RecvProxy_AnimTime),
	]);
	public static readonly ClientClass CC_AnimTimeMustBeFirst = new ClientClass("AnimTimeMustBeFirst", null, null, DT_AnimTimeMustBeFirst);


	public static RecvTable DT_PredictableId = new(nameof(DT_PredictableId), [
		RecvPropPredictableId(FIELDOF(nameof(PredictableId))),
		RecvPropInt(FIELDOF(nameof(IsPlayerSimulated))),
	]);
	public static readonly ClientClass CC_PredictableId = new ClientClass("PredictableId", null, null, DT_PredictableId);

	private static void RecvProxy_SimulationTime(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		C_BaseEntity entity = (C_BaseEntity)instance;

		int addt = data.Value.Int;
		int tickbase = (int)gpGlobals.GetNetworkBase(gpGlobals.TickCount, entity.EntIndex());

		int t = tickbase + addt;

		while (t < gpGlobals.TickCount - 127)
			t += 256;
		while (t > gpGlobals.TickCount + 127)
			t -= 256;

		entity.SimulationTime = (t * TICK_INTERVAL);
	}

	public static RecvTable DT_BaseEntity = new([
		RecvPropDataTable("AnimTimeMustBeFirst", DT_AnimTimeMustBeFirst),

		RecvPropInt(FIELDOF(nameof(SimulationTime)), 0, RecvProxy_SimulationTime),
		RecvPropInt(FIELDOF(nameof(InterpolationFrame))),

		RecvPropVector(FIELDOF(nameof(NetworkOrigin))),
		RecvPropQAngles(FIELDOF(nameof(NetworkAngles))),

		RecvPropInt(FIELDOF(nameof(ModelIndex)), 0, RecvProxy_IntToModelIndex16_BackCompatible),

		RecvPropInt(FIELDOF(nameof(Effects)), 0, RecvProxy_EffectFlags),
		RecvPropInt(FIELDOF(nameof(RenderMode))),
		RecvPropInt(FIELDOF(nameof(RenderFX))),
		RecvPropInt(FIELDOF(nameof(ColorRender))),
		RecvPropInt(FIELDOF(nameof(TeamNum))),
		RecvPropInt(FIELDOF(nameof(CollisionGroup))),
		RecvPropFloat(FIELDOF(nameof(Elasticity))),
		RecvPropFloat(FIELDOF(nameof(ShadowCastDistance))),
		RecvPropEHandle(FIELDOF(nameof(OwnerEntity))),
		RecvPropEHandle(FIELDOF(nameof(EffectEntity))),
		RecvPropInt(FIELDOF(nameof(MoveParent)), 0, RecvProxy_IntToMoveParent),
		RecvPropInt(FIELDOF(nameof(ParentAttachment))),

		RecvPropInt(FIELDOF(nameof(MoveType)), 0, RecvProxy_MoveType),
		RecvPropInt(FIELDOF(nameof(MoveCollide)), 0, RecvProxy_MoveCollide),
		RecvPropQAngles (FIELDOF(nameof(AngRotation))),
		RecvPropInt( FIELDOF(nameof( TextureFrameIndex) )),
		RecvPropDataTable( "predictable_id", DT_PredictableId ),
		RecvPropInt(FIELDOF(nameof(SimulatedEveryTick))),
		RecvPropInt(FIELDOF(nameof(AnimatedEveryTick))),
		RecvPropBool( FIELDOF(nameof( AlternateSorting ))),

		RecvPropDataTable(nameof(Collision), FIELDOF(nameof(Collision)), CollisionProperty.DT_CollisionProperty, 0, RECV_GET_OBJECT_AT_FIELD(FIELDOF(nameof(Collision)))),

		// gmod specific
		RecvPropInt(FIELDOF(nameof(TakeDamage))),
		RecvPropInt(FIELDOF(nameof(RealClassName))),

		RecvPropInt(FIELDOF(nameof(OverrideMaterial))),

		RecvPropInt(FIELDOF_ARRAYINDEX(nameof(OverrideSubMaterials), 0), PropFlags.Unsigned),
		RecvPropArray2(null, 32, "OverrideSubMaterials"),

		RecvPropInt(FIELDOF(nameof(Health))),
		RecvPropInt(FIELDOF(nameof(MaxHealth))),
		RecvPropInt(FIELDOF(nameof(SpawnFlags))),
		RecvPropInt(FIELDOF(nameof(GModFlags))),
		RecvPropBool(FIELDOF(nameof(OnFire))),
		RecvPropFloat(FIELDOF(nameof(CreationTime))),

		RecvPropFloat(FIELDOF_ARRAYINDEX(nameof(Velocity), 0)),
		RecvPropFloat(FIELDOF_ARRAYINDEX(nameof(Velocity), 1)),
		RecvPropFloat(FIELDOF_ARRAYINDEX(nameof(Velocity), 2)),

		// NW2 table
		RecvPropGModTable(FIELDOF(nameof(GMOD_DataTable))),

		// Addon exposed data tables
		RecvPropArray3(FIELDOF_ARRAY(nameof(GMOD_bool)), RecvPropBool(FIELDOF_ARRAYINDEX(nameof(GMOD_bool), 0))),
		RecvPropArray3(FIELDOF_ARRAY(nameof(GMOD_float)), RecvPropFloat(FIELDOF_ARRAYINDEX(nameof(GMOD_float), 0))),
		RecvPropArray3(FIELDOF_ARRAY(nameof(GMOD_int)), RecvPropInt(FIELDOF_ARRAYINDEX(nameof(GMOD_int), 0))),
		RecvPropArray3(FIELDOF_ARRAY(nameof(GMOD_Vector)), RecvPropVector(FIELDOF_ARRAYINDEX(nameof(GMOD_Vector), 0))),
		RecvPropArray3(FIELDOF_ARRAY(nameof(GMOD_QAngle)), RecvPropQAngles(FIELDOF_ARRAYINDEX(nameof(GMOD_QAngle), 0))),
		RecvPropArray3(FIELDOF_ARRAY(nameof(GMOD_EHANDLE)), RecvPropEHandle(FIELDOF_ARRAYINDEX(nameof(GMOD_EHANDLE), 0))),
		RecvPropString(FIELDOF(nameof(GMOD_String0))),
		RecvPropString(FIELDOF(nameof(GMOD_String1))),
		RecvPropString(FIELDOF(nameof(GMOD_String2))),
		RecvPropString(FIELDOF(nameof(GMOD_String3))),

		// Creation IDs
		RecvPropInt(FIELDOF(nameof(CreationID))),
		RecvPropInt(FIELDOF(nameof(MapCreatedID))),
	]);

	private static void RecvProxy_OverrideMaterial(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		Warning("RecvProxy_OverrideMaterial not implemented yet\n");
	}

	private static void RecvProxy_MoveCollide(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		Warning("RecvProxy_MoveCollide not implemented yet\n");
	}

	private static void RecvProxy_MoveType(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		Warning("RecvProxy_MoveType not implemented yet\n");
	}

	public static readonly ClientClass ClientClass = new ClientClass("BaseEntity", null, null, DT_BaseEntity)
																		.WithManualClassID(StaticClassIndices.CBaseEntity);

	public int Index;

	private Model? Model;

	public double AnimTime;
	public double OldAnimTime;

	public double SimulationTime;
	public double OldSimulationTime;

	public double CreateTime;

	public byte InterpolationFrame;
	public byte OldInterpolationFrame;
	public short ModelIndex;
	public byte ParentAttachment;

	public byte MoveType;
	public byte MoveCollide;
	public QAngle AngRotation;
	public bool TextureFrameIndex;
	public bool SimulatedEveryTick;
	public bool AnimatedEveryTick;
	public bool AlternateSorting;

	public readonly PredictableId PredictableId = new();
	public readonly bool IsPlayerSimulated;

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

	public CollisionProperty Collision = new();

	int Effects;
	byte RenderMode;
	byte RenderFX;
	byte RenderFXBlend;
	Color ColorRender;
	int CollisionGroup;
	float Elasticity;
	float ShadowCastDistance;
	byte OldRenderMode;

	InlineArray32<bool> GMOD_bool;
	InlineArray32<float> GMOD_float;
	InlineArray32<int> GMOD_int;
	InlineArray32<Vector3> GMOD_Vector;
	InlineArray32<QAngle> GMOD_QAngle;
	InlineArrayNew32<EHANDLE> GMOD_EHANDLE = new(); 
	InlineArray512<char> GMOD_String0;
	InlineArray512<char> GMOD_String1;
	InlineArray512<char> GMOD_String2;
	InlineArray512<char> GMOD_String3;

	public readonly GModTable GMOD_DataTable = new();

	public double Speed;
	public int TeamNum;

	EHANDLE OwnerEntity = new();
	EHANDLE EffectEntity = new();


	long CreationTick;

	bool OldShouldDraw;

	float Friction;
	Vector3 AbsOrigin;
	QAngle AngAbsRotation;
	Vector3 OldOrigin;
	QAngle OldAngRotation;

	Matrix4x4 CoordinateFrame;
	Vector3 NetworkOrigin;
	QAngle NetworkAngles;

	Handle<C_BasePlayer> PlayerSimulationOwner = new();
	int DataChangeEventRef;

	public void ClientThink() {
		throw new NotImplementedException();
	}

	public int DrawModel(int flags) {
		throw new NotImplementedException();
	}

	public ref readonly QAngle GetAbsAngles() {
		throw new NotImplementedException();
	}

	public ref readonly Vector3 GetAbsOrigin() {
		throw new NotImplementedException();
	}

	public virtual ClientClass GetClientClass() => ClientClassRetriever.GetOrError(GetType());


	public IClientNetworkable GetClientNetworkable() => this;
	public Source.Common.IClientRenderable GetClientRenderable() => this;
	public IClientThinkable GetClientThinkable() => this;
	public IClientEntity GetIClientEntity() => this;
	public IClientUnknown GetIClientUnknown() => this;



	public Model? GetModel() {
		throw new NotImplementedException();
	}

	public ref readonly QAngle GetRenderAngles() {
		throw new NotImplementedException();
	}

	public void GetRenderBounds(out Vector3 mins, out Vector3 maxs) {
		throw new NotImplementedException();
	}

	public void GetRenderBoundsWorldspace(out Vector3 mins, out Vector3 maxs) {
		throw new NotImplementedException();
	}

	public ref readonly Vector3 GetRenderOrigin() {
		throw new NotImplementedException();
	}

	public bool IsTransparent() {
		throw new NotImplementedException();
	}

	public void UpdateOnRemove() {
		// VPhysicsDestroyObject();

		// Assert(GetMoveParent() == null);
		// UnlinkFromHierarchy();
		// SetGroundEntity(NULL);
	}

	readonly EHANDLE MoveParent = new();
	readonly EHANDLE MoveChild = new();
	readonly EHANDLE MovePeer = new();
	readonly EHANDLE MovePrevPeer = new();

	public void UnlinkFromHierarchy() {
		// todo
	}

	public void Release() {
		UnlinkFromHierarchy();

		//  if (IsIntermediateDataAllocated()) 
		//  	DestroyIntermediateData();

		UpdateOnRemove();
	}

	public bool SetupBones(ref Matrix4x4 boneToWOrldOut, int maxBones, int boneMask, double currentTime) {
		throw new NotImplementedException();
	}

	public virtual bool ShouldDraw() {
		if ((RenderMode)RenderMode == Source.RenderMode.None)
			return false;

		return Model != null && !IsEffectActive(EntityEffects.NoDraw) && Index != 0;
	}

	private bool IsEffectActive(EntityEffects fx) {
		return ((EntityEffects)Effects & fx) != 0;
	}

	public virtual bool Init(int entNum, int serialNum) {
		cl_entitylist.AddNetworkableEntity(GetIClientUnknown(), entNum, serialNum);

		CreationTick = gpGlobals.TickCount;

		return true;
	}

	public virtual EntityCapabilities ObjectCaps() => 0;

	public virtual void Dispose() {
		GC.SuppressFinalize(this);
	}

	double SpawnTime;
	double LastMessageTime;

	public virtual void PostDataUpdate(DataUpdateType updateType) {
		// todo
		MarkMessageReceived();
	}

	protected readonly IEngineClient engine = Singleton<IEngineClient>();

	protected void MarkMessageReceived() {
		LastMessageTime = engine.GetLastTimeStamp();
	}

	public void NotifyShouldTransmit(ShouldTransmiteState state) {
		throw new NotImplementedException();
	}

	public void OnPreDataChanged(DataUpdateType updateType) {
		throw new NotImplementedException();
	}

	public void OnDataChanged(DataUpdateType updateType) {
		throw new NotImplementedException();
	}

	public virtual void Spawn() {

	}

	//PredictableId PredictionId;
	//PredictionContext? PredictionContext;
	object? PredictionContext;
	bool Dormant;
	bool Predictable;

	public bool GetPredictable() => Predictable;
	public void SetPredictable(bool state) {
		Predictable = state;
		// todo: interp
	}

	public bool IsClientCreated() {
		if (PredictionContext != null) {
			Assert(GetPredictable() == null);
			return true;
		}
		return false;
	}

	public Matrix4x4 EntityToWorldTransform() {
		CalcAbsolutePosition();
		return CoordinateFrame;
	}

	public Vector3 GetNetworkOrigin() => NetworkOrigin;
	public QAngle GetNetworkAngles() => NetworkAngles;

	private void CalcAbsolutePosition() {

	}

	public void PreDataUpdate(DataUpdateType updateType) {
		if (AddDataChangeEvent(this, updateType, ref DataChangeEventRef))
			OnPreDataChanged(updateType);

		bool newentity = (updateType == DataUpdateType.Created);

		// if (!newentity) 
		// Interp_RestoreToLastNetworked(GetVarMapping());

		if (newentity && !IsClientCreated()) {
			SpawnTime = engine.GetLastTimeStamp();
			Spawn();
		}

		OldOrigin = GetNetworkOrigin();
		OldAngRotation = GetNetworkAngles();

		OldAnimTime = AnimTime;
		OldSimulationTime = SimulationTime;

		OldRenderMode = RenderMode;


		OldInterpolationFrame = InterpolationFrame;
		OldShouldDraw = ShouldDraw();
	}

	private bool AddDataChangeEvent(C_BaseEntity c_BaseEntity, DataUpdateType updateType, ref int storedEvent) {
		if (storedEvent >= 0) {
			// todo
			return false;
		}
		else {
			// todo
			return true;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsDormant() => IsServerEntity() ? Dormant : false;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsServerEntity() => Index != -1;

	public int EntIndex() {
		return Index;
	}

	public void ReceiveMessage(int classID, bf_read msg) {
		throw new NotImplementedException();
	}

	public void SetDestroyedOnRecreateEntities() {

	}

	public virtual ICollideable GetCollideable() => throw new NotImplementedException();
	public virtual BaseHandle GetRefEHandle() {
		return RefEHandle;
	}
	public virtual void SetRefEHandle(BaseHandle handle) {
		RefEHandle = handle;
	}

	public void OnDataUnchangedInPVS() {
		// HierarchySetParent(NetworkMoveParent);
		MarkMessageReceived();
	}

	public virtual IPVSNotify? GetPVSNotifyInterface() {
		return null;
	}

	public virtual object GetDataTableBasePtr() {
		return this;
	}

	BaseHandle? RefEHandle;
}
