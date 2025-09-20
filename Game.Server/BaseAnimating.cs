using Source.Common;

namespace Game.Server;

public class BaseAnimating : BaseEntity {
	public static readonly SendTable DT_BaseAnimating = new(DT_BaseEntity, [

	]);
	public static readonly new ServerClass ServerClass = new ServerClass("BaseAnimating", DT_BaseAnimating);
}
