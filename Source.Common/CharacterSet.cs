namespace Source;

public class CharacterSet : HashSet<char>
{
	public CharacterSet() { }
	public CharacterSet(ReadOnlySpan<char> chars) {
		foreach (var c in chars)
			Add(c);
	}
}
