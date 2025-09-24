using Game.Shared;

using Source.Common;

namespace Game.Server;
public class BaseCombatCharacter : BaseFlex
{
	public static readonly SendTable DT_BaseCombatCharacter = new(DT_BaseFlex, [

	]);
	public static readonly new ServerClass ServerClass = new ServerClass("BaseCombatCharacter", DT_BaseCombatCharacter).WithManualClassID(StaticClassIndices.CBaseCombatCharacter);
}