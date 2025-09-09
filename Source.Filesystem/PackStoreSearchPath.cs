using Source.Common.Filesystem;
using Source.FileSystem;
using Source.Formats.VPK;


namespace Source.Filesystem;

public class VpkFileHandle(IBaseFileSystem filesystem, MemoryStream data) : IFileHandle, IDisposable
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


public class PackStoreSearchPath : SearchPath
{
	private readonly IBaseFileSystem parent;
	private readonly VpkArchive vpk;

	private Dictionary<UtlSymId_t, VpkEntry> vpkEntryLookups = [];
	private Dictionary<UtlSymId_t, VpkDirectory> vpkDirectoryLookups = [];

	public PackStoreSearchPath(IBaseFileSystem filesystem, string absPath) {
		absPath = absPath.EndsWith(".vpk") ? absPath.Substring(0, absPath.Length - ".vpk".Length) : absPath;
		absPath = absPath.EndsWith("_dir") ? absPath.Substring(0, absPath.Length - "_dir".Length) : absPath;
		absPath = $"{absPath}_dir.vpk";
		parent = filesystem;
		vpk = new VpkArchive();
		vpk.Load(absPath);

		Span<char> buildPath = stackalloc char[260];
		foreach(var dir in vpk.Directories) {
			vpkDirectoryLookups[dir.Path.Hash()] = dir;

			foreach (var entry in dir.Entries) {
				var path = entry.Path;
				var filename = entry.Filename;
				var ext = entry.Extension;

				int strlen = 0;
				if (path.Length > 0) {
					path.CopyTo(buildPath[strlen..]); strlen += path.Length;
					buildPath[strlen] = '/'; strlen += 1;
				}
				filename.CopyTo(buildPath[strlen..]); strlen += filename.Length;
				buildPath[strlen] = '/'; strlen += 1;
				ext.CopyTo(buildPath[strlen..]); strlen += filename.Length;

				ReadOnlySpan<char> finalSpan = buildPath[..strlen];
				vpkEntryLookups[finalSpan.Hash()] = entry;
			}
		}

		if (!Path.IsPathFullyQualified(absPath))
			absPath = Path.GetFullPath(absPath);

		SetPath(absPath);
	}

	public override bool Exists(ReadOnlySpan<char> path) {
		ulong hash = path.Hash();
		return vpkEntryLookups.ContainsKey(hash) || vpkDirectoryLookups.ContainsKey(hash);
	}

	public override bool IsDirectory(ReadOnlySpan<char> path) {
		return vpkDirectoryLookups.ContainsKey(path.Hash());
	}

	public override bool IsFileWritable(ReadOnlySpan<char> path) {
		return false;
	}

	public override IFileHandle? Open(ReadOnlySpan<char> path, FileOpenOptions options) {
		if(vpkEntryLookups.TryGetValue(path.Hash(), out VpkEntry? entry))
			return new VpkFileHandle(parent, new MemoryStream(entry.Data));
		
		return null;
	}

	public override bool RemoveFile(ReadOnlySpan<char> path) => false;

	public override bool RenameFile(ReadOnlySpan<char> oldPath, ReadOnlySpan<char> newPath) => false;
	public override bool SetFileWritable(ReadOnlySpan<char> path, bool writable) => false;

	public override long Size(ReadOnlySpan<char> path) {
		if (vpkEntryLookups.TryGetValue(path.Hash(), out VpkEntry? entry))
			return entry.EntryLength;
		return -1;
	}

	public override DateTime Time(ReadOnlySpan<char> path) {
		return DateTime.MinValue;
	}

	internal override object? GetPackedStore() {
		return null; // TODO: Review GetPackedStore again
	}

	internal override object? GetPackFile() {
		return null; // TODO: Review GetPackFile again
	}

	internal override ReadOnlySpan<char> GetPathString() {
		return DiskPath;
	}
}
