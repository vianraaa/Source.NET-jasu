using Source;
using Source.Common;

using System.Numerics;

namespace Game.Server;

public class BaseAnimating : BaseEntity
{
	public const int ANIMATION_SEQUENCE_BITS = 12;
	public const int ANIMATION_SKIN_BITS = 10;
	public const int ANIMATION_BODY_BITS = 32;
	public const int ANIMATION_HITBOXSET_BITS = 2;
	public const int ANIMATION_POSEPARAMETER_BITS = 11;
	public const int ANIMATION_PLAYBACKRATE_BITS = 8;

	public static readonly SendTable DT_ServerAnimationData = new([
		SendPropFloat(FIELDOF(nameof(Cycle)), ANIMATION_CYCLE_BITS, PropFlags.ChangesOften|PropFlags.RoundDown, 0.0f, 1.0f)
	]);
	public static readonly ServerClass CC_ServerAnimationData = new ServerClass("ServerAnimationData", DT_ServerAnimationData);
	public static readonly SendTable DT_BaseAnimating = new(DT_BaseEntity, [
		SendPropInt( FIELDOF(nameof(ForceBone)), 8, 0 ),
		SendPropVector( FIELDOF(nameof(Force)), 0, PropFlags.NoScale ),

		SendPropInt( FIELDOF(nameof(Skin)), ANIMATION_SKIN_BITS),
		SendPropInt( FIELDOF(nameof(Body)), ANIMATION_BODY_BITS),

		SendPropInt( FIELDOF(nameof(HitboxSet)),ANIMATION_HITBOXSET_BITS, PropFlags.Unsigned ),

		SendPropFloat( FIELDOF(nameof(ModelScale)) ),

		SendPropArray3( FIELDOF_ARRAY(nameof(PoseParameter)), SendPropFloat(null!, ANIMATION_POSEPARAMETER_BITS, 0, 0.0f, 1.0f ) ),

		SendPropInt( FIELDOF(nameof(Sequence)), ANIMATION_SEQUENCE_BITS, PropFlags.Unsigned ),
		SendPropFloat( FIELDOF(nameof(PlaybackRate)), ANIMATION_PLAYBACKRATE_BITS, PropFlags.RoundUp, -4.0f, 12.0f ),

		SendPropArray3(FIELDOF_ARRAY(nameof(EncodedController)), SendPropFloat(null!, 11, PropFlags.RoundDown, 0.0f, 1.0f ) ),

		SendPropInt( FIELDOF(nameof( ClientSideAnimation )), 1, PropFlags.Unsigned ),
		SendPropInt( FIELDOF(nameof( ClientSideFrameReset )), 1, PropFlags.Unsigned ),

		SendPropInt( FIELDOF(nameof( NewSequenceParity) ), (int)EntityEffects.ParityBits, PropFlags.Unsigned ),
		SendPropInt( FIELDOF(nameof( ResetEventsParity )), (int)EntityEffects.ParityBits, PropFlags.Unsigned ),
		SendPropInt( FIELDOF(nameof( MuzzleFlashParity )), (int)EntityEffects.MuzzleflashBits, PropFlags.Unsigned ),

		SendPropEHandle( FIELDOF(nameof( LightingOrigin )) ),
		SendPropEHandle( FIELDOF(nameof( LightingOriginRelative )) ),

		SendPropDataTable( "serveranimdata", DT_ServerAnimationData, SendProxy_ClientSideAnimation ),

		SendPropFloat( FIELDOF(nameof(FadeMinDist) ), 0, PropFlags.NoScale ),
		SendPropFloat( FIELDOF(nameof(FadeMaxDist )), 0, PropFlags.NoScale ),
		SendPropFloat( FIELDOF(nameof(FadeScale )), 0, PropFlags.NoScale ),

		// Gmod specific
		SendPropEHandle(FIELDOF(nameof(BoneManipulator))),
		SendPropEHandle(FIELDOF(nameof(FlexManipulator))),
		SendPropVector(FIELDOF(nameof(OverrideViewTarget)), 0, PropFlags.NoScale),
	]);
	public static readonly new ServerClass ServerClass = new ServerClass("BaseAnimating", DT_BaseAnimating);

	public int ForceBone;
	public Vector3 Force;
	public int Skin;
	public int Body;
	public int HitboxSet;

	public Vector3 ModelScale;
	public InlineArrayMaxStudioPoseParam<float> PoseParameter;
	public InlineArrayMaxStudioPoseParam<float> OldPoseParameters;
	public float PrevEventCycle;
	public int EventSequence;
	public InlineArrayMaxStudioBoneCtrls<float> EncodedController;
	public InlineArrayMaxStudioBoneCtrls<float> OldEncodedController;
	public int Sequence;
	public float PlaybackRate;
	public bool ClientSideAnimation;
	public bool ClientSideFrameReset;
	public int NewSequenceParity;
	public int ResetEventsParity;
	public int MuzzleFlashParity;
	public readonly EHANDLE LightingOrigin = new();
	public readonly EHANDLE LightingOriginRelative = new();
	public readonly EHANDLE BoneManipulator = new();
	public readonly EHANDLE FlexManipulator = new();
	public float FadeMinDist;
	public float FadeMaxDist;
	public float FadeScale;
	public int Cycle;
	public Vector3 OverrideViewTarget;
}
