namespace Source.Common;

public interface ILocalize
{
	bool AddFile(ReadOnlySpan<char> fileName, ReadOnlySpan<char> pathID = default, bool includeFallbackSearchPaths = false);
	ReadOnlySpan<char> Find(ReadOnlySpan<char> text);
	ulong FindIndex(ReadOnlySpan<char> value);
	ReadOnlySpan<char> GetValueByIndex(ulong hash);
}
