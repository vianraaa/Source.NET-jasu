using Source;
using Source.Common.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Shared;


[EngineComponent]
public class MoveVarsShared
{
	public const string DEFAULT_GRAVITY_STRING = "600";

	public ConVar sv_gravity = new(DEFAULT_GRAVITY_STRING, FCvar.Notify | FCvar.Replicated, "World gravity.");
	public ConVar sv_stopspeed = new("100", FCvar.Notify | FCvar.Replicated, "Minimum stopping speed when on ground.");

	public ConVar sv_maxspeed = new("320", FCvar.Notify | FCvar.Replicated);
	public ConVar sv_accelerate = new("10", FCvar.Notify | FCvar.Replicated);

	public ConVar sv_airaccelerate = new("10", FCvar.Notify | FCvar.Replicated);
	public ConVar sv_wateraccelerate = new("10", FCvar.Notify | FCvar.Replicated);
	public ConVar sv_waterfriction = new("1", FCvar.Notify | FCvar.Replicated);
	public ConVar sv_footsteps = new("1", FCvar.Notify | FCvar.Replicated, "Play footstep sound for players");
	public ConVar sv_rollspeed = new("200", FCvar.Notify | FCvar.Replicated);
	public ConVar sv_rollangle = new("0", FCvar.Notify | FCvar.Replicated, "Max view roll angle");

	public ConVar sv_bounce = new("0", FCvar.Notify | FCvar.Replicated, "Bounce multiplier for when physically simulated objects collide with other objects." );
	public ConVar sv_maxvelocity = new("3500", FCvar.Replicated, "Maximum speed any ballistically moving object is allowed to attain per axis." );
	public ConVar sv_stepsize = new("18", FCvar.Notify | FCvar.Replicated );
	public ConVar sv_backspeed = new("0.6", FCvar.Archive | FCvar.Replicated, "How much to slow down backwards motion" );
	public ConVar sv_waterdist = new("12", FCvar.Replicated, "Vertical view fixup when eyes are near water plane." );

	public ConVar sv_skyname = new("sky_urb01", FCvar.Archive | FCvar.Replicated, "Current name of the skybox texture" );

	// Vehicle convars
	public ConVar r_VehicleViewDampen = new( "r_VehicleViewDampen", "1", FCvar.Cheat | FCvar.Notify | FCvar.Replicated );

	// Jeep convars
	public ConVar r_JeepViewDampenFreq = new("7.0", FCvar.Cheat | FCvar.Notify | FCvar.Replicated );
	public ConVar r_JeepViewDampenDamp = new( "1.0", FCvar.Cheat | FCvar.Notify | FCvar.Replicated);
	public ConVar r_JeepViewZHeight = new("10.0", FCvar.Cheat | FCvar.Notify | FCvar.Replicated );

	// Airboat convars
	public ConVar r_AirboatViewDampenFreq = new("7.0", FCvar.Cheat | FCvar.Notify | FCvar.Replicated );
	public ConVar r_AirboatViewDampenDamp = new("1.0", FCvar.Cheat | FCvar.Notify | FCvar.Replicated);
	public ConVar r_AirboatViewZHeight = new( "0.0", FCvar.Cheat | FCvar.Notify | FCvar.Replicated );
}
