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

using FIELD = Source.FIELD<Game.Client.C_BaseEntity>;

using static System.Net.Mime.MediaTypeNames;

namespace Game.Client;
public partial class C_BaseEntity : IClientEntity
{
	private static void RecvProxy_AnimTime(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
		C_BaseEntity pEntity = (C_BaseEntity)instance;

		long t = gpGlobals.GetNetworkBase(gpGlobals.TickCount, pEntity.EntIndex()) + data.Value.Int;

		while (t < gpGlobals.TickCount - 127)
			t += 256;
		while (t > gpGlobals.TickCount + 127)
			t -= 256;

		pEntity.AnimTime = t * TICK_INTERVAL;
	}

	private static void RecvProxy_EffectFlags(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
		// ((C_BaseEntity)instance).SetEffects(data.Value.Int);
	}


	public static RecvTable DT_AnimTimeMustBeFirst = new(nameof(DT_AnimTimeMustBeFirst), [
		RecvPropInt(FIELD.OF(nameof(AnimTime)), 0, RecvProxy_AnimTime),
	]);
	public static readonly ClientClass CC_AnimTimeMustBeFirst = new ClientClass("AnimTimeMustBeFirst", null, null, DT_AnimTimeMustBeFirst);


	public static RecvTable DT_PredictableId = new(nameof(DT_PredictableId), [
		RecvPropPredictableId(FIELD.OF(nameof(PredictableId))),
		RecvPropInt(FIELD.OF(nameof(IsPlayerSimulated))),
	]);
	public static readonly ClientClass CC_PredictableId = new ClientClass("PredictableId", null, null, DT_PredictableId);

