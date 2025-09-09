using Source.Common.Filesystem;
using Source.FileSystem;


namespace Source.Filesystem;

public class PackStoreSearchPath : SearchPath
{
	private IBaseFileSystem parent;
	public PackStoreSearchPath(IBaseFileSystem filesystem, string absPath) {
		parent = filesystem;

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
