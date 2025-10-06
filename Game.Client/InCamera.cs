using Source.Common.Commands;

namespace Game.Client;

public partial class Input
{
	ConVar cam_command = new("0", FCvar.Cheat);   
	ConVar cam_snapto = new("0", FCvar.Archive | FCvar.Cheat);  
	ConVar cam_ideallag = new("4.0", FCvar.Archive| FCvar.Cheat, "Amount of lag used when matching offset to ideal angles in thirdperson view" );
	ConVar cam_idealdelta = new("4.0", FCvar.Archive| FCvar.Cheat, "Controls the speed when matching offset to ideal angles in thirdperson view" );
	ConVar cam_idealyaw = new("0", FCvar.Archive| FCvar.Cheat);        
	ConVar cam_idealpitch = new("0", FCvar.Archive | FCvar.Cheat  );   
	ConVar cam_idealdist = new("150", FCvar.Archive | FCvar.Cheat );   
	ConVar cam_idealdistright = new("0", FCvar.Archive | FCvar.Cheat );
	ConVar cam_idealdistup = new("0", FCvar.Archive | FCvar.Cheat );   
	ConVar cam_collision = new("1", FCvar.Archive | FCvar.Cheat, "When in thirdperson and cam_collision is set to 1, an attempt is made to keep the camera from passing though walls." );
	ConVar cam_showangles = new("0", FCvar.Cheat, "When in thirdperson, print viewangles/idealangles/cameraoffsets to the console." );
	ConVar c_maxpitch = new("90", FCvar.Archive| FCvar.Cheat );
	ConVar c_minpitch = new("0", FCvar.Archive| FCvar.Cheat );
	ConVar c_maxyaw = new("135", FCvar.Archive | FCvar.Cheat);
	ConVar c_minyaw = new("-135", FCvar.Archive| FCvar.Cheat );
	ConVar c_maxdistance = new("200", FCvar.Archive| FCvar.Cheat );
	ConVar c_mindistance = new("30", FCvar.Archive| FCvar.Cheat );
	ConVar c_orthowidth = new("100", FCvar.Archive| FCvar.Cheat );
	ConVar c_orthoheight = new("100", FCvar.Archive | FCvar.Cheat );

	public float CAM_CapPitch(float val) => val;
	public float CAM_CapYaw(float val) => val;
}
