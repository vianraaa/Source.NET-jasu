using Source.Common;
using Source.Common.Bitbuffers;
using Source.Common.Client;
using Source.Common.Engine;
using Source.Common.Mathematics;

using System.Numerics;

namespace Game.Client;
public partial class C_BaseEntity : IClientEntity
{
	public int Index;

	private Model? Model;

	public double AnimTime;
	public double OldAnimTime;

	public double SimulationTime;
	public double OldSimulationTime;

	public double CreateTime;

	public byte InterpolationFrame;
	public byte OldInterpolationFrame;

	public int Health;
	public double Speed;
	public int TeamNum;

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

	public ClientClass GetClientClass() {
		throw new NotImplementedException();
	}

	public IClientNetworkable GetClientNetworkable() {
		throw new NotImplementedException();
	}

	public Source.Common.IClientRenderable GetClientRenderable() {
		throw new NotImplementedException();
	}

	public IClientThinkable GetClientThinkable() {
		throw new NotImplementedException();
	}

	public IClientEntity GetIClientEntity() {
		throw new NotImplementedException();
	}

	public IClientUnknown GetIClientUnknown() {
		throw new NotImplementedException();
	}

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

	public void Release() {
		throw new NotImplementedException();
	}

	public bool SetupBones(ref Matrix4x4 boneToWOrldOut, int maxBones, int boneMask, double currentTime) {
		throw new NotImplementedException();
	}

	public bool ShouldDraw() {
		throw new NotImplementedException();
	}

	public virtual void Dispose() {
		GC.SuppressFinalize(this);
	}

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

	public void PreDataUpdate(DataUpdateType updateType) {
		throw new NotImplementedException();
	}

	public bool IsDormant() {
		throw new NotImplementedException();
	}

	public int EntIndex() {
		throw new NotImplementedException();
	}

	public void ReceiveMessage(int classID, bf_read msg) {
		throw new NotImplementedException();
	}

	public void SetDestroyedOnRecreateEntities() {

	}
}
