using Source.Common.Client;
using Source.Engine.Client;
using Source.Engine.Server;

namespace Source.Engine;

public class EngineClient(ClientState cl, GameServer sv, Cbuf Cbuf, Scr Scr) : IEngineClient
{
	public ReadOnlySpan<char> Key_LookupBinding(ReadOnlySpan<char> binding) {
		return "";
	}
	public void GetUILanguage(Span<char> destination) {
		"english".CopyTo(destination);
	}

	public void GetMainMenuBackgroundName(Span<char> dest) {
		"kagami".CopyTo(dest);
	}

	public bool IsDrawingLoadingImage() => Scr.DrawLoading;

	public int GetMaxClients() => cl.MaxClients;

	public ReadOnlySpan<char> GetLevelName() {
		if (sv.IsDedicated())
			return "Dedicated Server";
		else if (!cl.IsConnected())
			return "";
		return cl.LevelFileName;
	}

	public void ClientCmd_Unrestricted(ReadOnlySpan<char> cmdString) => Cbuf.AddText(cmdString);
	public void ExecuteClientCmd(ReadOnlySpan<char> cmdString) {
		Cbuf.AddText(cmdString);
		Cbuf.Execute();
	}

	public bool IsLevelMainMenuBackground() => sv.IsLevelMainMenuBackground();

	public bool IsPaused() => cl.IsPaused();

	public bool GetPlayerInfo(int playerIndex, out PlayerInfo playerInfo) {
		playerIndex--;
		if (playerIndex >= cl.MaxClients || playerIndex < 0) {
			playerInfo = new();
			return false;
		}

		Assert(cl.UserInfoTable != null);
		if (cl.UserInfoTable == null) {
			playerInfo = new();
			return false;
		}

		Assert(playerIndex < cl.UserInfoTable.GetNumStrings());
		if (playerIndex >= cl.UserInfoTable.GetNumStrings()) {
			playerInfo = new();
			return false;
		}

		Span<byte> pi = cl.UserInfoTable.GetStringUserData(playerIndex);
		PlayerInfo.FromBytes(pi, out playerInfo);
		return true; // todo: the rest of this
	}
}
