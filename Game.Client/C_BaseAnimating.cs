using Game.Shared;

using Source;
using Source.Common;
using Source.Common.Engine;

using System.Numerics;
using System.Reflection;

namespace Game.Client;

public partial class C_BaseAnimating : C_BaseEntity, IModelLoadCallback
{
	public static readonly RecvTable DT_ServerAnimationData = new([
		RecvPropFloat(FIELDOF(nameof(Cycle))),
	]);
	public static readonly ClientClass CC_ServerAnimationData = new ClientClass("ServerAnimationData", null, null, DT_ServerAnimationData);
	public static readonly RecvTable DT_BaseAnimating = new(DT_BaseEntity, [
		RecvPropInt( FIELDOF(nameof(ForceBone))),
		RecvPropVector( FIELDOF(nameof(Force))),

		RecvPropInt( FIELDOF(nameof(Skin))),
		RecvPropInt( FIELDOF(nameof(Body))),

		RecvPropInt( FIELDOF(nameof(HitboxSet))),

		RecvPropFloat( FIELDOF(nameof(ModelScale))),

		RecvPropArray3( FIELDOF_ARRAY(nameof(PoseParameter)), RecvPropFloat(null!)),

		RecvPropInt( FIELDOF(nameof(Sequence))),
		RecvPropFloat( FIELDOF(nameof(PlaybackRate))),

		RecvPropArray3(FIELDOF_ARRAY(nameof(EncodedController)), RecvPropFloat(null!) ),

		RecvPropInt( FIELDOF(nameof( ClientSideAnimation ))),
		RecvPropInt( FIELDOF(nameof( ClientSideFrameReset ))),

		RecvPropInt( FIELDOF(nameof( NewSequenceParity) )),
		RecvPropInt( FIELDOF(nameof( ResetEventsParity ))),
		RecvPropInt( FIELDOF(nameof( MuzzleFlashParity ))),

		RecvPropEHandle( FIELDOF(nameof( LightingOrigin )) ),
		RecvPropEHandle( FIELDOF(nameof( LightingOriginRelative )) ),

		RecvPropDataTable( "serveranimdata", DT_ServerAnimationData ),

		RecvPropFloat( FIELDOF(nameof(FadeMinDist) )),
		RecvPropFloat( FIELDOF(nameof(FadeMaxDist ))),
		RecvPropFloat( FIELDOF(nameof(FadeScale ))),

		// Gmod specific
		RecvPropEHandle(FIELDOF(nameof(BoneManipulator))),
		RecvPropEHandle(FIELDOF(nameof(FlexManipulator))),
		RecvPropVector(FIELDOF(nameof(OverrideViewTarget))),
	]);

	private static void RecvProxy_Sequence(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		throw new NotImplementedException();
	}

	public static readonly new ClientClass ClientClass = new ClientClass("BaseAnimating", null, null, DT_BaseAnimating).WithManualClassID(StaticClassIndices.CBaseAnimating);

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
	public readonly EHANDLE LightingOrigin = new();
	public readonly EHANDLE LightingOriginRelative = new();
	public readonly EHANDLE BoneManipulator = new();
	public readonly EHANDLE FlexManipulator = new();
	public Vector3 OverrideViewTarget;
	public float FadeMinDist;
	public float FadeMaxDist;
	public float FadeScale;
	public float Cycle;

	public InlineArrayMaxStudioPoseParam<float> PoseParameter;
	public InlineArrayMaxStudioPoseParam<float> OldPoseParameters;
	public float PrevEventCycle;
	public int EventSequence;
	public InlineArrayMaxStudioBoneCtrls<float> EncodedController;
	public InlineArrayMaxStudioBoneCtrls<float> OldEncodedController;
}
