using Source;
using Source.Common;
using Source.Common.Engine;

using System.Reflection;

namespace Game.Server;
public partial class BasePlayer : BaseCombatCharacter
{
	public static readonly SendTable DT_PlayerState = new([
		SendPropInt(FIELDOF(nameof(DeadFlag)), 1, PropFlags.Unsigned)
	]); public static readonly ServerClass CC_PlayerState = new("PlayerState", DT_PlayerState);

	public static readonly SendTable DT_Local = new([

	]); public static readonly ServerClass CC_Local = new("Local", DT_Local);

	public static readonly SendTable DT_LocalPlayerExclusive = new([

	]); public static readonly ServerClass PlayerExclusive = new("LocalPlayerExclusive", DT_LocalPlayerExclusive);

	public static readonly SendTable DT_BasePlayer = new(DT_BaseCombatCharacter, [
		SendPropDataTable(nameof(pl), DT_PlayerState, SendProxy_DataTableToDataTable),
		SendPropEHandle(FIELDOF(nameof(Vehicle))),
		SendPropEHandle(FIELDOF(nameof(UseEntity))),
		SendPropInt(FIELDOF(nameof(LifeState)), 3, PropFlags.Unsigned ),
		SendPropEHandle(FIELDOF(nameof(ColorCorrectionCtrl))), // << gmod specific
		SendPropFloat(FIELDOF(nameof(MaxSpeed)), 12, PropFlags.RoundDown, 0.0f, 2048.0f ),
		SendPropInt(FIELDOF(nameof(Flags)), Constants.PLAYER_FLAG_BITS, PropFlags.Unsigned|PropFlags.ChangesOften, SendProxy_CropFlagsToPlayerFlagBitsLength ),
		SendPropInt(FIELDOF(nameof(ObserverMode)), 3, PropFlags.Unsigned ),
		SendPropEHandle(FIELDOF(nameof(ObserverTarget))),
		SendPropInt(FIELDOF(nameof(FOV)), 8, PropFlags.Unsigned ),
		SendPropInt(FIELDOF(nameof(FOVStart)), 8, PropFlags.Unsigned ),
		SendPropFloat(FIELDOF(nameof(FOVTime)), 0, PropFlags.NoScale),
		SendPropFloat(FIELDOF(nameof(DefaultFOV)), 8, PropFlags.Unsigned ),
		SendPropEHandle(FIELDOF(nameof(ZoomOwner))),
		// SendPropArray( SendPropEHandle( FIELDOF_ARRAYINDEX(nameof(ViewModel), 1) ), ViewModel ), << todo
		SendPropString(FIELDOF(nameof(LastPlaceName))),
		SendPropBool(FIELDOF(nameof(UseWeaponsInVehicle))),
		SendPropDataTable( "localdata", DT_LocalPlayerExclusive, SendProxy_SendLocalDataTable),
	]);

	readonly EHANDLE Vehicle = new();
	readonly EHANDLE UseEntity = new();
	readonly EHANDLE ObserverTarget = new();
	readonly EHANDLE ZoomOwner = new();
	int LifeState;
	int BonusProgress;
	int BonusChallenge;
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

	private static void SendProxy_CropFlagsToPlayerFlagBitsLength(SendProp prop, object instance, FieldInfo field, ref DVariant outData, int element, int objectID) {
		throw new NotImplementedException();
	}

	private static object? SendProxy_SendLocalDataTable(SendProp prop, object instance, FieldInfo data, SendProxyRecipients recipients, int objectID) {
		throw new NotImplementedException();
	}

	public static readonly new ServerClass ServerClass = new ServerClass("BasePlayer", DT_BasePlayer);

	bool DeadFlag;
	readonly PlayerState pl = new();
}