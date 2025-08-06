// TODO: Logging calls when things go wrong, ie. try/catches


using Source.Common.Filesystem;

using System.Diagnostics.CodeAnalysis;

namespace Nucleus.FileSystem;

public class DiskFileHandle(IBaseFileSystem filesystem, FileStream data) : IFileHandle, IDisposable
{
	private bool disposedValue;

	public Stream Stream => data;

	protected virtual void Dispose(bool disposing) {
		if (!disposedValue && disposing)
			data.Dispose();
	}

	public void Dispose() {
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public bool IsOK() => !disposedValue && data != null;
}

public abstract class SearchPath
{
	public string? DiskPath { get; private set; }
	public void SetPath(string diskPath) {
		DiskPath = diskPath;
	}

	public abstract bool Exists(string path); // Returns if the file or directory exists
	public abstract bool IsDirectory(string path); // Returns true if the path is a directory
	public abstract bool IsFileWritable(string path); // Returns true if the path can be written to
	public abstract IFileHandle? Open(string path, FileOpenOptions options); // Can return null if something went wrong
	public abstract bool RemoveFile(string path); // Return true if the file was deleted
	public abstract bool RenameFile(string oldPath, string newPath); // Renames a single file, returns true if it worked
	public abstract bool SetFileWritable(string path, bool writable); // Determines if the file is writable
	public abstract long Size(string path); // Gets the size of a file
	/// <summary>
	/// Gets the last modified time of a file (UTC)
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public abstract DateTime Time(string path);
}

public class SearchPathCollection : List<SearchPath>
{
	/// <summary>
	/// Defines whether the search path ID is searchable when pathID == null in queries.
	/// </summary>
	public bool RequestOnly { get; set; } = false;
}

public class SearchPathIDCollection : Dictionary<string, SearchPathCollection>
{
	List<string> pathOrder = [];
	/// <summary>
	/// 
	/// </summary>
	/// <param name="pathID"></param>
	/// <param name="collection"></param>
	/// <returns>True if the collection was created, false if it already existed.</returns>
	public bool OpenOrCreateCollection(string pathID, out SearchPathCollection collection) {
		if (TryGetValue(pathID, out var c)) {
			collection = c;
			return false;
		}

		collection = new();
		this[pathID] = collection;
		pathOrder.Add(pathID);
		return true;
	}

	public new bool Remove(string pathID) {
		base.Remove(pathID);
		return pathOrder.Remove(pathID);
	}

	public new void Clear() {
		base.Clear();
		pathOrder.Clear();
	}

	/// <summary>
	/// Enumerates entries in the order of pathOrder.
	/// </summary>
	public new IEnumerator<KeyValuePair<string, SearchPathCollection>> GetEnumerator() {
		foreach (var key in pathOrder) {
			if (TryGetValue(key, out var value))
				yield return new KeyValuePair<string, SearchPathCollection>(key, value);
		}
	}
}


public class DiskSearchPath : SearchPath
{
	private IBaseFileSystem parent;
	public DiskSearchPath(IBaseFileSystem filesystem, string absPath) {
		parent = filesystem;

		if (!Path.IsPathFullyQualified(absPath))
			absPath = Path.GetFullPath(absPath);

		SetPath(absPath);
	}

	private string GetAbsPath(string relPath) => Path.Combine(DiskPath!, relPath);

	public override bool Exists(string path) => Path.Exists(GetAbsPath(path));
	public override bool IsDirectory(string path) => Directory.Exists(GetAbsPath(path));

	public override bool IsFileWritable(string path) {
		var info = new FileInfo(GetAbsPath(path));
		return info.Exists && !info.IsReadOnly;
	}

	public override IFileHandle? Open(string path, FileOpenOptions options) {
		string absPath = GetAbsPath(path);
		var info = new FileInfo(absPath);

		// Scram early if the file doesn't even exist
		if (!info.Exists) return null;

		// Check file options for invalid access
		FileOpenOptions operation = options.GetOperation();
		if (operation == FileOpenOptions.Write && info.IsReadOnly)
			return null;

		// Open the file stream
		FileMode mode = operation switch {
			FileOpenOptions.Read => FileMode.Open,
			FileOpenOptions.Write => FileMode.Create,
			FileOpenOptions.Append => FileMode.Append,
			_ => throw new NotSupportedException()
		};

		FileAccess access = options.Extended() ? FileAccess.ReadWrite : operation switch {
			FileOpenOptions.Read => FileAccess.Read,
			FileOpenOptions.Write => FileAccess.Write,
			FileOpenOptions.Append => FileAccess.Write,
			_ => throw new NotSupportedException()
		};

		try {
			return new DiskFileHandle(parent, File.Open(absPath, mode, access));
		}
		catch {
			return null;
		}
	}

