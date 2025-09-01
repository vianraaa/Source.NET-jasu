using Source.Common;

namespace Source.GUI;

/// <summary>
/// Temporary localize stub until ready to impl ILocalize
/// </summary>
public class LocalizeStub : ILocalize
{
	public bool AddFile(ReadOnlySpan<char> fileName, ReadOnlySpan<char> pathID = default, bool includeFallbackSearchPaths = false) {
		return true;
	}

	public ReadOnlySpan<char> Find(ReadOnlySpan<char> tokenName) {
		return tokenName;
	}
}
