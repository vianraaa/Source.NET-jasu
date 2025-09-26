using Source;
using Source.Common;
using Source.Common.Engine;

using System.Reflection;

namespace Game.Server;
using FIELD = Source.FIELD<BasePlayer>;

public partial class BasePlayer : BaseCombatCharacter
{
	public static readonly SendTable DT_PlayerState = new([
		SendPropInt(FIELD.OF(nameof(DeadFlag)), 1, PropFlags.Unsigned)
	]); public static readonly ServerClass CC_PlayerState = new("PlayerState", DT_PlayerState);

	
	public static readonly SendTable DT_LocalPlayerExclusive = new([
		SendPropDataTable(nameof(Local), PlayerLocalData.DT_Local)
	]); public static readonly ServerClass PlayerExclusive = new("LocalPlayerExclusive", DT_LocalPlayerExclusive);

	public static readonly SendTable DT_BasePlayer = new(DT_BaseCombatCharacter, [
		SendPropDataTable(nameof(pl), DT_PlayerState, SendProxy_DataTableToDataTable),
		SendPropEHandle(FIELD.OF(nameof(Vehicle))),
		SendPropEHandle(FIELD.OF(nameof(UseEntity))),
		SendPropInt(FIELD.OF(nameof(LifeState)), 3, PropFlags.Unsigned ),
		SendPropEHandle(FIELD.OF(nameof(ColorCorrectionCtrl))), // << gmod specific
		SendPropFloat(FIELD.OF(nameof(MaxSpeed)), 12, PropFlags.RoundDown, 0.0f, 2048.0f ),
		SendPropInt(FIELD.OF(nameof(Flags)), Constants.PLAYER_FLAG_BITS, PropFlags.Unsigned|PropFlags.ChangesOften, SendProxy_CropFlagsToPlayerFlagBitsLength ),
		SendPropInt(FIELD.OF(nameof(ObserverMode)), 3, PropFlags.Unsigned ),
		SendPropEHandle(FIELD.OF(nameof(ObserverTarget))),
		SendPropInt(FIELD.OF(nameof(FOV)), 8, PropFlags.Unsigned ),
		SendPropInt(FIELD.OF(nameof(FOVStart)), 8, PropFlags.Unsigned ),
		SendPropFloat(FIELD.OF(nameof(FOVTime)), 0, PropFlags.NoScale),
		SendPropFloat(FIELD.OF(nameof(DefaultFOV)), 8, PropFlags.Unsigned ),
		SendPropEHandle(FIELD.OF(nameof(ZoomOwner))),
		// SendPropArray( SendPropEHandle( FIELD.OF_ARRAYINDEX(nameof(ViewModel), 1) ), ViewModel ), << todo
		SendPropString(FIELD.OF(nameof(LastPlaceName))),
		SendPropBool(FIELD.OF(nameof(UseWeaponsInVehicle))),
		SendPropDataTable( "localdata", DT_LocalPlayerExclusive, SendProxy_SendLocalDataTable),
	]);

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

	private static void SendProxy_CropFlagsToPlayerFlagBitsLength(SendProp prop, object instance, IFieldAccessor field, ref DVariant outData, int element, int objectID) {
		throw new NotImplementedException();
	}

	private static object? SendProxy_SendLocalDataTable(SendProp prop, object instance, IFieldAccessor data, SendProxyRecipients recipients, int objectID) {
		throw new NotImplementedException();
	}

	public static readonly new ServerClass ServerClass = new ServerClass("BasePlayer", DT_BasePlayer);

	bool DeadFlag;
	readonly PlayerState pl = new();
	readonly PlayerLocalData Local = new();
}