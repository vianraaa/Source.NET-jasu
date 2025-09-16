// TODO: Logging calls when things go wrong, ie. try/catches


namespace Source.FileSystem;

public class SearchPathCollection : List<SearchPath>
{
	/// <summary>
	/// Defines whether the search path ID is searchable when pathID == null in queries.
	/// </summary>
	public bool RequestOnly { get; set; } = false;

	public SearchPath? At(int index) {
		if (index >= Count)
			return null;
		return this[index];
	}
}
