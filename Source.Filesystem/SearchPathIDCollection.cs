// TODO: Logging calls when things go wrong, ie. try/catches

namespace Source.FileSystem;

public class SearchPathIDCollection : Dictionary<ulong, SearchPathCollection>
{
	List<ulong> pathOrder = [];
	/// <summary>
	/// 
	/// </summary>
	/// <param name="pathID"></param>
	/// <param name="collection"></param>
	/// <returns>True if the collection was created, false if it already existed.</returns>
	public bool OpenOrCreateCollection(in ReadOnlySpan<char> pathID, out SearchPathCollection collection) {
		ulong hashID = pathID.Hash();
		if (TryGetValue(hashID, out var c)) {
			collection = c;
			return false;
		}

		collection = new();
		this[hashID] = collection;
		pathOrder.Add(hashID);
		return true;
	}

	public new bool Remove(in ReadOnlySpan<char> pathID) {
		ulong hashID = pathID.Hash();

		base.Remove(hashID);
		return pathOrder.Remove(hashID);
	}

	public new void Clear() {
		base.Clear();
		pathOrder.Clear();
	}
}
