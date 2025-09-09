using Source.Common.Filesystem;
using Source.FileSystem;
using Source.Formats.VPK;


namespace Source.Filesystem;

public class PackStoreSearchPath : SearchPath
{
	private readonly IBaseFileSystem parent;
	private readonly VpkArchive vpk;
	public PackStoreSearchPath(IBaseFileSystem filesystem, string absPath) {
		absPath = absPath.EndsWith(".vpk") ? absPath.Substring(0, absPath.Length - ".vpk".Length) : absPath;
		absPath = absPath.EndsWith("_dir") ? absPath.Substring(0, absPath.Length - "_dir".Length) : absPath;
		absPath = $"{absPath}_dir.vpk";
		parent = filesystem;
		vpk = new VpkArchive();
		vpk.Load(absPath);

		if (!Path.IsPathFullyQualified(absPath))
			absPath = Path.GetFullPath(absPath);

		SetPath(absPath);
	}

	public override bool Exists(ReadOnlySpan<char> path) {
		throw new NotImplementedException();
	}

	public override bool IsDirectory(ReadOnlySpan<char> path) {
		throw new NotImplementedException();
	}

	public override bool IsFileWritable(ReadOnlySpan<char> path) {
		throw new NotImplementedException();
	}

	public override IFileHandle? Open(ReadOnlySpan<char> path, FileOpenOptions options) {
		throw new NotImplementedException();
	}

	public override bool RemoveFile(ReadOnlySpan<char> path) {
		throw new NotImplementedException();
	}

	public override bool RenameFile(ReadOnlySpan<char> oldPath, ReadOnlySpan<char> newPath) {
		throw new NotImplementedException();
	}

	public override bool SetFileWritable(ReadOnlySpan<char> path, bool writable) {
		throw new NotImplementedException();
	}

	public override long Size(ReadOnlySpan<char> path) {
		throw new NotImplementedException();
	}

	public override DateTime Time(ReadOnlySpan<char> path) {
		throw new NotImplementedException();
	}

	internal override object? GetPackedStore() {
		throw new NotImplementedException();
	}

	internal override object? GetPackFile() {
		throw new NotImplementedException();
	}

	internal override ReadOnlySpan<char> GetPathString() {
		throw new NotImplementedException();
	}
}
