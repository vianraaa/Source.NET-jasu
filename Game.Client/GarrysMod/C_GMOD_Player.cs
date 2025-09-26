using Game.Client.HL2MP;
using Game.Shared;

using Source;
using Source.Common;

using System.Numerics;

namespace Game.Client.GarrysMod;
using FIELD = FIELD<C_GMOD_Player>;

[LinkEntityToClass(LocalName = "player")]
public class C_GMOD_Player : C_HL2MP_Player
{
	public static readonly RecvTable DT_GMOD_Player = new(DT_HL2MP_Player, [

	]);
	public static readonly new ClientClass ClientClass = new ClientClass("GMOD_Player", null, null, DT_GMOD_Player)
															.WithManualClassID(StaticClassIndices.CGMOD_Player);

	public int GModPlayerFlags;
	public readonly EHANDLE HoveredWidget = new();
	public readonly EHANDLE PressedWidget = new();
	public readonly EHANDLE Driving = new();
	public int DrivingMode;
	public int PlayerClass;
	public bool CanZoom;
	public bool CanWalk;
	public bool IsTyping;
	public float StepSize;
	public float JumpPower;
	public Vector3 ViewOffsetDucked;
	public float GestureEndTime;
	public Vector3 PlayerColor;
	public Vector3 WeaponColor;
	public readonly EHANDLE Hands = new();
	public int MaxArmor;
	public float Gravity;
	public bool SprintEnabled;
}