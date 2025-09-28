using Game.Shared;

using Source.Common;

namespace Game.Server;
using FIELD = Source.FIELD<Game.Server.BaseCombatWeapon>;
public partial class BaseCombatWeapon : BaseAnimating
{
	public static readonly SendTable DT_BaseCombatWeapon = new(DT_BaseAnimating, [

	]);
	public static readonly new ServerClass ServerClass = new ServerClass("BaseCombatWeapon", DT_BaseCombatWeapon).WithManualClassID(StaticClassIndices.CBaseCombatWeapon);
}
