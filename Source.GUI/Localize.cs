using Source.Common;

namespace Source.GUI;

public class Localize : ILocalize
{
	public bool AddFile(ReadOnlySpan<char> fileName, ReadOnlySpan<char> pathID = default, bool includeFallbackSearchPaths = false) {
		throw new NotImplementedException();
	}

	public ReadOnlySpan<char> Find(ReadOnlySpan<char> tokenName) {
		throw new NotImplementedException();
	}
}
