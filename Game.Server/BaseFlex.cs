using Game.Shared;

using Source;
using Source.Common;

using System.Numerics;

namespace Game.Server;

public class BaseFlex : BaseAnimatingOverlay {
	public static readonly SendTable DT_BaseFlex = new(DT_BaseAnimatingOverlay, [
		SendPropArray3  (FIELDOF_ARRAY(nameof(FlexWeight)), SendPropFloat(FIELDOF_ARRAY(nameof(FlexWeight)), 12, PropFlags.RoundDown, 0.0f, 1.0f )),
		SendPropInt     (FIELDOF(nameof(BlinkToggle)), 1, PropFlags.Unsigned ),
		SendPropVector  (FIELDOF(nameof(ViewTarget)), -1, PropFlags.Coord),

		SendPropFloat   ( FIELDOF_VECTORELEM(nameof(ViewOffset), 0), 0, PropFlags.NoScale ),
		SendPropFloat   ( FIELDOF_VECTORELEM(nameof(ViewOffset), 1), 0, PropFlags.NoScale ),
		SendPropFloat   ( FIELDOF_VECTORELEM(nameof(ViewOffset), 2), 0, PropFlags.NoScale ),

		SendPropVector  ( FIELDOF(nameof(Lean)), -1, PropFlags.Coord ),
	]);
	public static readonly new ServerClass ServerClass = new ServerClass("BaseFlex", DT_BaseFlex).WithManualClassID(StaticClassIndices.CBaseFlex);

	public InlineArray96<float> FlexWeight;
	public int BlinkToggle;
	public Vector3 ViewTarget;
	public Vector3 ViewOffset;
	public Vector3 Lean;
}
