// TODO: Logging calls when things go wrong, ie. try/catches


using CommunityToolkit.HighPerformance;

using Source.Common.Commands;
using Source.Common.Filesystem;
using Source.Common.Utilities;
using Source.Filesystem;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.JavaScript;

namespace Source.FileSystem;

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
	private void AddVPKFile(ReadOnlySpan<char> path, ReadOnlySpan<char> pathID, SearchPathAdd addType) {
		string newPath = Path.IsPathFullyQualified(path) ? new(path) : Path.GetFullPath(new(path));

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

		if (addType == SearchPathAdd.ToHead)
			collection.Insert(0, new PackStoreSearchPath(this, newPath));
		else
			collection.Add(new PackStoreSearchPath(this, newPath));
	}
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
	public ReadOnlySpan<char> RelativePathToFullPath(ReadOnlySpan<char> fileName, ReadOnlySpan<char> pathID, Span<char> dest, PathTypeFilter filter = PathTypeFilter.None) {
		if (!FirstToThePost(fileName, pathID, (path, filename) => path.Exists(filename), boolWin, false, out SearchPath? winner))
			return null;

		// TODO: If dest is provided, try writing to it instead of performing this concat?
		// Also TODO: rework this to be less garbage
		var concatted = winner.Concat(fileName);
		return concatted;
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
		Directory.CreateDirectory(new(scratchFileName.SliceNullTerminatedString()));
	}
	private SearchPath? FindWritePath(ReadOnlySpan<char> filename, ReadOnlySpan<char> pathID) {
		ulong hash = pathID.Hash();
		if (hash == 0) return null;

		foreach (var searchPaths in SearchPaths) {
			foreach (var searchPath in searchPaths.Value) {
				if (searchPath is not DiskSearchPath)
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

	public void GetLocalCopy(ReadOnlySpan<char> path) {

	}

	public void MarkAllCRCsUnverified() {
		// Todo
	}

	public ReadOnlySpan<char> WhereIsFile(ReadOnlySpan<char> fileName, ReadOnlySpan<char> pathID = default) {
		return FirstToThePost(fileName, pathID, (path, filename) => path.Exists(filename), boolWin, false, out SearchPath? path)
			? path.Concat(fileName) : null;
	}

	public void PrintSearchPaths() {
		Msg("---------------\n");
		Msg("Paths:\n");

		foreach (var searchpath in SearchPaths) {
			ReadOnlySpan<char> pathID = SearchPaths.GetName(searchpath.Key);
			foreach (var spi in searchpath.Value) {
				ReadOnlySpan<char> pack = "";
				ReadOnlySpan<char> type = "";
				if (false /* TODO: Map-based pack files */) {
					// type = "(map)";
				}
				else if (spi is PackStoreSearchPath pssp) {
					type = "(VPK)";
					pack = pssp.DiskPath;
				}
				else if(spi is DiskSearchPath dsp) {
					pack = dsp.DiskPath;
				}

				Msg($"\"{pack}\" \"{pathID}\" {type}\n");
			}
		}

	}
}