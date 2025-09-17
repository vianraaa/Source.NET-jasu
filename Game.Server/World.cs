using Game.Shared;

using Source.Common;

using System.Numerics;

namespace Game.Server;

[LinkEntityToClass(LocalName = "worldspawn")]
public class World : BaseEntity
{
	public static SendTable DT_World = new([
		SendPropDataTable("baseclass", FIELDOF(nameof(DT_BaseEntity))),

		SendPropFloat(FIELDOF(nameof(WaveHeight)), 8, PropFlags.RoundUp, 0, 8),
		SendPropVector(FIELDOF(nameof(WorldMins)), -1, PropFlags.Coord),
		SendPropVector(FIELDOF(nameof(WorldMaxs)), -1, PropFlags.Coord),
		SendPropInt(FIELDOF(nameof(StartDark)), 1, PropFlags.Unsigned),
		SendPropFloat(FIELDOF(nameof(MaxOccludeeArea)), 0, PropFlags.NoScale),
		SendPropFloat(FIELDOF(nameof(MinOccluderArea)), 0, PropFlags.NoScale),
		SendPropFloat(FIELDOF(nameof(MaxPropScreenSpaceWidth)), 0, PropFlags.NoScale),
		SendPropFloat(FIELDOF(nameof(MinPropScreenSpaceWidth)), 0, PropFlags.NoScale),
		SendPropStringT(FIELDOF(nameof(DetailSpriteMaterial))),
		SendPropInt(FIELDOF(nameof(ColdWorld)), 1, PropFlags.Unsigned),
	]);

	public static readonly new ServerClass ServerClass = new ServerClass("World", DT_World)
																		.WithManualClassID(StaticClassIndices.CWorld);
	float WaveHeight;
	Vector3 WorldMins;
	Vector3 WorldMaxs;
	bool StartDark;
	float MaxOccludeeArea;
	float MinOccluderArea;
	float MaxPropScreenSpaceWidth;
	float MinPropScreenSpaceWidth;
	string? DetailSpriteMaterial;
	bool ColdWorld;
}