	private static void RecvProxy_SimulationTime(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
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

		RecvPropInt(FIELD.OF(nameof(SimulationTime)), 0, RecvProxy_SimulationTime),
		RecvPropInt(FIELD.OF(nameof(InterpolationFrame))),

		RecvPropVector(FIELD.OF_NAMED(nameof(NetworkOrigin), nameof(Origin))),
		RecvPropQAngles(FIELD.OF_NAMED(nameof(NetworkAngles), nameof(Rotation))),

		RecvPropInt(FIELD.OF(nameof(ModelIndex)), 0, RecvProxy_IntToModelIndex16_BackCompatible),

		RecvPropInt(FIELD.OF(nameof(Effects)), 0, RecvProxy_EffectFlags),
		RecvPropInt(FIELD.OF(nameof(RenderMode))),
		RecvPropInt(FIELD.OF(nameof(RenderFX))),
		RecvPropInt(FIELD.OF(nameof(ColorRender))),
		RecvPropInt(FIELD.OF(nameof(TeamNum))),
		RecvPropInt(FIELD.OF(nameof(CollisionGroup))),
		RecvPropFloat(FIELD.OF(nameof(Elasticity))),
		RecvPropFloat(FIELD.OF(nameof(ShadowCastDistance))),
		RecvPropEHandle(FIELD.OF(nameof(OwnerEntity))),
		RecvPropEHandle(FIELD.OF(nameof(EffectEntity))),
		RecvPropInt(FIELD.OF(nameof(MoveParent)), 0, RecvProxy_IntToMoveParent),
		RecvPropInt(FIELD.OF(nameof(ParentAttachment))),

		RecvPropInt(FIELD.OF(nameof(MoveType)), 0, RecvProxy_MoveType),
		RecvPropInt(FIELD.OF(nameof(MoveCollide)), 0, RecvProxy_MoveCollide),
		RecvPropQAngles (FIELD.OF(nameof(AngRotation))),
		RecvPropInt( FIELD.OF(nameof( TextureFrameIndex) )),
		RecvPropDataTable( "predictable_id", DT_PredictableId ),
		RecvPropInt(FIELD.OF(nameof(SimulatedEveryTick))),
		RecvPropInt(FIELD.OF(nameof(AnimatedEveryTick))),
		RecvPropBool( FIELD.OF(nameof( AlternateSorting ))),

		RecvPropDataTable(nameof(Collision), FIELD.OF(nameof(Collision)), CollisionProperty.DT_CollisionProperty, 0, RECV_GET_OBJECT_AT_FIELD(FIELD.OF(nameof(Collision)))),

		// gmod specific
		RecvPropInt(FIELD.OF(nameof(TakeDamage))),
		RecvPropInt(FIELD.OF(nameof(RealClassName))),

		RecvPropInt(FIELD.OF(nameof(OverrideMaterial))),

		RecvPropInt(FIELD.OF_ARRAYINDEX(nameof(OverrideSubMaterials), 0), PropFlags.Unsigned),
		RecvPropArray2(null, 32, "OverrideSubMaterials"),

		RecvPropInt(FIELD.OF(nameof(Health))),
		RecvPropInt(FIELD.OF(nameof(MaxHealth))),
		RecvPropInt(FIELD.OF(nameof(SpawnFlags))),
		RecvPropInt(FIELD.OF(nameof(GModFlags))),
		RecvPropBool(FIELD.OF(nameof(OnFire))),
		RecvPropFloat(FIELD.OF(nameof(CreationTime))),

		RecvPropFloat(FIELD.OF_ARRAYINDEX(nameof(Velocity), 0)),
		RecvPropFloat(FIELD.OF_ARRAYINDEX(nameof(Velocity), 1)),
		RecvPropFloat(FIELD.OF_ARRAYINDEX(nameof(Velocity), 2)),

		// NW2 table
		RecvPropGModTable(FIELD.OF(nameof(GMOD_DataTable))),

		// Addon exposed data tables
		RecvPropArray3(FIELD.OF_ARRAY(nameof(GMOD_bool)), RecvPropBool(FIELD.OF_ARRAYINDEX(nameof(GMOD_bool), 0))),
		RecvPropArray3(FIELD.OF_ARRAY(nameof(GMOD_float)), RecvPropFloat(FIELD.OF_ARRAYINDEX(nameof(GMOD_float), 0))),
		RecvPropArray3(FIELD.OF_ARRAY(nameof(GMOD_int)), RecvPropInt(FIELD.OF_ARRAYINDEX(nameof(GMOD_int), 0))),
		RecvPropArray3(FIELD.OF_ARRAY(nameof(GMOD_Vector)), RecvPropVector(FIELD.OF_ARRAYINDEX(nameof(GMOD_Vector), 0))),
		RecvPropArray3(FIELD.OF_ARRAY(nameof(GMOD_QAngle)), RecvPropQAngles(FIELD.OF_ARRAYINDEX(nameof(GMOD_QAngle), 0))),
		RecvPropArray3(FIELD.OF_ARRAY(nameof(GMOD_EHANDLE)), RecvPropEHandle(FIELD.OF_ARRAYINDEX(nameof(GMOD_EHANDLE), 0))),
		RecvPropString(FIELD.OF(nameof(GMOD_String0))),
		RecvPropString(FIELD.OF(nameof(GMOD_String1))),
		RecvPropString(FIELD.OF(nameof(GMOD_String2))),
		RecvPropString(FIELD.OF(nameof(GMOD_String3))),

		// Creation IDs
		RecvPropInt(FIELD.OF(nameof(CreationID))),
		RecvPropInt(FIELD.OF(nameof(MapCreatedID))),
	]);

	private static void RecvProxy_OverrideMaterial(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
		Warning("RecvProxy_OverrideMaterial not implemented yet\n");
	}

	private static void RecvProxy_MoveCollide(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
		Warning("RecvProxy_MoveCollide not implemented yet\n");
	}

	private static void RecvProxy_MoveType(ref readonly RecvProxyData data, object instance, IFieldAccessor field) {
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
	public float Friction;

	public CollisionProperty Collision = new();

	public int Effects;
	public byte RenderMode;
	public byte RenderFX;
	public byte RenderFXBlend;
	public Color ColorRender;
	public int CollisionGroup;
	public float Elasticity;
	public float ShadowCastDistance;
	public byte OldRenderMode;

	public InlineArray32<bool> GMOD_bool;
	public InlineArray32<float> GMOD_float;
	public InlineArray32<int> GMOD_int;
	public InlineArray32<Vector3> GMOD_Vector;
	public InlineArray32<QAngle> GMOD_QAngle;
	public InlineArrayNew32<EHANDLE> GMOD_EHANDLE = new();
	public InlineArray512<char> GMOD_String0;
	public InlineArray512<char> GMOD_String1;
	public InlineArray512<char> GMOD_String2;
	public InlineArray512<char> GMOD_String3;

	public readonly GModTable GMOD_DataTable = new();

	public double Speed;
	public int TeamNum;

	public readonly EHANDLE OwnerEntity = new();
	public readonly EHANDLE EffectEntity = new();
	public readonly EHANDLE GroundEntity = new();
	public int LifeState;
	public Vector3 BaseVelocity;
	public int NextThinkTick;
	public byte WaterLevel;

	public long CreationTick;

	public bool OldShouldDraw;

	public Vector3 AbsOrigin;
	public QAngle AbsRotation;
	public Vector3 ViewOffset;
	public Vector3 OldOrigin;
	public QAngle OldRotation;
	public Vector3 Origin;
	public QAngle Rotation;
	public Vector3 NetworkOrigin;
	public QAngle NetworkAngles;

	public readonly Handle<C_BasePlayer> PlayerSimulationOwner = new();
	public int DataChangeEventRef;

	public void ClientThink() {
		throw new NotImplementedException();
	}

	public int DrawModel(int flags) {
		throw new NotImplementedException();
	}

	public ref readonly Vector3 GetLocalOrigin() => ref Origin;
	public ref readonly QAngle GetLocalAngles() => ref Rotation;
	public ref readonly Vector3 GetAbsOrigin() {
		CalcAbsolutePosition();
		return ref AbsOrigin;
	}
	public ref readonly QAngle GetAbsAngles() {
		CalcAbsolutePosition();
		return ref AbsRotation;
	}
	public ref readonly Vector3 GetViewOffset() => ref ViewOffset;

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
		Index = entNum;
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
		// throw new NotImplementedException();
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

	public Vector3 GetNetworkOrigin() => Origin;
	public QAngle GetNetworkAngles() => NetworkAngles;

	static bool s_AbsRecomputionEnabled = true;

	EFL eflags;
	public Matrix4x4 CoordinateFrame;

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public void AddEFlags(EFL flags) => eflags |= flags;
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public void RemoveEFlags(EFL flags) => eflags &= ~flags;

	private void CalcAbsolutePosition() {
		if (!s_AbsRecomputionEnabled)
			return;

		if ((eflags & EFL.DirtyAbsTransform) == 0)
			return;

		RemoveEFlags(EFL.DirtyAbsTransform);

		if (!MoveParent.IsValid()) {
			MathLib.AngleMatrix(GetLocalAngles(), GetLocalOrigin(), ref CoordinateFrame);
			AbsOrigin = GetLocalOrigin();
			AbsRotation = GetLocalAngles();
			MathLib.NormalizeAngles(ref AbsRotation);
			return;
		}

		if (IsEffectActive(EntityEffects.BoneMerge)) {
			MoveToAimEnt();
			return;
		}

		// todo
	}

	public void MoveToAimEnt() {
		throw new NotImplementedException();
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
		OldRotation = GetNetworkAngles();

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
