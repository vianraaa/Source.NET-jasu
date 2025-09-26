using Game.Shared;

using Source;
using Source.Common;
using Source.Common.Mathematics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server;

using FIELD = FIELD<PlayerLocalData>;

public class PlayerLocalData
{
	public static readonly SendTable DT_Local = new([
		SendPropArray3(FIELD.OF_ARRAY(nameof(AreaBits)), SendPropInt(FIELD.OF_ARRAYINDEX(nameof(AreaBits)), 8, PropFlags.Unsigned)),
		SendPropArray3(FIELD.OF_ARRAY(nameof(AreaPortalBits)), SendPropInt(FIELD.OF_ARRAYINDEX(nameof(AreaPortalBits)), 8, PropFlags.Unsigned)),
		SendPropInt(FIELD.OF(nameof(HideHUD)), (int)HideHudBits.BitCount, PropFlags.Unsigned),
		SendPropFloat(FIELD.OF(nameof(FOVRate)), 0, PropFlags.NoScale ),
		SendPropInt(FIELD.OF(nameof(Ducked)),   1, PropFlags.Unsigned ),
		SendPropInt(FIELD.OF(nameof(Ducking)),  1, PropFlags.Unsigned ),
		SendPropInt(FIELD.OF(nameof(InDuckJump)),   1, PropFlags.Unsigned ),
		SendPropFloat(FIELD.OF(nameof(DuckTime)), 12, PropFlags.RoundDown|PropFlags.ChangesOften, 0.0f, 2048.0f ),
		SendPropFloat(FIELD.OF(nameof(DuckJumpTime)), 12, PropFlags.RoundDown, 0.0f, 2048.0f ),
		SendPropFloat(FIELD.OF(nameof(JumpTime)), 12, PropFlags.RoundDown, 0.0f, 2048.0f ),
		SendPropFloat(FIELD.OF(nameof(FallVelocity)), 17, PropFlags.ChangesOften, -4096.0f, 4096.0f ),
		SendPropVector(FIELD.OF(nameof(PunchAngle)),      -1,  PropFlags.Coord|PropFlags.ChangesOften),
		SendPropVector(FIELD.OF(nameof(PunchAngleVel)),      -1,  PropFlags.Coord),
		SendPropInt(FIELD.OF(nameof(DrawViewmodel)), 1, PropFlags.Unsigned ),
		SendPropInt(FIELD.OF(nameof(WearingSuit)), 1, PropFlags.Unsigned ),
		SendPropBool(FIELD.OF(nameof(Poisoned))),
		SendPropFloat(FIELD.OF(nameof(StepSize)), 16, PropFlags.RoundUp, 0.0f, 128.0f ),
		SendPropInt(FIELD.OF(nameof(AllowAutoMovement)),1, PropFlags.Unsigned ),

		SendPropInt(FIELD.OF("Skybox3D.Scale"), 12),
		SendPropVector(FIELD.OF("Skybox3D.Origin"), -1, PropFlags.Coord),
		SendPropInt(FIELD.OF("Skybox3D.Area"), 8, PropFlags.Unsigned),
		SendPropInt(FIELD.OF("Skybox3D.Fog.Enable"), 1, PropFlags.Unsigned),
		SendPropInt(FIELD.OF("Skybox3D.Fog.Blend"), 1, PropFlags.Unsigned),
		SendPropVector(FIELD.OF("Skybox3D.Fog.dirPrimary"), -1, PropFlags.Coord),
		SendPropInt(FIELD.OF("Skybox3D.Fog.ColorPrimary"), 32, PropFlags.Unsigned),
		SendPropInt(FIELD.OF("Skybox3D.Fog.ColorSecondary"), 32, PropFlags.Unsigned),
		SendPropFloat(FIELD.OF("Skybox3D.Fog.Start"), 0, PropFlags.NoScale),
		SendPropFloat(FIELD.OF("Skybox3D.Fog.End"), 0, PropFlags.NoScale),
		SendPropFloat(FIELD.OF("Skybox3D.Fog.MaxDensity"), 0, PropFlags.NoScale),

		SendPropEHandle( FIELD.OF("PlayerFog.Ctrl") ),

		SendPropVector( FIELD.OF_ARRAYINDEX("Audio.LocalSound", 0), -1, PropFlags.Coord),
		SendPropVector( FIELD.OF_ARRAYINDEX("Audio.LocalSound", 1), -1, PropFlags.Coord),
		SendPropVector( FIELD.OF_ARRAYINDEX("Audio.LocalSound", 2), -1, PropFlags.Coord),
		SendPropVector( FIELD.OF_ARRAYINDEX("Audio.LocalSound", 3), -1, PropFlags.Coord),
		SendPropVector( FIELD.OF_ARRAYINDEX("Audio.LocalSound", 4), -1, PropFlags.Coord),
		SendPropVector( FIELD.OF_ARRAYINDEX("Audio.LocalSound", 5), -1, PropFlags.Coord),
		SendPropVector( FIELD.OF_ARRAYINDEX("Audio.LocalSound", 6), -1, PropFlags.Coord),
		SendPropVector( FIELD.OF_ARRAYINDEX("Audio.LocalSound", 7), -1, PropFlags.Coord),
		SendPropInt( FIELD.OF("Audio.SoundscapeIndex"), 17, 0 ),
		SendPropInt( FIELD.OF("Audio.LocalBits"), NUM_AUDIO_LOCAL_SOUNDS, PropFlags.Unsigned ),
		SendPropEHandle( FIELD.OF("Audio.Ent") ),
	]); public static readonly ServerClass CC_Local = new("Local", DT_Local);


	// TODO: NETWORK VARS!!!!!
	public InlineArrayMaxAreaStateBytes<byte> AreaBits;
	public InlineArrayMaxAreaPortalStateBytes<byte> AreaPortalBits;
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

	public Sky3DParams Skybox3D = new();
	public FogPlayerParams PlayerFog = new();
	public AudioParams Audio = new();
}
