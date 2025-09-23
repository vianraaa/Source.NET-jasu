using Source.Common;

namespace Game.Client;

public partial class C_BaseAnimatingOverlay : C_BaseAnimating {
	public static readonly RecvTable DT_AnimationLayer = new([

	]); public static readonly ClientClass SC_AnimationLayer = new ClientClass("AnimationLayer", null, null, DT_AnimationLayer);

	public static readonly RecvTable DT_OverlayVars = new([

	]); public static readonly ClientClass CC_OverlayVars = new ClientClass("BaseAnimatingOverlay", null, null, DT_OverlayVars);

	public static readonly RecvTable DT_BaseAnimatingOverlay = new(DT_BaseAnimating, [
		RecvPropDataTable("overlay_vars", DT_OverlayVars)
	]); public static readonly new ClientClass ClientClass = new ClientClass("BaseAnimatingOverlay", null, null, DT_BaseAnimatingOverlay);
}
