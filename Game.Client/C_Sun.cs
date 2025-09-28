using Game.Client;
using Game.Shared;

using Source;
using Source.Common;

using System.Numerics;

namespace Game.Server;
using FIELD = Source.FIELD<C_Sun>;

public class C_Sun : C_BaseEntity
{
	public static readonly RecvTable DT_Sun = new([
		RecvPropInt(FIELD.OF(nameof(Render))),
		RecvPropInt(FIELD.OF(nameof(Overlay))),
		RecvPropVector(FIELD.OF(nameof(Direction))),
		RecvPropInt(FIELD.OF(nameof(On))),
		RecvPropInt(FIELD.OF(nameof(Size))),
		RecvPropInt(FIELD.OF(nameof(OverlaySize))),
		RecvPropInt(FIELD.OF(nameof(Material))),
		RecvPropInt(FIELD.OF(nameof(OverlayMaterial))),
		RecvPropFloat(FIELD.OF(nameof(HDRColorScale))),
	]);
	public static readonly new ClientClass ClientClass = new ClientClass("Sun", DT_Sun).WithManualClassID(StaticClassIndices.CSun);

	public Color Render;
	public Color Overlay;
	public Vector3 Direction;
	public bool On;
	public int Size;
	public int OverlaySize;
	public int Material;
	public int OverlayMaterial;
	public int HDRColorScale;
}