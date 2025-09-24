using Game.Client.GarrysMod;
using Game.Shared;

using Source.Common;

namespace Game.Client;
public partial class C_BaseCombatCharacter : C_BaseFlex
{
	public static readonly RecvTable DT_BaseCombatCharacter = new(DT_BaseFlex, [

	]);
	public static readonly new ClientClass ClientClass = new ClientClass("BaseCombatCharacter", null, null, DT_BaseCombatCharacter).WithManualClassID(StaticClassIndices.CBaseCombatCharacter);
}
