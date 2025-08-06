using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;

using static Source.Engine.COM;

namespace Source.Engine;

public struct CFSSearchPathsInit
{
	public string? DirectoryName;
	public string Language;
	public IFileSystem FileSystem;
	public bool MountHDContent;
	public bool LowViolence;
	public string ModPath;
}
public enum FSReturnCode
{
	OK,
	MissingGameInfoFile,
	InvalidGameInfoFile,
	InvalidParameters,
	UnableToInit,
	MissingSteamDLL,
}
/// <summary>
/// Internal engine filesystem initializer.
/// </summary>
public class FileSystem {
	FSReturnCode SetupFileSystemError(bool run, FSReturnCode ret, ReadOnlySpan<char> msg) {
		Dbg.Error($"{msg}\n");
		return ret;
	}
	public FSReturnCode LoadSearchPaths(in CFSSearchPathsInit initInfo) {
		if (initInfo.FileSystem == null || initInfo.DirectoryName == null)
			return SetupFileSystemError(false, FSReturnCode.InvalidParameters, "FileSystem.LoadSearchPaths: Invalid parameters specified.");

		KeyValues mainFile, fileSystemInfo, searchPaths;
		FSReturnCode retVal = LoadGameInfoFile(initInfo.DirectoryName, out mainFile, out fileSystemInfo, out searchPaths);
		if (retVal != FSReturnCode.OK)
			return retVal;

		string baseDir;
		if(GetBaseDir(out baseDir))
			return SetupFileSystemError(false, FSReturnCode.InvalidParameters, "FileSystem.GetBaseDir: failed.");

		const string GAMEINFOPATH_TOKEN = "|gameinfo_path|";
		const string BASESOURCEPATHS_TOKEN = "|all_source_engine_paths|";

		// todo: extraSearchPath.
		bool lowViolence = initInfo.LowViolence;
		bool firstGamePath = true;
		for(KeyValues cur = searchPaths.GetFirstSubKey())
	}

	private bool GetBaseDir(out string baseDir) {
		throw new NotImplementedException();
	}

	public FSReturnCode LoadGameInfoFile(string directoryName, out KeyValues mainFile, out KeyValues fileSystemInfo, out KeyValues searchPaths) {

	}
}
