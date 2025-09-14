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
	public static new IClientNetworkable CreateObject(int entNum, int serialNum) {
		C_World ret = new C_World();
		ret.Init(entNum, serialNum);
		return ret;
	}
	public static RecvTable DT_World = new([
		new RecvPropDataTable<C_World>("baseclass", DT_BaseEntity),
		new RecvPropFloat<C_World>("m_flWaveHeight", (instance) => ref instance.WaveHeight),
		new RecvPropVector<C_World>("m_WorldMins", (instance) => ref instance.WorldMins),
		new RecvPropVector<C_World>("m_WorldMaxs", (instance) => ref instance.WorldMaxs),
		new RecvPropBool<C_World>("m_bStartDark", (instance) => ref instance.StartDark),
		new RecvPropFloat<C_World>("m_flMaxOccludeeArea", (instance) => ref instance.MaxOccludeeArea),
		new RecvPropFloat<C_World>("m_flMinOccluderArea", (instance) => ref instance.MinOccluderArea),
		new RecvPropFloat<C_World>("m_flMaxPropScreenSpaceWidth", (instance) => ref instance.MaxPropScreenSpaceWidth),
		new RecvPropFloat<C_World>("m_flMinPropScreenSpaceWidth", (instance) => ref instance.MinPropScreenSpaceWidth),
		new RecvPropString<C_World>("m_iszDetailSpriteMaterial", (instance) => ref instance.DetailSpriteMaterial),
		new RecvPropBool<C_World>("m_bColdWorld", (instance) => ref instance.ColdWorld),
	]);

	public static new readonly ClientClass ClientClass = new ClientClass("World", CreateObject, null, DT_World)
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