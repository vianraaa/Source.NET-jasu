using Game.Client.GarrysMod;
using Game.Shared;

using Source.Common;

namespace Game.Client;
public partial class C_BaseCombatCharacter : C_BaseFlex
{
	public static readonly RecvTable DT_BCCLocalPlayerExclusive = new(DT_BaseFlex, [
		RecvPropTime(FIELDOF(nameof(NextAttack))),
	]);
	public static readonly ClientClass CC_BCCLocalPlayerExclusive = new ClientClass("BCCLocalPlayerExclusive", null, null, DT_BCCLocalPlayerExclusive);

	public static readonly RecvTable DT_BaseCombatCharacter = new(DT_BaseFlex, [
		RecvPropDataTable( "bcc_localdata", DT_BCCLocalPlayerExclusive ),
		RecvPropEHandle(FIELDOF(nameof(ActiveWeapon))),
		RecvPropArray3(FIELDOF_ARRAY(nameof(MyWeapons)), RecvPropEHandle( FIELDOF_ARRAY(nameof(MyWeapons)))),
	]);
	public static readonly new ClientClass ClientClass = new ClientClass("BaseCombatCharacter", null, null, DT_BaseCombatCharacter).WithManualClassID(StaticClassIndices.CBaseCombatCharacter);

	public double NextAttack;
	public readonly EHANDLE ActiveWeapon = new();
	public InlineArrayNewMaxWeapons<EHANDLE> MyWeapons;
}
