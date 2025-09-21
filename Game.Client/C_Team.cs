using Game.Shared;

using Source;
using Source.Common;

namespace Game.Client;

public class C_Team : C_BaseEntity
{
	public static readonly RecvTable DT_Team = new([
		RecvPropInt( FIELDOF(nameof(TeamNum))),
		RecvPropInt( FIELDOF(nameof(Score))),
		RecvPropInt( FIELDOF(nameof(RoundsWon)) ),
		RecvPropString( FIELDOF(nameof(Teamname))),
	]);

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