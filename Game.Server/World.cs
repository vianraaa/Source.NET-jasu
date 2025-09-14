using Game.Shared;

using Source.Common;

using System.Numerics;

namespace Game.Server;

[LinkEntityToClass(LocalName = "player")]
public class World : BaseEntity
{
	public static SendTable DT_World = new([
		new SendPropDataTable<World>("baseclass", (_) => ref DT_BaseEntity),
		new SendPropFloat<World>("m_flWaveHeight", (instance) => ref instance.WaveHeight, 8, PropFlags.RoundUp),
		new SendPropVector<World>("m_WorldMins", (instance) => ref instance.WorldMins, -1, PropFlags.Coord),
		new SendPropVector<World>("m_WorldMaxs", (instance) => ref instance.WorldMaxs, -1, PropFlags.Coord),
		new SendPropBool<World>("m_bStartDark", (instance) => ref instance.StartDark, -1, PropFlags.Unsigned),
		new SendPropFloat<World>("m_flMaxOccludeeArea", (instance) => ref instance.MaxOccludeeArea, 0, PropFlags.NoScale),
		new SendPropFloat<World>("m_flMinOccluderArea", (instance) => ref instance.MinOccluderArea, 0, PropFlags.NoScale),
		new SendPropFloat<World>("m_flMaxPropScreenSpaceWidth", (instance) => ref instance.MaxPropScreenSpaceWidth, 0, PropFlags.NoScale),
		new SendPropFloat<World>("m_flMinPropScreenSpaceWidth", (instance) => ref instance.MinPropScreenSpaceWidth, 0, PropFlags.NoScale),
		new SendPropString<World>("m_iszDetailSpriteMaterial", (instance) => ref instance.DetailSpriteMaterial),
		new SendPropBool<World>("m_bColdWorld", (instance) => ref instance.ColdWorld, 1, PropFlags.Unsigned),
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
