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
	public static readonly RecvTable DT_World = new(DT_BaseEntity, [
		RecvPropFloat(FIELDOF(nameof(WaveHeight))),
		RecvPropVector(FIELDOF(nameof(WorldMins))),
		RecvPropVector(FIELDOF(nameof(WorldMaxs))),
		RecvPropInt(FIELDOF(nameof(StartDark))),
		RecvPropFloat(FIELDOF(nameof(MaxOccludeeArea))),
		RecvPropFloat(FIELDOF(nameof(MinOccluderArea))),
		RecvPropFloat(FIELDOF(nameof(MaxPropScreenSpaceWidth))),
		RecvPropFloat(FIELDOF(nameof(MinPropScreenSpaceWidth))),
		RecvPropString(FIELDOF(nameof(DetailSpriteMaterial))),
		RecvPropInt(FIELDOF(nameof(ColdWorld))),
	]);

	public static new readonly ClientClass ClientClass = new ClientClass("World", null, null, DT_World)
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