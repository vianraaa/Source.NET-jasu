using Game.Shared;

using Source;
using Source.Common;

using System.Numerics;

namespace Game.Client;

public interface IHasLocalToGlobalFlexSettings;
public partial class C_BaseFlex : C_BaseAnimatingOverlay, IHasLocalToGlobalFlexSettings {
	public static readonly RecvTable DT_BaseFlex = new(DT_BaseAnimatingOverlay, [
		RecvPropArray3  (FIELDOF_ARRAY(nameof(FlexWeight)), RecvPropFloat(FIELDOF_ARRAY(nameof(FlexWeight)))),
		RecvPropInt     (FIELDOF(nameof(BlinkToggle))),
		RecvPropVector  (FIELDOF(nameof(ViewTarget))),
		
		RecvPropFloat   ( FIELDOF_VECTORELEM(nameof(ViewOffset), 0)),
		RecvPropFloat   ( FIELDOF_VECTORELEM(nameof(ViewOffset), 1)),
		RecvPropFloat   ( FIELDOF_VECTORELEM(nameof(ViewOffset), 2)),
		
		RecvPropVector  ( FIELDOF(nameof(Lean))),
	]);
	public static readonly new ClientClass ClientClass = new ClientClass("BaseFlex", null, null, DT_BaseFlex).WithManualClassID(StaticClassIndices.CBaseFlex);
	public InlineArray96<float> FlexWeight;
	public int BlinkToggle;
	public Vector3 ViewTarget;
	public Vector3 ViewOffset;
	public Vector3 Lean;
}