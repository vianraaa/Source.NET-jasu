using Game.Shared;

using Source;
using Source.Common;

using System.Reflection;

namespace Game.Client;

public class C_Team : C_BaseEntity
{
	public static readonly RecvTable DT_Team = new([
		RecvPropInt( FIELDOF(nameof(TeamNum))),
		RecvPropInt( FIELDOF(nameof(Score))),
		RecvPropInt( FIELDOF(nameof(RoundsWon)) ),
		RecvPropString( FIELDOF(nameof(Teamname))),

		RecvPropInt( "player_array_element", 0, RecvProxy_PlayerList ),
		RecvPropArray2(RecvProxyArrayLength_PlayerArray, Constants.MAX_PLAYERS, "player_array")
	]);

	private static void RecvProxyArrayLength_PlayerArray(object instance, int objectID, int currentArrayLength) {

	}

	private static void RecvProxy_PlayerList(ref readonly RecvProxyData data, object instance, FieldInfo field) {

	}

	public static readonly new ClientClass ClientClass = new ClientClass("Team", null, null, DT_Team).WithManualClassID(StaticClassIndices.CTeam);

	public readonly List<int> Players = [];
	public InlineArray32<char> Teamname;
	public int Score;
	public int RoundsWon;

	public int Deaths;
	public int Ping;
	public int Packetloss;
	public new int TeamNum;
}