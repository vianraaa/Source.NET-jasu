using Source.Common.Client;

namespace Source.Engine;

internal class EngineClient : IEngineClient
{
	public ReadOnlySpan<char> Key_LookupBinding(ReadOnlySpan<char> binding) {
		return "";
	}
	public void GetUILanguage(Span<char> destination) {
		"english".CopyTo(destination);
	}
}
