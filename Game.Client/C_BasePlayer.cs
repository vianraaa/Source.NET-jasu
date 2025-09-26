using Source;
using Source.Common;

using FIELD = Source.FIELD<Game.Client.C_BasePlayer>;

namespace Game.Client;

public partial class C_BasePlayer : C_BaseCombatCharacter, IGameEventListener2
{
	public static readonly RecvTable DT_PlayerState = new([
		RecvPropInt(FIELD.OF(nameof(DeadFlag)))
	]); public static readonly ClientClass CC_PlayerState = new("PlayerState", null, null, DT_PlayerState);

	public static readonly RecvTable DT_LocalPlayerExclusive = new([
		RecvPropDataTable(nameof(Local), C_PlayerLocalData.DT_Local)
	]); public static readonly ClientClass CC_LocalPlayerExclusive = new ClientClass("LocalPlayerExclusive", null, null, DT_LocalPlayerExclusive);

	public static readonly RecvTable DT_BasePlayer = new(DT_BaseCombatCharacter, [
		RecvPropDataTable(nameof(pl), FIELD.OF(nameof(pl)), DT_PlayerState),
		RecvPropEHandle(FIELD.OF(nameof(Vehicle))),
		RecvPropEHandle(FIELD.OF(nameof(UseEntity))),
		RecvPropInt(FIELD.OF(nameof(LifeState))),
		RecvPropEHandle(FIELD.OF(nameof(ColorCorrectionCtrl))), // << gmod specific
		RecvPropFloat(FIELD.OF(nameof(MaxSpeed))),
		RecvPropInt(FIELD.OF(nameof(Flags))),
		RecvPropInt(FIELD.OF(nameof(ObserverMode))),
		RecvPropEHandle(FIELD.OF(nameof(ObserverTarget))),
		RecvPropInt(FIELD.OF(nameof(FOV))),
		RecvPropInt(FIELD.OF(nameof(FOVStart))),
		RecvPropFloat(FIELD.OF(nameof(FOVTime))),
		RecvPropFloat(FIELD.OF(nameof(DefaultFOV))),
		RecvPropEHandle(FIELD.OF(nameof(ZoomOwner))),
		// SendPropArray( SendPropEHandle( FIELD.OF_ARRAYINDEX(nameof(ViewModel), 1) ), ViewModel ), << todo
		RecvPropString(FIELD.OF(nameof(LastPlaceName))),
		RecvPropBool(FIELD.OF(nameof(UseWeaponsInVehicle))),
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
	readonly C_PlayerLocalData Local = new();
}
