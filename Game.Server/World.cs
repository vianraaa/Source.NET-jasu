using Game.Shared;

using Source.Common;

namespace Game.Server;

[LinkEntityToClass(LocalName = "player")]
public class World : BaseEntity
{
	public static readonly SendTable DT_World = [];
	public static readonly ServerClass ServerClass = new ServerClass("World", DT_World)
																		.WithManualClassID(StaticClassIndices.CWorld);
}
