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

	internal virtual ReadOnlySpan<char> Concat(ReadOnlySpan<char> fileName) => Path.Combine(DiskPath!, new(fileName));
}
