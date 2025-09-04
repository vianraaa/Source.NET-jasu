using Source.Common.Client;
using Source.Engine.Client;
using Source.Engine.Server;

namespace Source.Engine;

public class EngineClient(ClientState cl, GameServer sv, Cbuf Cbuf) : IEngineClient
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

	public int GetMaxClients() => cl.MaxClients;

	public ReadOnlySpan<char> GetLevelName() {
		if (sv.IsDedicated())
			return "Dedicated Server";
		else if (!cl.IsConnected())
			return "";
		return cl.LevelFileName;
	}

	public void ClientCmd_Unrestricted(ReadOnlySpan<char> cmdString) => Cbuf.AddText(cmdString);
}
