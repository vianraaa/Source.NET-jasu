using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Game.Shared.HL2;

public struct LadderMove()
{
	public bool ForceLadderMove;
	public bool ForceMount;
	public float StartTime;
	public float ArrivalTime;
	public Vector3 GoalPosition;
	public Vector3 StartPosition;
	public readonly EHANDLE ForceLadder = new();
	public readonly EHANDLE ReservedSpot = new();
}