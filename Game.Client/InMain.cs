using Source;
using Source.Common.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client;
[EngineComponent]
public class Input
{
	[ConCommand(name: "+moveup")] void IN_UpDown() { }
	[ConCommand(name: "-moveup")] void IN_UpUp() { }
	[ConCommand(name: "+movedown")] void IN_DownDown() { }
	[ConCommand(name: "-movedown")] void IN_DownUp() { }
	[ConCommand(name: "+left")] void IN_LeftDown() { }
	[ConCommand(name: "-left")] void IN_LeftUp() { }
	[ConCommand(name: "+right")] void IN_RightDown() { }
	[ConCommand(name: "-right")] void IN_RightUp() { }
	[ConCommand(name: "+forward")] void IN_ForwardDown() { }
	[ConCommand(name: "-forward")] void IN_ForwardUp() { }
	[ConCommand(name: "+back")] void IN_BackDown() { }
	[ConCommand(name: "-back")] void IN_BackUp() { }
	[ConCommand(name: "+lookup")] void IN_LookupDown() { }
	[ConCommand(name: "-lookup")] void IN_LookupUp() { }
	[ConCommand(name: "+lookdown")] void IN_LookdownDown() { }
	[ConCommand(name: "-lookdown")] void IN_LookdownUp() { }
	[ConCommand(name: "+strafe")] void IN_StrafeDown() { }
	[ConCommand(name: "-strafe")] void IN_StrafeUp() { }
	[ConCommand(name: "+moveleft")] void IN_MoveleftDown() { }
	[ConCommand(name: "-moveleft")] void IN_MoveleftUp() { }
	[ConCommand(name: "+moveright")] void IN_MoverightDown() { }
	[ConCommand(name: "-moveright")] void IN_MoverightUp() { }
	[ConCommand(name: "+speed")] void IN_SpeedDown() { }
	[ConCommand(name: "-speed")] void IN_SpeedUp() { }
	[ConCommand(name: "+walk")] void IN_WalkDown() { }
	[ConCommand(name: "-walk")] void IN_WalkUp() { }
	[ConCommand(name: "+attack")] void IN_AttackDown() { }
	[ConCommand(name: "-attack")] void IN_AttackUp() { }
	[ConCommand(name: "+attack2")] void IN_Attack2Down() { }
	[ConCommand(name: "-attack2")] void IN_Attack2Up() { }
	[ConCommand(name: "+use")] void IN_UseDown() { }
	[ConCommand(name: "-use")] void IN_UseUp() { }
	[ConCommand(name: "+jump")] void IN_JumpDown() { }
	[ConCommand(name: "-jump")] void IN_JumpUp() { }
	[ConCommand(name: "impulse")] void IN_Impulse() { }
	[ConCommand(name: "+klook")] void IN_KLookDown() { }
	[ConCommand(name: "-klook")] void IN_KLookUp() { }
	[ConCommand(name: "+jlook")] void IN_JLookDown() { }
	[ConCommand(name: "-jlook")] void IN_JLookUp() { }
	[ConCommand(name: "+duck")] void IN_DuckDown() { }
	[ConCommand(name: "-duck")] void IN_DuckUp() { }
	[ConCommand(name: "+reload")] void IN_ReloadDown() { }
	[ConCommand(name: "-reload")] void IN_ReloadUp() { }
	[ConCommand(name: "+alt1")] void IN_Alt1Down() { }
	[ConCommand(name: "-alt1")] void IN_Alt1Up() { }
	[ConCommand(name: "+alt2")] void IN_Alt2Down() { }
	[ConCommand(name: "-alt2")] void IN_Alt2Up() { }
	[ConCommand(name: "+graph")] void IN_GraphDown() { }
	[ConCommand(name: "-graph")] void IN_GraphUp() { }
	[ConCommand(name: "+break")] void IN_BreakDown() { }
	[ConCommand(name: "-break")] void IN_BreakUp() { }
	[ConCommand(name: "+zoom")] void IN_ZoomDown() { }
	[ConCommand(name: "-zoom")] void IN_ZoomUp() { }
	[ConCommand(name: "+attack3")] void IN_Attack3Down() { }
	[ConCommand(name: "-attack3")] void IN_Attack3Up() { }

}
