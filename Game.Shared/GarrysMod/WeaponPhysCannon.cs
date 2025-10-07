#if CLIENT_DLL || GAME_DLL
using Source.Common;

using System.Numerics;
namespace Game.Shared.GarrysMod;
using FIELD = Source.FIELD<WeaponPhysCannon>;
public class WeaponPhysCannon : BaseHL2MPCombatWeapon
{
	public static readonly
#if CLIENT_DLL
		RecvTable
#else
		SendTable
#endif
		DT_WeaponPhysCannon = new(DT_BaseHL2MPCombatWeapon, [
#if CLIENT_DLL
			RecvPropBool(FIELD.OF(nameof(Active))),
			RecvPropEHandle(FIELD.OF(nameof(AttachedObject))),
			RecvPropVector(FIELD.OF(nameof(AttachedPositionObjectSpace))),
			RecvPropFloat(FIELD.OF_VECTORELEM(nameof(AttachedAnglesPlayerSpace), 0)),
			RecvPropFloat(FIELD.OF_VECTORELEM(nameof(AttachedAnglesPlayerSpace), 1)),
			RecvPropFloat(FIELD.OF_VECTORELEM(nameof(AttachedAnglesPlayerSpace), 2)),
			RecvPropInt(FIELD.OF(nameof(EffectState))),
			RecvPropBool(FIELD.OF(nameof(Open))),
#else
			SendPropBool(FIELD.OF(nameof(Active))),
			SendPropEHandle(FIELD.OF(nameof(AttachedObject))),
			SendPropVector(FIELD.OF(nameof(AttachedPositionObjectSpace)), -1, PropFlags.Coord),
			SendPropFloat(FIELD.OF_VECTORELEM(nameof(AttachedAnglesPlayerSpace), 0), 11),
			SendPropFloat(FIELD.OF_VECTORELEM(nameof(AttachedAnglesPlayerSpace), 1), 11),
			SendPropFloat(FIELD.OF_VECTORELEM(nameof(AttachedAnglesPlayerSpace), 2), 11),
			SendPropInt(FIELD.OF(nameof(EffectState))),
			SendPropBool(FIELD.OF(nameof(Open))),
#endif
		]);
#if CLIENT_DLL
	public static readonly new ClientClass ClientClass = new ClientClass("WeaponPhysCannon", null, null, DT_WeaponPhysCannon).WithManualClassID(StaticClassIndices.CWeaponPhysCannon);
#else
	public static readonly new ServerClass ServerClass = new ServerClass("WeaponPhysCannon", DT_WeaponPhysCannon).WithManualClassID(StaticClassIndices.CWeaponPhysCannon);
#endif
	public bool Active;
	public readonly EHANDLE AttachedObject = new();
	public Vector3 AttachedPositionObjectSpace;
	public Vector3 AttachedAnglesPlayerSpace;
	public int EffectState;
	public bool Open;
	public bool PhyscannonState;
}
#endif
