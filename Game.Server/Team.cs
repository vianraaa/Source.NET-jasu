using Source.Common;
using Source;
using Game.Shared;

namespace Game.Server;
public class Team : BaseEntity
{
	public static readonly SendTable DT_Team = new([
		SendPropInt(FIELDOF(nameof(TeamNum)), 5),
		SendPropInt(FIELDOF(nameof(Score)), 0),
		SendPropInt(FIELDOF(nameof(RoundsWon)), 8),
		SendPropString(FIELDOF(nameof(Teamname))),
	]);
	public static readonly new ServerClass ServerClass = new ServerClass("Team", DT_Team).WithManualClassID(StaticClassIndices.CTeam);

	public readonly List<BasePlayer> Players = [];
	public InlineArray32<char> Teamname;
	public int Score;
	public int RoundsWon;
	public int Deaths;
	public int LastSpawn;
	public new int TeamNum;
}