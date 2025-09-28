using Game.Shared;

using Source;
using Source.Common;
using Source.Common.Engine;

using System.Numerics;
using System.Reflection;

using FIELD = Source.FIELD<Game.Client.C_BaseAnimating>;

namespace Game.Client;

public partial class C_BaseAnimating : C_BaseEntity, IModelLoadCallback
{
	public static readonly RecvTable DT_ServerAnimationData = new([
		RecvPropFloat(FIELD.OF(nameof(Cycle))),
	]);
	public static readonly ClientClass CC_ServerAnimationData = ClientClass.New(DT_ServerAnimationData);
	public static readonly RecvTable DT_BaseAnimating = new(DT_BaseEntity, [
		RecvPropInt( FIELD.OF(nameof(ForceBone))),
		RecvPropVector( FIELD.OF(nameof(Force))),

		RecvPropInt( FIELD.OF(nameof(Skin))),
		RecvPropInt( FIELD.OF(nameof(Body))),

		RecvPropInt( FIELD.OF(nameof(HitboxSet))),

		RecvPropFloat( FIELD.OF(nameof(ModelScale))),

		RecvPropArray3( FIELD.OF_ARRAY(nameof(PoseParameter)), RecvPropFloat(null!)),

		RecvPropInt( FIELD.OF(nameof(Sequence))),
		RecvPropFloat( FIELD.OF(nameof(PlaybackRate))),

		RecvPropArray3(FIELD.OF_ARRAY(nameof(EncodedController)), RecvPropFloat(null!) ),

		RecvPropInt( FIELD.OF(nameof( ClientSideAnimation ))),
		RecvPropInt( FIELD.OF(nameof( ClientSideFrameReset ))),

		RecvPropInt( FIELD.OF(nameof( NewSequenceParity) )),
		RecvPropInt( FIELD.OF(nameof( ResetEventsParity ))),
		RecvPropInt( FIELD.OF(nameof( MuzzleFlashParity ))),

		RecvPropEHandle( FIELD.OF(nameof( LightingOrigin )) ),
		RecvPropEHandle( FIELD.OF(nameof( LightingOriginRelative )) ),

		RecvPropDataTable( "serveranimdata", DT_ServerAnimationData ),

		RecvPropFloat( FIELD.OF(nameof(FadeMinDist) )),
		RecvPropFloat( FIELD.OF(nameof(FadeMaxDist ))),
		RecvPropFloat( FIELD.OF(nameof(FadeScale ))),

		// Gmod specific
		RecvPropEHandle(FIELD.OF(nameof(BoneManipulator))),
		RecvPropEHandle(FIELD.OF(nameof(FlexManipulator))),
		RecvPropVector(FIELD.OF(nameof(OverrideViewTarget))),
	]);

	private static void RecvProxy_Sequence(ref readonly RecvProxyData data, object instance, FieldInfo field) {
		throw new NotImplementedException();
	}

	public static readonly new ClientClass ClientClass = ClientClass.New(DT_BaseAnimating).WithManualClassID(StaticClassIndices.CBaseAnimating).WithAutoEntityCreateFn<C_BaseAnimating>();

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
