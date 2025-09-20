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

namespace Game.Client;
public partial class C_BaseEntity : IClientEntity
{
	public static IClientNetworkable CreateObject(int entNum, int serialNum) {
		C_BaseEntity ret = new C_BaseEntity();
		ret.Init(entNum, serialNum);
		return ret;
	}

	private static void RecvProxy_AnimTime(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		throw new NotImplementedException();
	}

	private static void RecvProxy_EffectFlags(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		throw new NotImplementedException();
	}


	public static RecvTable DT_AnimTimeMustBeFirst = new(nameof(DT_AnimTimeMustBeFirst), [
		RecvPropInt(FIELDOF(nameof(AnimTime)), 0, RecvProxy_AnimTime),
	]);
	public static readonly ClientClass CC_AnimTimeMustBeFirst = new ClientClass("AnimTimeMustBeFirst", CreateObject, null, DT_AnimTimeMustBeFirst);


	public static RecvTable DT_PredictableId = new(nameof(DT_PredictableId), [

	]);
	public static readonly ClientClass CC_PredictableId = new ClientClass("PredictableId", CreateObject, null, DT_PredictableId);

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

		RecvPropDataTable(nameof(Collision), FIELDOF(nameof(Collision)), CollisionProperty.DT_CollisionProperty, 0, RECV_GET_OBJECT_AT_FIELD(FIELDOF(nameof(Collision))))
	]);

	public static readonly ClientClass CC_BaseEntity = new ClientClass("BaseEntity", CreateObject, null, DT_BaseEntity)
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

	public int Health;
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
