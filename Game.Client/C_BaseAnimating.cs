using Source;
using Source.Common;
using Source.Common.Engine;

using System.Numerics;
using System.Reflection;

namespace Game.Client;

public partial class C_BaseAnimating : C_BaseEntity, IModelLoadCallback
{
	public static readonly RecvTable DT_ServerAnimationData = new([

	]);
	public static readonly new ClientClass CC_ServerAnimationData = new ClientClass("ServerAnimationData", null, null, DT_ServerAnimationData);
	public static readonly RecvTable DT_BaseAnimating = new(DT_BaseEntity, [
		RecvPropInt(FIELDOF(nameof(Sequence)), 0, RecvProxy_Sequence),
		RecvPropInt(FIELDOF(nameof(ForceBone))),
		RecvPropVector(FIELDOF(nameof(Force))),
		RecvPropInt(FIELDOF(nameof(Skin))),
		RecvPropInt(FIELDOF(nameof(Body))),
		RecvPropInt(FIELDOF(nameof(HitboxSet))),

		RecvPropFloat(FIELDOF(nameof(ModelScale))),

		RecvPropArray3(FIELDOF_ARRAY(nameof(PoseParameter)), RecvPropFloat(null!) ),

		RecvPropFloat(FIELDOF(nameof(PlaybackRate))),

		RecvPropArray3( FIELDOF_ARRAY(nameof(EncodedController)), RecvPropFloat(null!)),

		RecvPropInt( FIELDOF(nameof(ClientSideAnimation ))),
		RecvPropInt( FIELDOF(nameof(ClientSideFrameReset ))),

		RecvPropInt( FIELDOF(nameof( NewSequenceParity ))),
		RecvPropInt( FIELDOF(nameof( ResetEventsParity) )),
		RecvPropInt( FIELDOF(nameof( MuzzleFlashParity )) ),

		RecvPropEHandle(FIELDOF(nameof(LightingOrigin))),
		RecvPropEHandle(FIELDOF(nameof(LightingOriginRelative))),

		RecvPropDataTable( "serveranimdata", DT_ServerAnimationData),

		RecvPropFloat( FIELDOF(nameof( FadeMinDist ) )),
		RecvPropFloat( FIELDOF(nameof( FadeMaxDist ) )),
		RecvPropFloat( FIELDOF(nameof( FadeScale ) )),
	]);

	private static void RecvProxy_Sequence(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		throw new NotImplementedException();
	}

	public static readonly new ClientClass ClientClass = new ClientClass("BaseAnimating", null, null, DT_BaseAnimating);

	public void OnModelLoadComplete(Model model) {
		throw new NotImplementedException();
	}
	public override void PostDataUpdate(DataUpdateType updateType) {
		base.PostDataUpdate(updateType);
	}

	public int Sequence;
	public int ForceBone;
	public Vector3 Force;
	public int Skin;
	public int Body;
	public int HitboxSet;
	public float ModelScale;
	public float PlaybackRate;
	public bool ClientSideAnimation;
	public bool ClientSideFrameReset;
	public int NewSequenceParity;
	public int ResetEventsParity;
	public byte MuzzleFlashParity;
	public EHANDLE LightingOrigin;
	public EHANDLE LightingOriginRelative;
	public float FadeMinDist;
	public float FadeMaxDist;
	public float FadeScale;

	public InlineArrayMaxStudioPoseParam<float> PoseParameter;
	public InlineArrayMaxStudioPoseParam<float> OldPoseParameters;
	public float PrevEventCycle;
	public int EventSequence;
	public InlineArrayMaxStudioBoneCtrls<float> EncodedController;
	public InlineArrayMaxStudioBoneCtrls<float> OldEncodedController;
}
