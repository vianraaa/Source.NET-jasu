using SharpCompress.Archives.Zip;

using Source.Common.Filesystem;
using Source.FileSystem;

namespace Source.Filesystem;

public class ZipArchiveEntryHandle(ZipArchiveEntry? entry) : IFileHandle
{
	Stream? stream = entry?.OpenEntryStream();
	public Stream Stream => stream!;

	public void Dispose() {
		throw new NotImplementedException();
	}

	public bool IsOK() => entry != null && stream != null;
}

public class ZipPackFileSearchPath : SearchPath
{
	readonly IFileSystem filesystem;
	readonly bool valid;
	readonly byte[] data;
	readonly ZipArchive archive;
	public bool IsValid() => valid;
	readonly Dictionary<FileNameHandle_t, ZipArchiveEntry> Entries = [];
	public ZipPackFileSearchPath(IFileSystem filesystem, string path, Stream stream, long offset, long length) {
		this.filesystem = filesystem;
		SetPath(path);
		data = new byte[length];
		stream.Seek(offset, SeekOrigin.Begin);
		stream.Read(data);

		// Unpack the zip
		archive = ZipArchive.Open(stream, new());
		// Create optimized lookup
		foreach(var entry in archive.Entries) {
			Entries[filesystem.FindOrAddFileName(entry.Key)] = entry;
		}
		valid = true;
	}
	private ZipArchiveEntry? getArchive(ReadOnlySpan<char> path) {
		if (Entries.TryGetValue(filesystem.FindOrAddFileName(path), out ZipArchiveEntry? entry))
			return entry;
		return null;
	}
	public override bool Exists(ReadOnlySpan<char> path) => getArchive(path) != null;

	public override bool IsDirectory(ReadOnlySpan<char> path) {
		return false;
	}

	public override bool IsFileWritable(ReadOnlySpan<char> path) => false;

	public override IFileHandle? Open(ReadOnlySpan<char> path, FileOpenOptions options) {
		ZipArchiveEntry? entry = getArchive(path);
		if (entry == null)
			return null;

		return new ZipArchiveEntryHandle(entry);
	}

	public override bool RemoveFile(ReadOnlySpan<char> path) => false;

	public override bool RenameFile(ReadOnlySpan<char> oldPath, ReadOnlySpan<char> newPath) => false;

	public override bool SetFileWritable(ReadOnlySpan<char> path, bool writable) => false;

	public override long Size(ReadOnlySpan<char> path) {
		if (Entries.TryGetValue(filesystem.FindOrAddFileName(path), out ZipArchiveEntry? entry))
			return entry.Size;
		return 0;
	}

	public override DateTime Time(ReadOnlySpan<char> path) {
		if (Entries.TryGetValue(filesystem.FindOrAddFileName(path), out ZipArchiveEntry? entry))
			return entry.LastModifiedTime ?? default;
		return default;
	}

	internal override object? GetPackedStore() => null;

	internal override object? GetPackFile() => null;

	internal override ReadOnlySpan<char> GetPathString() => this.DiskPath;
}
