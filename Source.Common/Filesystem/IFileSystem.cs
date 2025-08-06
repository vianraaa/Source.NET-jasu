namespace Source.Common.Filesystem;

public interface IBaseFileSystem
{
	/// <summary>
	/// Tries to open the file. May return null.
	/// </summary>
	/// <param name="fileName">The file name.</param>
	/// <param name="options">File options.<br/><code>
	/// |==============================================|
	/// | r  | Read                                    |
	/// | w  | Read                                    |
	/// | a  | Read                                    |
	/// | +  | Extended                                |
	/// | b  | Binary                                  |
	/// | n  | Text                                    |
	/// |==============================================|
	/// | r+ | Read   + Extended (or just ReadEx)      |
	/// | w+ | Write  + Extended (or just WriteEx)     |
	/// | a+ | Append + Extended (or just AppendEx)    |
	/// |==============================================|
	/// </code></param>
	/// <param name="pathID"></param>
	/// <returns></returns>
	public IFileHandle? Open(string fileName, FileOpenOptions options, string? pathID = null);
	/// <summary>
	/// Checks if the file exists.
	/// </summary>
	/// <param name="fileName">The file name.</param>
	/// <param name="pathID">The search path ID.</param>
	/// <returns>True if the file exists, and vice versa.</returns>
	public bool Exists(string fileName, string? pathID = null);
	/// <summary>
	/// Checks if the file is writable.
	/// </summary>
	/// <param name="fileName">The file name.</param>
	/// <param name="pathID">The search path ID.</param>
	/// <returns>True if the file is writable, and vice versa.</returns>
	public bool IsFileWritable(string fileName, string? pathID = null);
	/// <summary>
	/// Tries to set the file as writable.
	/// </summary>
	/// <param name="fileName">The file name.</param>
	/// <param name="writable">Is it writable?</param>
	/// <param name="pathID">The search path ID.</param>
	/// <returns>True if the operation succeded, and false if it didn't.</returns>
	public bool SetFileWritable(string fileName, bool writable, string? pathID = null);
	public long Size(string fileName, string? pathID = null);
	public DateTime Time(string fileName, string? pathID = null);
}

public interface IFileSystem : IBaseFileSystem
{
	public FileSystemMountRetval MountSteamContent(long extraAppID = -1);
	/// <summary>
	/// Add a search path.
	/// </summary>
	/// <param name="path"></param>
	/// <param name="pathID"></param>
	/// <param name="addType"></param>
	public void AddSearchPath(string path, string pathID, SearchPathAdd addType = SearchPathAdd.ToTail);
	/// <summary>
	/// Remove a search path.
	/// </summary>
	/// <param name="path"></param>
	/// <param name="pathID"></param>
	public bool RemoveSearchPath(string path, string pathID);
	/// <summary>
	/// Remove all search paths.
	/// </summary>
	public void RemoveAllSearchPaths();
	/// <summary>
	/// Remove all search paths associated with a given path ID.
	/// </summary>
	/// <param name="pathID"></param>
	public void RemoveSearchPaths(string pathID);

	/// <summary>
	/// Marks a path ID by request only, which means files inside of it will only be accessed if the path ID is specifically requested.<br/>
	/// Otherwise, it will be ignored (in the case of global lookups without a path ID). <br/><br/>
	/// <b>NOTE</b>: <i>If there are currently no search paths with this path ID, then it will still remember it for later if you add other search paths with that path ID.</i>
	/// </summary>
	/// <param name="pathID"></param>
	/// <param name="requestOnly"></param>
	public void MarkPathIDByRequestOnly(string pathID, bool requestOnly);

	public bool RemoveFile(string relativePath, string? pathID = null);
	public bool RenameFile(string oldPath, string newPath, string? pathID = null);
	public bool IsDirectory(string fileName, string? pathID = null);
}