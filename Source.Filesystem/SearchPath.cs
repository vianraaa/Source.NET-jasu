// TODO: Logging calls when things go wrong, ie. try/catches


using Source.Common.Filesystem;

namespace Source.FileSystem;

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

	internal virtual ReadOnlySpan<char> Concat(ReadOnlySpan<char> fileNameUnnormalized, Span<char> target) {
		Span<char> fileNameNormalized = stackalloc char[MAX_PATH];
		ReadOnlySpan<char> fileName = BaseFileSystem.Normalize(fileNameUnnormalized, fileNameNormalized);

		int writePtr = 0;
		string diskpath = DiskPath ?? "";
		diskpath.CopyTo(target[writePtr..]); writePtr += diskpath.Length;
		if (diskpath.EndsWith('\\'))
			target[writePtr - 1] = '/';

		bool hasSlash = target[writePtr - 1] == '/';
		if (!hasSlash) {
			// Write a slash now
			target[writePtr] = '/'; writePtr++;
			hasSlash = true;
		}
		// Confirm we arent writing another slash
		if ((fileName.Length > 0 && fileName[0] == '/' || fileName[0] == '\\') && hasSlash)
			fileName = fileName[1..];

		fileName.ClampedCopyTo(target[writePtr..]); writePtr+= fileName.Length;
		return target[..writePtr];
	}
}
