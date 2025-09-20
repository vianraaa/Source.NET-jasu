using Source.Common;

namespace Game.Client;

public partial class C_BaseAnimatingOverlay : C_BaseAnimating {
	public static readonly RecvTable DT_BaseAnimatingOverlay = new(DT_BaseAnimating, [

	]);
	public static readonly new ClientClass ClientClass = new ClientClass("BaseAnimatingOverlay", null, null, DT_BaseAnimatingOverlay);
}
