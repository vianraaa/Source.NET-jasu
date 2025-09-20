using Source.Common;

namespace Game.Server;

public class BaseAnimatingOverlay : BaseAnimating {
	public static readonly SendTable DT_BaseAnimatingOverlay = new(DT_BaseAnimating, [

	]);
	public static readonly new ServerClass ServerClass = new ServerClass("BaseAnimatingOverlay", DT_BaseAnimatingOverlay);
}