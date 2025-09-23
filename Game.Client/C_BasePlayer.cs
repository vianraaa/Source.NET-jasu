using Game.Client.GarrysMod;
using Game.Client.HL2MP;

using Source;
using Source.Common;
using Source.Common.Client;
using static Source.Common.Networking.svc_ClassInfo;

using Steamworks;

namespace Game.Client;

public partial class C_BasePlayer : C_BaseCombatCharacter, IGameEventListener2
{
	public static readonly RecvTable DT_PlayerState = new([
		RecvPropInt(FIELDOF(nameof(DeadFlag)))
	]); public static readonly ClientClass CC_PlayerState = new("PlayerState", null, null, DT_PlayerState);

	public static readonly RecvTable DT_Local = new([

	]); public static readonly ClientClass CC_Local = new ClientClass("Local", null, null, DT_Local);

	public static readonly RecvTable DT_LocalPlayerExclusive = new([

	]); public static readonly ClientClass CC_LocalPlayerExclusive = new ClientClass("LocalPlayerExclusive", null, null, DT_LocalPlayerExclusive);

	public static readonly RecvTable DT_BasePlayer = new(DT_BaseCombatCharacter, [
		RecvPropDataTable(nameof(pl), FIELDOF(nameof(pl)), DT_PlayerState),
		RecvPropEHandle(FIELDOF(nameof(Vehicle))),
		RecvPropEHandle(FIELDOF(nameof(UseEntity))),
		RecvPropInt(FIELDOF(nameof(LifeState))),
		RecvPropEHandle(FIELDOF(nameof(ColorCorrectionCtrl))), // << gmod specific
		RecvPropFloat(FIELDOF(nameof(MaxSpeed))),
		RecvPropInt(FIELDOF(nameof(Flags))),
		RecvPropInt(FIELDOF(nameof(ObserverMode))),
		RecvPropEHandle(FIELDOF(nameof(ObserverTarget))),
		RecvPropInt(FIELDOF(nameof(FOV))),
		RecvPropInt(FIELDOF(nameof(FOVStart))),
		RecvPropFloat(FIELDOF(nameof(FOVTime))),
		RecvPropFloat(FIELDOF(nameof(DefaultFOV))),
		RecvPropEHandle(FIELDOF(nameof(ZoomOwner))),
		// SendPropArray( SendPropEHandle( FIELDOF_ARRAYINDEX(nameof(ViewModel), 1) ), ViewModel ), << todo
		RecvPropString(FIELDOF(nameof(LastPlaceName))),
		RecvPropBool(FIELDOF(nameof(UseWeaponsInVehicle))),
		RecvPropDataTable("localdata", DT_LocalPlayerExclusive),
	]); public static readonly new ClientClass ClientClass = new ClientClass("BasePlayer", null, null, DT_BasePlayer);

	readonly EHANDLE Vehicle = new();
	readonly EHANDLE UseEntity = new();
	readonly EHANDLE ObserverTarget = new();
	readonly EHANDLE ZoomOwner = new();
	int LifeState;
	float MaxSpeed;
	int Flags;
	int ObserverMode;
	int FOV;
	int FOVStart;
	float FOVTime;
	float DefaultFOV;
	InlineArray18<char> LastPlaceName;
	readonly EHANDLE ColorCorrectionCtrl = new();
	bool UseWeaponsInVehicle;
	readonly PlayerState pl = new();

	static C_BasePlayer? localPlayer;
	internal static C_BasePlayer? GetLocalPlayer() => localPlayer;

	public void FireGameEvent(IGameEvent ev) {
		throw new NotImplementedException();
	}

	public override void Dispose() {
		base.Dispose();
		if (this == localPlayer) {
			localPlayer = null;
		}
	}

	public override void PostDataUpdate(DataUpdateType updateType) {
		if(updateType == DataUpdateType.Created) {
			int localPlayerIndex = engine.GetLocalPlayer();

			if(localPlayerIndex == Index) {
				localPlayer = this;
			}
		}

		base.PostDataUpdate(updateType);
	}

	public bool DeadFlag;
}
