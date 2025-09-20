using Source.Common;

namespace Game.Server;

public class BaseFlex : BaseAnimatingOverlay {
	public static readonly SendTable DT_BaseFlex = new(DT_BaseAnimatingOverlay, [

	]);
	public static readonly new ServerClass ServerClass = new ServerClass("BaseFlex", DT_BaseFlex);
}
