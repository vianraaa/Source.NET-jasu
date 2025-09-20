using Source.Common;

namespace Game.Server;
public partial class BasePlayer : BaseCombatCharacter {
	public static readonly SendTable DT_BasePlayer = new([

	]);
	public static readonly ServerClass ServerClass = new ServerClass("BasePlayer", DT_BasePlayer);
}