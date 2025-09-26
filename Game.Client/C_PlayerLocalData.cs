using Source.Common;
using Source.Common.Mathematics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FIELD = Source.FIELD<Game.Client.C_PlayerLocalData>;
namespace Game.Client;

public class C_PlayerLocalData
{
	public static readonly RecvTable DT_Local = new([
		RecvPropInt(FIELD.OF(nameof(HideHUD))),
	]); public static readonly ClientClass CC_Local = new ClientClass("Local", null, null, DT_Local);

	public int HideHUD;
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
}
