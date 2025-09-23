using Game.Shared;

using Source.Common;

namespace Game.Client;

public class C_AnimationLayer
{
	public static readonly RecvTable DT_AnimationLayer = new([
		RecvPropInt(FIELDOF(nameof(Sequence))),
		RecvPropFloat(FIELDOF(nameof(Cycle))),
		RecvPropFloat(FIELDOF(nameof(PrevCycle))),
		RecvPropFloat(FIELDOF(nameof(Weight))),
		RecvPropInt(FIELDOF(nameof(Order))),
	]); public static readonly ClientClass ClientClass = new ClientClass("Client", null, null, DT_AnimationLayer).WithManualClassID(StaticClassIndices.CBaseAnimatingOverlay);

	public int Sequence;
	public float Cycle;
	public float PrevCycle;
	public float Weight;
	public int Order;
}


public partial class C_BaseAnimatingOverlay : C_BaseAnimating {
	public const int MAX_OVERLAYS = 15;

	public static readonly RecvTable DT_OverlayVars = new([
		RecvPropList(FIELDOF(nameof(AnimOverlay)), ResizeAnimationLayerCallback, MAX_OVERLAYS, RecvPropDataTable(null, C_AnimationLayer.DT_AnimationLayer))
	]);

	private static void ResizeAnimationLayerCallback(object instance, object list, int length) {
		throw new NotImplementedException();
	}

	public static readonly ClientClass CC_OverlayVars = new ClientClass("OverlayVars", null, null, DT_OverlayVars);

	public static readonly RecvTable DT_BaseAnimatingOverlay = new(DT_BaseAnimating, [
		RecvPropDataTable("overlay_vars", DT_OverlayVars)
	]); public static readonly new ClientClass ClientClass = new ClientClass("BaseAnimatingOverlay", null, null, DT_BaseAnimatingOverlay);

	readonly List<C_AnimationLayer> AnimOverlay = [];
}
