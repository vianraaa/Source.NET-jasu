using Game.Client.HL2;
using Game.Shared;

using Source.Common;
using Source.Common.Mathematics;

namespace Game.Client.HL2MP;

public partial class C_HL2MP_Player : C_BaseHLPlayer
{
	public static readonly RecvTable DT_HL2MP_Player = new(DT_BasePlayer, [

	]);
	public static readonly ClientClass ClientClass = new ClientClass("HL2MP_Player", null, null, DT_HL2MP_Player)
															.WithManualClassID(StaticClassIndices.CHL2MP_Player);

	public QAngle EyeAngles;
	public EHANDLE Ragdoll = new();
	public int SpawnInterpCounter;
	public int PlayerSoundType;
	public bool IsWalking;

	public override void PostDataUpdate(DataUpdateType updateType) {
		base.PostDataUpdate(updateType);
	}
}