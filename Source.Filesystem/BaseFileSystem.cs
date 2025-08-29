// TODO: Logging calls when things go wrong, ie. try/catches


using CommunityToolkit.HighPerformance;

using Source.Common.Filesystem;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace Source.FileSystem;

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
	public void SetPath(ReadOnlySpan<char> diskPath) {
		DiskPath = new(diskPath);
	}

	public abstract bool Exists(ReadOnlySpan<char> path); // Returns if the file or directory exists
	public abstract bool IsDirectory(ReadOnlySpan<char> path); // Returns true if the path is a directory
	public abstract bool IsFileWritable(ReadOnlySpan<char> path); // Returns true if the path can be written to
	public abstract IFileHandle? Open(ReadOnlySpan<char> path, FileOpenOptions options); // Can return null if something went wrong
	public abstract bool RemoveFile(ReadOnlySpan<char> path); // Return true if the file was deleted
	public abstract bool RenameFile(ReadOnlySpan<char> oldPath, ReadOnlySpan<char> newPath); // Renames a single file, returns true if it worked
	public abstract bool SetFileWritable(ReadOnlySpan<char> path, bool writable); // Determines if the file is writable
	public abstract long Size(ReadOnlySpan<char> path); // Gets the size of a file
	/// <summary>
	/// Gets the last modified time of a file (UTC)
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public abstract DateTime Time(ReadOnlySpan<char> path);
	internal abstract ReadOnlySpan<char> GetPathString();
	internal abstract object? GetPackFile();
	internal abstract object? GetPackedStore();
}

public class SearchPathCollection : List<SearchPath>
{
	/// <summary>
	/// Defines whether the search path ID is searchable when pathID == null in queries.
	/// </summary>
	public bool RequestOnly { get; set; } = false;
}

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


public class DiskSearchPath : SearchPath
{
	private IBaseFileSystem parent;
	public DiskSearchPath(IBaseFileSystem filesystem, string absPath) {
		parent = filesystem;

		if (!Path.IsPathFullyQualified(absPath))
			absPath = Path.GetFullPath(absPath);

		SetPath(absPath);
	}

	private string GetAbsPath(ReadOnlySpan<char> relPath) => Path.Combine(DiskPath!, new(relPath));

	public override bool Exists(ReadOnlySpan<char> path) => Path.Exists(GetAbsPath(path));
	public override bool IsDirectory(ReadOnlySpan<char> path) => Directory.Exists(GetAbsPath(path));

	public override bool IsFileWritable(ReadOnlySpan<char> path) {
		var info = new FileInfo(GetAbsPath(path));
		return info.Exists && !info.IsReadOnly;
	}