	public override bool RemoveFile(string path) {
		var absPath = GetAbsPath(path);
		var info = new FileInfo(absPath);
		if (!info.Exists) return false;
		if (info.IsReadOnly) return false;

		try {
			info.Delete();
		}
		catch {
			return false;
		}

		return true;
	}

	public override bool RenameFile(string oldPath, string newPath) {
		var absPath = GetAbsPath(oldPath);
		var info = new FileInfo(absPath);
		if (!info.Exists) return false;
		if (info.IsReadOnly) return false;

		try {
			info.MoveTo(GetAbsPath(newPath));
		}
		catch {
			return false;
		}

		return true;
	}

	// Do nothing
	public override bool SetFileWritable(string path, bool writable) => false;

	public override long Size(string path) {
		var absPath = GetAbsPath(path);
		var info = new FileInfo(absPath);
		if (!info.Exists) return -1;

		return info.Length;
	}

	public override DateTime Time(string path) {
		var absPath = GetAbsPath(path);
		var info = new FileInfo(absPath);
		if (!info.Exists) return DateTime.UnixEpoch;

		return info.LastWriteTimeUtc;
	}
}

// Maybe we redo this one day...
public class BaseFileSystem : IFileSystem
{
	private SearchPathIDCollection SearchPaths = [];

	public BaseFileSystem() {
		RemoveSearchPaths("EXECUTABLE_PATH");
		AddSearchPath(System.Reflection.Assembly.GetExecutingAssembly().Location, "EXECUTABLE_PATH");
		AddSearchPath(AppContext.BaseDirectory, "BASE_PATH");
	}

	private void AddMapPackFile(string path, string pathID, SearchPathAdd addType) => throw new NotImplementedException();
	private void AddVPKFile(string path, string pathID, SearchPathAdd addType) => throw new NotImplementedException();
	private void AddPackFiles(string path, string pathID, SearchPathAdd addType) { } // TODO 
	private void AddSeparatorAndFixPath(ref string path) { // this sucks fix it later
		path = (path.TrimEnd('\\').TrimEnd('/') + "/").Replace("\\", "/");
	}
	private void AddSearchPathInternal(string path, string pathID, SearchPathAdd addType, bool addPackFiles) {
		var ext = Path.GetExtension(path);

		switch (ext) {
			case ".bsp": AddMapPackFile(path, pathID, addType); return;
			case ".vpk": AddVPKFile(path, pathID, addType); return;
		}

		string newPath = Path.IsPathFullyQualified(path) ? path : Path.GetFullPath(path);
		AddSeparatorAndFixPath(ref newPath);

		if (!SearchPaths.OpenOrCreateCollection(pathID, out SearchPathCollection collection)) {
			for (int i = 0, c = collection.Count; i < c; i++) {
				var searchPath = collection[i];
				if (searchPath.DiskPath == newPath) {
					if ((addType == SearchPathAdd.ToHead && i == 0) || addType == SearchPathAdd.ToTail)
						return;
					else {
						collection.RemoveAt(i);
						i--;
						c--;
						break;
					}
				}
			}
		}

		if (addPackFiles) {
			AddPackFiles(newPath, pathID, addType);
		}

		if (addType == SearchPathAdd.ToHead)
			collection.Insert(0, new DiskSearchPath(this, newPath));
		else
			collection.Add(new DiskSearchPath(this, newPath));

	}

	public void AddSearchPath(string path, string pathID, SearchPathAdd addType = SearchPathAdd.ToTail) {
		AddSearchPathInternal(path, pathID, addType, true);
	}

