using Game.Shared;

using Source;
using Source.Common;
using Source.Common.Engine;

using System.Reflection;

namespace Game.Server;
public class BaseCombatCharacter : BaseFlex
{
	public static readonly SendTable DT_BCCLocalPlayerExclusive = new([
		SendPropTime(FIELDOF(nameof(NextAttack))),
	]);
	public static readonly ServerClass CC_BCCLocalPlayerExclusive = new ServerClass("BCCLocalPlayerExclusive", DT_BCCLocalPlayerExclusive);

	public static readonly SendTable DT_BaseCombatCharacter = new(DT_BaseFlex, [
		SendPropDataTable( "bcc_localdata", DT_BCCLocalPlayerExclusive, SendProxy_SendBaseCombatCharacterLocalDataTable ),
		SendPropEHandle(FIELDOF(nameof(ActiveWeapon))),
		SendPropArray3(FIELDOF_ARRAY(nameof(MyWeapons)), SendPropEHandle( FIELDOF_ARRAY(nameof(MyWeapons)))),
	]);

	public double NextAttack;
	public readonly EHANDLE ActiveWeapon = new();
	public InlineArrayNewMaxWeapons<EHANDLE> MyWeapons = new();

	private static object? SendProxy_SendBaseCombatCharacterLocalDataTable(SendProp prop, object instance, FieldInfo data, SendProxyRecipients recipients, int objectID) {
		throw new NotImplementedException();
	}

	public static readonly new ServerClass ServerClass = new ServerClass("BaseCombatCharacter", DT_BaseCombatCharacter).WithManualClassID(StaticClassIndices.CBaseCombatCharacter);
}