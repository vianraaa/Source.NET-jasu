namespace Game.Server;

using Game.Shared;

using Source;
using Source.Common;

using FIELD = Source.FIELD<PlayerResource>;

public class PlayerResource : BaseEntity
{
	public static readonly SendTable DT_PlayerResource = new([
		SendPropArray3(FIELD.OF_ARRAY(nameof(Ping)), SendPropInt(FIELD.OF_ARRAYINDEX(nameof(Ping), 0), 10, PropFlags.Unsigned ) ),
		SendPropArray3(FIELD.OF_ARRAY(nameof(Score)), SendPropInt(FIELD.OF_ARRAYINDEX(nameof(Score), 0), 12 ) ),
		SendPropArray3(FIELD.OF_ARRAY(nameof(Deaths)), SendPropInt(FIELD.OF_ARRAYINDEX(nameof(Deaths), 0), 12 ) ),
		SendPropArray3(FIELD.OF_ARRAY(nameof(Connected)), SendPropInt(FIELD.OF_ARRAYINDEX(nameof(Connected), 0), 1, PropFlags.Unsigned ) ),
		SendPropArray3(FIELD.OF_ARRAY(nameof(Team)), SendPropInt(FIELD.OF_ARRAYINDEX(nameof(Team), 0), 4 ) ),
		SendPropArray3(FIELD.OF_ARRAY(nameof(Alive)), SendPropInt(FIELD.OF_ARRAYINDEX(nameof(Alive), 0), 1, PropFlags.Unsigned ) ),
		SendPropArray3(FIELD.OF_ARRAY(nameof(Health)), SendPropInt(FIELD.OF_ARRAYINDEX(nameof(Health), 0), -1, PropFlags.VarInt | PropFlags.Unsigned ) ),
		SendPropArray3(FIELD.OF_ARRAY(nameof(AccountID)), SendPropInt(FIELD.OF_ARRAYINDEX(nameof(AccountID), 0), 32, PropFlags.Unsigned ) ),
		SendPropArray3(FIELD.OF_ARRAY(nameof(Valid)), SendPropInt(FIELD.OF_ARRAYINDEX(nameof(Valid), 0), 1, PropFlags.Unsigned ) ),
	]);
	public static readonly new ServerClass ServerClass = new ServerClass("PlayerResource", DT_PlayerResource).WithManualClassID(StaticClassIndices.CPlayerResource);

	InlineArrayMaxPlayersPlusOne<string> Name = new();
	InlineArrayMaxPlayersPlusOne<int> Ping = new();
	InlineArrayMaxPlayersPlusOne<int> Score = new();
	InlineArrayMaxPlayersPlusOne<int> Deaths = new();
	InlineArrayMaxPlayersPlusOne<bool> Connected = new();
	InlineArrayMaxPlayersPlusOne<int> Team = new();
	InlineArrayMaxPlayersPlusOne<bool> Alive = new();
	new InlineArrayMaxPlayersPlusOne<int> Health = new();
	InlineArrayMaxPlayersPlusOne<Color> Colors = new();
	InlineArrayMaxPlayersPlusOne<uint> AccountID = new();
	InlineArrayMaxPlayersPlusOne<bool> Valid = new();
	InlineArrayMaxPlayersPlusOne<string> UnconnectedName = new();
}