	public IEnumerable<SearchPath> GetCollections(string? pathID) {
		if (pathID == null) {
			foreach (var path in SearchPaths.Values)
				if (!path.RequestOnly)
					foreach (var searchPath in path)
						yield return searchPath;
		}
		else {
			if (!SearchPaths.TryGetValue(pathID, out var collection))
				yield break;

			foreach (var searchPath in collection)
				yield return searchPath;
		}
	}
	/// <summary>
	/// Iterates through all <see cref="SearchPathCollection"/>'s (or a single lookup if pathID != null), and returns the first time <paramref name="winCondition"/> returns true.
	/// <br/> 
	/// If nothing returns true, the method returns <see cref="loseDefault"/>.
	/// </summary>
	/// <param name="filename">A local-to-searchpath filename</param>
	/// <param name="pathID">A pathID. If null, will search through every <see cref="SearchPathCollection"/>; otherwise searches for the single collection in the <see cref="SearchPaths"/> lookup table.</param>
	/// <param name="func">A delegate to run on every <see cref="SearchPath"/></param>
	/// <param name="winCondition">Compares the return value from the search path. Return true if the search path won.</param>
	/// <param name="loseDefault">If no search paths won, then this value is returned.</param>
	/// <param name="winner">The <see cref="SearchPath"/> that won (if the method returns true)</param>
	/// <returns>True if a <see cref="SearchPath"/> won.</returns>
	private T? FirstToThePost<T>(
		string filename, 
		string? pathID, 
		Func<SearchPath, T> func, 
		Func<T, bool> winCondition, 
		T? loseDefault, 
		[NotNullWhen(true)] out SearchPath? winner
	) {
		foreach (var path in GetCollections(pathID)) {
			T? ret = func(path);
			if (winCondition(ret)) {
				winner = path;
				return ret;
			}
		}
		winner = null;
		return loseDefault;
	}

	private static bool boolWin(bool inp) => inp;
	private static bool notNullWin<T>(T? v) => v != null;

	public bool Exists(string fileName, string? pathID = null)
		=> FirstToThePost(fileName, pathID, (path) => path.Exists(fileName), boolWin, false, out _);
	public bool IsDirectory(string fileName, string? pathID = null)
		=> FirstToThePost(fileName, pathID, (path) => path.IsDirectory(fileName), boolWin, false, out _);
	public bool IsFileWritable(string fileName, string? pathID = null)
		=> FirstToThePost(fileName, pathID, (path) => path.IsFileWritable(fileName), boolWin, false, out _);

	public void MarkPathIDByRequestOnly(string pathID, bool requestOnly) {
		if (!SearchPaths.TryGetValue(pathID, out var collection))
			return;

		collection.RequestOnly = requestOnly;
	}

	public FileSystemMountRetval MountSteamContent(long extraAppID = -1) {
		throw new NotImplementedException(); // todo
	}

	public IFileHandle? Open(string fileName, FileOpenOptions options, string? pathID = null)
		=> FirstToThePost(fileName, pathID, (path) => path.Open(fileName, options), notNullWin, null, out _);


	public void RemoveAllSearchPaths() {
		SearchPaths.Clear();
	}

	public bool RemoveFile(string relativePath, string? pathID = null)
		=> FirstToThePost(relativePath, pathID, (path) => path.RemoveFile(relativePath), boolWin, false, out _);

	public bool RemoveSearchPath(string path, string pathID) {
		if (!SearchPaths.TryGetValue(pathID, out var collection))
			return false;

		bool ret = false;

		for (int i = collection.Count - 1; i >= 0; i--) {
			if (collection[i].DiskPath != path)
				continue;
			collection.RemoveAt(i);
			ret = true;
		}

		return ret;
	}

	public void RemoveSearchPaths(string pathID) {
		SearchPaths.Remove(pathID);
	}

	public bool RenameFile(string oldPath, string newPath, string? pathID = null)
		=> FirstToThePost(oldPath, pathID, (path) => path.RenameFile(oldPath, newPath), boolWin, false, out _);

	public bool SetFileWritable(string fileName, bool writable, string? pathID = null)
		=> FirstToThePost(fileName, pathID, (path) => path.SetFileWritable(fileName, writable), boolWin, false, out _);

	public long Size(string fileName, string? pathID = null)
		=> FirstToThePost(fileName, pathID, (path) => path.Size(fileName), (v) => v != -1, -1, out _);

	public DateTime Time(string fileName, string? pathID = null)
		=> FirstToThePost(fileName, pathID, (path) => path.Time(fileName), (v) => v != DateTime.UnixEpoch, DateTime.UnixEpoch, out _);
}