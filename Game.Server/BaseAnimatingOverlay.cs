using Source.Common;

namespace Game.Server;

public class AnimationLayer {
	public static readonly SendTable DT_AnimationLayer = new([

	]); public static readonly ServerClass SC_AnimationLayer = new ServerClass("AnimationLayer", DT_AnimationLayer);
}

public class BaseAnimatingOverlay : BaseAnimating {
	public const int MAX_OVERLAYS = 15;

	public static readonly SendTable DT_OverlayVars = new([
		SendPropList(FIELDOF(nameof(AnimOverlay)), MAX_OVERLAYS, SendPropDataTable(null, AnimationLayer.DT_AnimationLayer))
	]); public static readonly ServerClass SC_OverlayVars = new ServerClass("OverlayVars", DT_OverlayVars);

	public static readonly SendTable DT_BaseAnimatingOverlay = new(DT_BaseAnimating, [
		SendPropDataTable("overlay_vars", DT_OverlayVars)
	]); public static readonly new ServerClass ServerClass = new ServerClass("BaseAnimatingOverlay", DT_BaseAnimatingOverlay);

	readonly List<AnimationLayer> AnimOverlay = [];
}