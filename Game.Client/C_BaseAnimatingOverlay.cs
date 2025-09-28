using Game.Shared;

using Source.Common;

using FIELD_AL = Source.FIELD<Game.Client.C_AnimationLayer>;
using FIELD_BAO = Source.FIELD<Game.Client.C_BaseAnimatingOverlay>;

namespace Game.Client;

public class C_AnimationLayer
{
	public static readonly RecvTable DT_AnimationLayer = new([
		RecvPropInt(FIELD_AL.OF(nameof(Sequence))),
		RecvPropFloat(FIELD_AL.OF(nameof(Cycle))),
		RecvPropFloat(FIELD_AL.OF(nameof(PrevCycle))),
		RecvPropFloat(FIELD_AL.OF(nameof(Weight))),
		RecvPropInt(FIELD_AL.OF(nameof(Order))),
	]); public static readonly ClientClass ClientClass = ClientClass.New(DT_AnimationLayer);

	public int Sequence;
	public float Cycle;
	public float PrevCycle;
	public float Weight;
	public int Order;
}


public partial class C_BaseAnimatingOverlay : C_BaseAnimating {
	public const int MAX_OVERLAYS = 15;

	public static readonly RecvTable DT_OverlayVars = new([
		RecvPropList(FIELD_BAO.OF(nameof(AnimOverlay)), ResizeAnimationLayerCallback, MAX_OVERLAYS, RecvPropDataTable(null, C_AnimationLayer.DT_AnimationLayer))
	]);

	private static void ResizeAnimationLayerCallback(object instance, object list, int length) {
		throw new NotImplementedException();
	}

	public static readonly ClientClass CC_OverlayVars = ClientClass.New(DT_OverlayVars);

	public static readonly RecvTable DT_BaseAnimatingOverlay = new(DT_BaseAnimating, [
		RecvPropDataTable("overlay_vars", DT_OverlayVars)
	]); public static readonly new ClientClass ClientClass = ClientClass.New(DT_BaseAnimatingOverlay);

	readonly List<C_AnimationLayer> AnimOverlay = [];
}
