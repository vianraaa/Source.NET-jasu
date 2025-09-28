using Game.Shared;

using Source;
using Source.Common;

using System.Numerics;

namespace Game.Server;
using FIELD = Source.FIELD<ShadowControl>;

public class ShadowControl : BaseEntity
{
	public FogParams Fog;
	public static readonly SendTable DT_ShadowControl = new([
		SendPropVector(FIELD.OF(nameof(ShadowDirection)), -1, PropFlags.NoScale),
		SendPropInt(FIELD.OF(nameof(ShadowColor)), 32, PropFlags.Unsigned),
		SendPropFloat(FIELD.OF(nameof(ShadowMaxDist)), 0, PropFlags.NoScale),
		SendPropBool(FIELD.OF(nameof(DisableShadows))),
		SendPropBool(FIELD.OF(nameof(EnableLocalLightShadows))),
	]);
	public static readonly new ServerClass ServerClass = new ServerClass("ShadowControl", DT_ShadowControl).WithManualClassID(StaticClassIndices.CShadowControl);

	public Vector3 ShadowDirection;
	public Color ShadowColor;
	public float ShadowMaxDist;
	public bool DisableShadows;
	public bool EnableLocalLightShadows;
}