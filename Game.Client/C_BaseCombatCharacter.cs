using Game.Client.GarrysMod;
using Game.Shared;

using Source.Common;

using FIELD = Source.FIELD<Game.Client.C_BaseCombatCharacter>;

namespace Game.Client;
public partial class C_BaseCombatCharacter : C_BaseFlex
{
	public static readonly RecvTable DT_BCCLocalPlayerExclusive = new([
		RecvPropTime(FIELD.OF(nameof(NextAttack))),
	]);
	public static readonly ClientClass CC_BCCLocalPlayerExclusive = new ClientClass("BCCLocalPlayerExclusive", null, null, DT_BCCLocalPlayerExclusive);

	public static readonly RecvTable DT_BaseCombatCharacter = new(DT_BaseFlex, [
		RecvPropDataTable( "bcc_localdata", DT_BCCLocalPlayerExclusive ),
		RecvPropEHandle(FIELD.OF(nameof(ActiveWeapon))),
		RecvPropArray3(FIELD.OF_ARRAY(nameof(MyWeapons)), RecvPropEHandle( FIELD.OF_ARRAY(nameof(MyWeapons)))),
	]);
	public static readonly new ClientClass ClientClass = new ClientClass("BaseCombatCharacter", null, null, DT_BaseCombatCharacter).WithManualClassID(StaticClassIndices.CBaseCombatCharacter);

	public double NextAttack;
	public readonly EHANDLE ActiveWeapon = new();
	public InlineArrayNewMaxWeapons<EHANDLE> MyWeapons = new();
}
