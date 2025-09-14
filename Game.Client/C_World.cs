using Game.Client.HL2MP;
using Game.Shared;

using Source;
using Source.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client;

[LinkEntityToClass(LocalName = "player")]
public class C_World : C_BaseEntity
{
	public static IClientNetworkable CreateObject(int entNum, int serialNum) {
		C_World ret = new C_World();
		ret.Init(entNum, serialNum);
		return ret;
	}
	public static RecvTable DT_World = new([
		new RecvPropDataTable<C_World>("baseclass", (_) => ref DT_BaseEntity, PropFlags.Collapsible),
		new RecvPropFloat<C_World>("m_flWaveHeight", (instance) => ref instance.WaveHeight, PropFlags.RoundUp),
		new RecvPropVector<C_World>("m_WorldMins", (instance) => ref instance.WorldMins, PropFlags.Coord),
		new RecvPropVector<C_World>("m_WorldMaxs", (instance) => ref instance.WorldMaxs, PropFlags.Coord),
		new RecvPropBool<C_World>("m_bStartDark", (instance) => ref instance.StartDark, PropFlags.Unsigned),
		new RecvPropFloat<C_World>("m_flMaxOccludeeArea", (instance) => ref instance.MaxOccludeeArea, PropFlags.NoScale),
		new RecvPropFloat<C_World>("m_flMinOccluderArea", (instance) => ref instance.MinOccluderArea, PropFlags.NoScale),
		new RecvPropFloat<C_World>("m_flMaxPropScreenSpaceWidth", (instance) => ref instance.MaxPropScreenSpaceWidth, PropFlags.NoScale),
		new RecvPropFloat<C_World>("m_flMinPropScreenSpaceWidth", (instance) => ref instance.MinPropScreenSpaceWidth, PropFlags.NoScale),
		new RecvPropSpan<C_World, char>("m_iszDetailSpriteMaterial", (instance) => instance.DetailSpriteMaterial),
		new RecvPropBool<C_World>("m_bColdWorld", (instance) => ref instance.ColdWorld, PropFlags.Unsigned),
	]);

	public static readonly ClientClass ClientClass = new ClientClass("World", CreateObject, null, DT_World)
																		.WithManualClassID(StaticClassIndices.CWorld);


	float WaveHeight;
	Vector3 WorldMins;
	Vector3 WorldMaxs;
	bool StartDark;
	float MaxOccludeeArea;
	float MinOccluderArea;
	float MaxPropScreenSpaceWidth;
	float MinPropScreenSpaceWidth;
	InlineArray256<char> DetailSpriteMaterial;
	bool ColdWorld;
}