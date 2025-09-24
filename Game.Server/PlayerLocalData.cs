using Game.Shared;

using Source.Common;
using Source.Common.Mathematics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server;



public class PlayerLocalData
{
	public static readonly SendTable DT_Local = new([
		/*SendPropArray3  (FIELDOF_ARRAY(nameof(m_chAreaBits)), SendPropInt(FIELDOF_ARRAY(m_chAreaBits), 8, PropFlags.Unsigned)),
		SendPropArray3  (FIELDOF_ARRAY(nameof(m_chAreaPortalBits)), SendPropInt(FIELDOF_ARRAY(m_chAreaPortalBits), 8, PropFlags.Unsigned)),
		SendPropInt     (FIELDOF(nameof(iHideHUD)), (int)HideHudBits.BitCount, PropFlags.Unsigned),
		SendPropFloat   (FIELDOF(nameof(flFOVRate)), 0, PropFlags.NoScale ),
		SendPropInt     (FIELDOF(nameof(bDucked)),   1, PropFlags.Unsigned ),
		SendPropInt     (FIELDOF(nameof(bDucking)),  1, PropFlags.Unsigned ),
		SendPropInt     (FIELDOF(nameof(bInDuckJump)),   1, PropFlags.Unsigned ),
		SendPropFloat   (FIELDOF(nameof(flDucktime)), 12, PropFlags.RoundDown|PropFlags.ChangesOften, 0.0f, 2048.0f ),
		SendPropFloat   (FIELDOF(nameof(flDuckJumpTime)), 12, PropFlags.RoundDown, 0.0f, 2048.0f ),
		SendPropFloat   (FIELDOF(nameof(flJumpTime)), 12, PropFlags.RoundDown, 0.0f, 2048.0f ),
		SendPropFloat   (FIELDOF(nameof(flFallVelocity)), 17, PropFlags.ChangesOften, -4096.0f, 4096.0f ),
		SendPropVector  (FIELDOF(nameof(vecPunchAngle)),      -1,  PropFlags.Coord|PropFlags.ChangesOften),
		SendPropVector  (FIELDOF(nameof(vecPunchAngleVel)),      -1,  PropFlags.Coord),
		SendPropInt     (FIELDOF(nameof(bDrawViewmodel)), 1, PropFlags.Unsigned ),
		SendPropInt     (FIELDOF(nameof(bWearingSuit)), 1, PropFlags.Unsigned ),
		SendPropBool    (FIELDOF(nameof(bPoisoned))),
		SendPropFloat   (FIELDOF(nameof(flStepSize)), 16, PropFlags.RoundUp, 0.0f, 128.0f ),
		SendPropInt     (FIELDOF(nameof(bAllowAutoMovement)),1, PropFlags.Unsigned ),

		SendPropInt(FIELDOF_STRUCTELEM<PlayerLocalData, int>((i) => ref i.Skybox3d.Scale), 12),
		SendPropVector  (FIELDOF_STRUCTELEM(m_skybox3d.origin),      -1,  PropFlags.Coord),
		SendPropInt (FIELDOF_STRUCTELEM(m_skybox3d.area),  8, PropFlags.Unsigned ),
		SendPropInt( FIELDOF_STRUCTELEM( m_skybox3d.fog.enable ), 1, PropFlags.Unsigned ),
		SendPropInt( FIELDOF_STRUCTELEM( m_skybox3d.fog.blend ), 1, PropFlags.Unsigned ),
		SendPropVector( FIELDOF_STRUCTELEM(m_skybox3d.fog.dirPrimary), -1, PropFlags.Coord),
		SendPropInt( FIELDOF_STRUCTELEM( m_skybox3d.fog.colorPrimary ), 32, PropFlags.Unsigned ),
		SendPropInt( FIELDOF_STRUCTELEM( m_skybox3d.fog.colorSecondary ), 32, PropFlags.Unsigned ),
		SendPropFloat( FIELDOF_STRUCTELEM( m_skybox3d.fog.start ), 0, PropFlags.NoScale ),
		SendPropFloat( FIELDOF_STRUCTELEM( m_skybox3d.fog.end ), 0, PropFlags.NoScale ),
		SendPropFloat( FIELDOF_STRUCTELEM( m_skybox3d.fog.maxdensity ), 0, PropFlags.NoScale ),

		SendPropEHandle( FIELDOF_STRUCTELEM( m_PlayerFog.m_hCtrl ) ),

		SendPropVector( SENDINFO_STRUCTARRAYELEM( m_audio.localSound, 0 ), -1, PropFlags.Coord),
		SendPropVector( SENDINFO_STRUCTARRAYELEM( m_audio.localSound, 1 ), -1, PropFlags.Coord),
		SendPropVector( SENDINFO_STRUCTARRAYELEM( m_audio.localSound, 2 ), -1, PropFlags.Coord),
		SendPropVector( SENDINFO_STRUCTARRAYELEM( m_audio.localSound, 3 ), -1, PropFlags.Coord),
		SendPropVector( SENDINFO_STRUCTARRAYELEM( m_audio.localSound, 4 ), -1, PropFlags.Coord),
		SendPropVector( SENDINFO_STRUCTARRAYELEM( m_audio.localSound, 5 ), -1, PropFlags.Coord),
		SendPropVector( SENDINFO_STRUCTARRAYELEM( m_audio.localSound, 6 ), -1, PropFlags.Coord),
		SendPropVector( SENDINFO_STRUCTARRAYELEM( m_audio.localSound, 7 ), -1, PropFlags.Coord),
		SendPropInt( FIELDOF_STRUCTELEM( m_audio.soundscapeIndex ), 17, 0 ),
		SendPropInt( FIELDOF_STRUCTELEM( m_audio.localBits ), NUM_AUDIO_LOCAL_SOUNDS, PropFlags.Unsigned ),
		SendPropEHandle( FIELDOF_STRUCTELEM( m_audio.ent ) ),*/
	]); public static readonly ServerClass CC_Local = new("Local", DT_Local);


	// TODO: NETWORK VARS!!!!!
	public bool HideHUD;
	public float FOVRate;
	public bool Ducked;
	public bool Ducking;
	public bool InDuckJump;
	public double DuckTime;
	public double DuckJumpTime;
	public double JumpTime;
	public int StepSide;
	public double FallVelocity;
	public int OldButtons;
	public int OldForwardMove;
	public QAngle PunchAngle;
	public QAngle PunchAngleVel;
	public bool DrawViewmodel;
	public bool WearingSuit;
	public bool Poisoned;
	public bool StepSize;
	public bool AllowAutoMovement;
	public bool SlowMovement;

	public Sky3DParams Skybox3d;
	public AudioParams Audio;
}
