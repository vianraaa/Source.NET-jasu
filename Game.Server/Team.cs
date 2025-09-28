using Source.Common;
using Source;
using Game.Shared;
using System.Reflection;

namespace Game.Server;
using FIELD = Source.FIELD<Team>;

public class Team : BaseEntity
{
	public static readonly SendTable DT_Team = new([
		SendPropInt(FIELD.OF(nameof(TeamNum)), 5),
		SendPropInt(FIELD.OF(nameof(Score)), 0),
		SendPropInt(FIELD.OF(nameof(RoundsWon)), 8),
		SendPropString(FIELD.OF(nameof(Teamname))),

		SendPropInt("player_array_element", 10, PropFlags.Unsigned, SendProxy_PlayerList, 4),
		SendPropArray2(SendProxyArrayLength_PlayerArray, Constants.MAX_PLAYERS, "player_array")
	]);

	private static int SendProxyArrayLength_PlayerArray(object instance, int objectID) => ((Team)instance).Players.Count;
	private static void SendProxy_PlayerList(SendProp prop, object instance, IFieldAccessor field, ref DVariant outData, int element, int objectID) {
	}
	public static readonly new ServerClass ServerClass = ServerClass.New(DT_Team).WithManualClassID(StaticClassIndices.CTeam);

	public readonly List<BasePlayer> Players = [];
	public InlineArray32<char> Teamname;
	public int Score;
	public int RoundsWon;
	public int Deaths;
	public int LastSpawn;
	public new int TeamNum;
}