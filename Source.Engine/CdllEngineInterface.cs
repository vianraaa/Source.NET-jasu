using Source.Common.Client;
using Source.Engine.Client;
using Source.Engine.Server;

namespace Source.Engine;

public class EngineClient(ClientState cl, GameServer sv) : IEngineClient
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
}
