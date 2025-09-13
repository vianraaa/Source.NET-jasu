using Game.Client.HL2;
using Game.Shared;

using Source.Common;

namespace Game.Client.HL2MP;

[LinkEntityToClass(LocalName = "player")]
[ManualClassIndex(Index = 76)]
public partial class C_HL2MP_Player : C_BaseHLPlayer
{
	public override void PostDataUpdate(DataUpdateType updateType) {
		base.PostDataUpdate(updateType);
	}
}