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
	public ConVar sv_airaccelerate = new("10", FCvar.Notify | FCvar.Replicated);
	public ConVar sv_wateraccelerate = new("10", FCvar.Notify | FCvar.Replicated);
	public ConVar sv_waterfriction = new("1", FCvar.Notify | FCvar.Replicated);
	public ConVar sv_footsteps = new("1", FCvar.Notify | FCvar.Replicated, "Play footstep sound for players");
	public ConVar sv_rollspeed = new("200", FCvar.Notify | FCvar.Replicated);
	public ConVar sv_rollangle = new("0", FCvar.Notify | FCvar.Replicated, "Max view roll angle");
}