	public override IFileHandle? Open(ReadOnlySpan<char> path, FileOpenOptions options) {
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

	public override bool RemoveFile(ReadOnlySpan<char> path) {
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

	public override bool RenameFile(ReadOnlySpan<char> oldPath, ReadOnlySpan<char> newPath) {
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
	public override bool SetFileWritable(ReadOnlySpan<char> path, bool writable) => false;

	public override long Size(ReadOnlySpan<char> path) {
		var absPath = GetAbsPath(path);
		var info = new FileInfo(absPath);
		if (!info.Exists) return -1;

		return info.Length;
	}

	public override DateTime Time(ReadOnlySpan<char> path) {
		var absPath = GetAbsPath(path);
		var info = new FileInfo(absPath);
		if (!info.Exists) return DateTime.UnixEpoch;

		return info.LastWriteTimeUtc;
	}

	internal override ReadOnlySpan<char> GetPathString() => DiskPath;

	internal override object? GetPackFile() {
		return null;
	}

	internal override object? GetPackedStore() {
		return null;
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

	private void AddMapPackFile(ReadOnlySpan<char> path, ReadOnlySpan<char> pathID, SearchPathAdd addType) => throw new NotImplementedException();
	private void AddVPKFile(ReadOnlySpan<char> path, ReadOnlySpan<char> pathID, SearchPathAdd addType) => throw new NotImplementedException();
	private void AddPackFiles(ReadOnlySpan<char> path, ReadOnlySpan<char> pathID, SearchPathAdd addType) { } // TODO 
	private void AddSeparatorAndFixPath(ref string path) { // this sucks fix it later
		path = (path.TrimEnd('\\').TrimEnd('/') + "/").Replace("\\", "/");
	}
	private void AddSearchPathInternal(ReadOnlySpan<char> path, ReadOnlySpan<char> pathID, SearchPathAdd addType, bool addPackFiles) {
		var ext = Path.GetExtension(path);

		switch (ext) {
			case ".bsp": AddMapPackFile(path, pathID, addType); return;
			case ".vpk": AddVPKFile(path, pathID, addType); return;
		}

		string newPath = Path.IsPathFullyQualified(path) ? new(path) : Path.GetFullPath(new(path));
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

	public void AddSearchPath(ReadOnlySpan<char> path, ReadOnlySpan<char> pathID, SearchPathAdd addType = SearchPathAdd.ToTail) {
		AddSearchPathInternal(path, pathID, addType, true);
	}

	public IEnumerable<SearchPath> GetCollections(ulong hashID) {
		if (hashID == 0) {
			foreach (var path in SearchPaths.Values)
				if (!path.RequestOnly)
					foreach (var searchPath in path)
						yield return searchPath;
		}
		else {
			if (!SearchPaths.TryGetValue(hashID, out var collection))
				yield break;

			foreach (var searchPath in collection)
				yield return searchPath;
		}
	}
	delegate T FileSystemFuncPost<T>(SearchPath searchPath, ReadOnlySpan<char> providedFileName);
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
		ReadOnlySpan<char> filename,
		ReadOnlySpan<char> pathID,
		FileSystemFuncPost<T> func,
		Func<T, bool> winCondition,
		T? loseDefault,
		[NotNullWhen(true)] out SearchPath? winner
	) {
		ulong hashID = pathID.Hash();
		foreach (var path in GetCollections(hashID)) {
			T? ret = func(path, filename);
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

	public bool IsDirectory(ReadOnlySpan<char> fileName, ReadOnlySpan<char> pathID) {
		return FirstToThePost(fileName, pathID, (path, filename) => path.IsDirectory(filename), boolWin, false, out _);
	}
	public bool IsFileWritable(ReadOnlySpan<char> fileName, ReadOnlySpan<char> pathID) {
		return FirstToThePost(fileName, pathID, (path, filename) => path.IsFileWritable(filename), boolWin, false, out _);
	}

	public void MarkPathIDByRequestOnly(ReadOnlySpan<char> pathID, bool requestOnly) {
		ulong hashID = pathID.Hash();

		if (!SearchPaths.TryGetValue(hashID, out var collection))
			return;

		collection.RequestOnly = requestOnly;
	}

	public FileSystemMountRetval MountSteamContent(long extraAppID = -1) {
		throw new NotImplementedException(); // todo
	}

	public IFileHandle? Open(ReadOnlySpan<char> fileName, FileOpenOptions options, ReadOnlySpan<char> pathID) {
		return FirstToThePost(fileName, pathID, (path, filename) => path.Open(filename, options), notNullWin, null, out _);
	}


	public void RemoveAllSearchPaths() {
		SearchPaths.Clear();
	}

	public bool RemoveFile(ReadOnlySpan<char> relativePath, ReadOnlySpan<char> pathID) {
		string fn = new(relativePath);
		return FirstToThePost(relativePath, pathID, (path, filename) => path.RemoveFile(filename), boolWin, false, out _);
	}
	public bool RemoveSearchPath(ReadOnlySpan<char> path, ReadOnlySpan<char> pathID) {
		ulong hash = pathID.Hash();
		if (hash == 0) return false;

		if (!SearchPaths.TryGetValue(hash, out var collection))
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

	public void RemoveSearchPaths(ReadOnlySpan<char> pathID) {
		ulong hash = pathID.Hash();
		if (hash == 0) return;
		SearchPaths.Remove(hash);
	}

	public unsafe bool RenameFile(ReadOnlySpan<char> oldPath, ReadOnlySpan<char> newPath, ReadOnlySpan<char> pathID) {
		int newPathLength = newPath.Length;
		fixed (char* nPath = newPath) {
			char** reallyBadHack = &nPath;
			return FirstToThePost(oldPath, pathID, (path, filename) => path.RenameFile(filename, new(*reallyBadHack, newPathLength)), boolWin, false, out _);
		}
	}

	public bool SetFileWritable(ReadOnlySpan<char> fileName, bool writable, ReadOnlySpan<char> pathID) {
		return FirstToThePost(fileName, pathID, (path, filename) => path.SetFileWritable(filename, writable), boolWin, false, out _);
	}

	public long Size(ReadOnlySpan<char> fileName, ReadOnlySpan<char> pathID) {
		string fn = new(fileName);
		return FirstToThePost(fileName, pathID, (path, filename) => path.Size(filename), (v) => v != -1, -1, out _);
	}

	public DateTime Time(ReadOnlySpan<char> fileName, ReadOnlySpan<char> pathID) {
		string fn = new(fileName);
		return FirstToThePost(fileName, pathID, (path, filename) => path.Time(filename), (v) => v != DateTime.UnixEpoch, DateTime.UnixEpoch, out _);
	}

	public bool FileExists(ReadOnlySpan<char> fileName, ReadOnlySpan<char> pathID) {
		string fn = new(fileName);
		return FirstToThePost(fileName, pathID, (path, filename) => path.Exists(filename), boolWin, false, out _);
	}

	public void CreateDirHierarchy(ReadOnlySpan<char> relativePath, ReadOnlySpan<char> pathID) {
		Span<char> scratchFileName = stackalloc char[MAX_PATH];
		if (!Path.IsPathFullyQualified(relativePath)) {
			Assert(pathID != null);
			ComputeFullWritePath(scratchFileName, relativePath, pathID);
		}
		else {
			relativePath.CopyTo(scratchFileName);
		}
		Directory.CreateDirectory(new(scratchFileName));
	}
	private SearchPath? FindWritePath(ReadOnlySpan<char> filename, ReadOnlySpan<char> pathID) {
		ulong hash = pathID.Hash();
		if (hash == 0) return null;

		foreach (var searchPaths in SearchPaths) {
			foreach (var searchPath in searchPaths.Value) {
				if (searchPath.GetPackFile() != null || searchPath.GetPackedStore() != null)
					continue;

				if (pathID == null || searchPaths.Key == hash)
					return searchPath;
			}
		}
		return null;
	}
	private ReadOnlySpan<char> GetWritePath(ReadOnlySpan<char> filename, ReadOnlySpan<char> pathID) {
		SearchPath? searchPath = null;
		if (pathID != null && pathID.Length > 0) {
			if (pathID.Equals("game", StringComparison.OrdinalIgnoreCase))
				searchPath = FindWritePath(filename, "game_write");
			else if (pathID.Equals("game", StringComparison.OrdinalIgnoreCase))
				searchPath = FindWritePath(filename, "mod_write");

			searchPath ??= FindWritePath(filename, pathID);
			if (searchPath != null)
				return searchPath.GetPathString();

			Warning("Requested non-existent write path %s!\n", new string(pathID));
		}

		searchPath = FindWritePath(filename, "DEFAULT_WRITE_PATH");
		if (searchPath != null) return searchPath.GetPathString();

		searchPath = FindWritePath(filename, null);
		if (searchPath != null) return searchPath.GetPathString();

		// Hope this is reasonable!!
		return "./";
	}

	private void ComputeFullWritePath(Span<char> dest, ReadOnlySpan<char> relativePath, ReadOnlySpan<char> pathID) {
		string combined = Path.Combine(new(GetWritePath(relativePath, pathID)), new(relativePath));
		combined.AsSpan().CopyTo(dest);
	}

	public bool ReadFile(ReadOnlySpan<char> fileName, ReadOnlySpan<char> path, Span<byte> buf, int startingByte) {
		using var handle = Open(fileName, FileOpenOptions.Read, path);
		if (handle == null) return false;

		int bytes = handle.Stream.Read(buf[startingByte..]);
		return bytes > 0;
	}

	public bool ReadFile(ReadOnlySpan<char> fileName, ReadOnlySpan<char> path, Span<char> buf, int startingByte) {
		throw new Exception();
	}
}