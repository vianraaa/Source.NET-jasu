using Source.Common.Formats.Keyvalues;
using Source.Common.Input;
using Source.Common.Launcher;

#if WIN32
using Microsoft.Win32;
#endif

namespace Source.SDLManager;

public unsafe class SDL3_System : ISystem
{
	public bool CommandLineParamExists(ReadOnlySpan<char> paramName) {
		throw new NotImplementedException();
	}

	public bool CreateShortcut(ReadOnlySpan<char> linkFileName, ReadOnlySpan<char> targetPath, ReadOnlySpan<char> arguments, ReadOnlySpan<char> workingDirectory, ReadOnlySpan<char> iconFile) {
		throw new NotImplementedException();
	}

	public bool DeleteRegistryKey(ReadOnlySpan<char> keyName) {
		throw new NotImplementedException();
	}

	public int GetAvailableDrives(Span<char> buf) {
		throw new NotImplementedException();
	}

	public int GetClipboardText(int offset, Span<char> buf) {
		throw new NotImplementedException();
	}

	public int GetClipboardTextCount() {
		throw new NotImplementedException();
	}

	public bool GetCommandLineParamValue(ReadOnlySpan<char> paramName, Span<char> value) {
		throw new NotImplementedException();
	}

	public double GetCurrentTime() {
		return Platform.Time;
	}

	public bool GetCurrentTimeAndDate(out int year, out int month, out int dayOfWeek, out int day, out int hour, out int minute, out int second) {
		throw new NotImplementedException();
	}

	public ReadOnlySpan<char> GetDesktopFolderPath() {
		throw new NotImplementedException();
	}

	public double GetFrameTime() {
		return FrameTime;
	}

	public double GetFreeDiskSpace(ReadOnlySpan<char> path) {
		throw new NotImplementedException();
	}

	public ReadOnlySpan<char> GetFullCommandLine() {
		throw new NotImplementedException();
	}

	public bool GetRegistryInteger(ReadOnlySpan<char> key, out int value) {
		throw new NotImplementedException();
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "WIN32 constant handle thsi")]
	public bool GetRegistryString(ReadOnlySpan<char> key, Span<char> value) {
#if WIN32
		string path = new(key);
		string fullPath = Path.GetDirectoryName(path)!;
		string valueName = Path.GetFileName(path);
		string? regValue = Registry.GetValue(fullPath, valueName, null) as string;
		if(regValue == null) {
			return false;
		}

		regValue.CopyTo(value);
		return true;
#endif
	}

	public bool GetShortcutTarget(ReadOnlySpan<char> linkFileName, Span<char> targetPath, Span<char> arguments) {
		throw new NotImplementedException();
	}

	public long GetTimeMillis() {
		throw new NotImplementedException();
	}

	public double GetTimeSinceLastUse() {
		throw new NotImplementedException();
	}

	public KeyValues? GetUserConfigFileData(ReadOnlySpan<char> dialogName, int dialogID) {
		throw new NotImplementedException();
	}

	public ButtonCode KeyCode_VirtualKeyToVGUI(int keyCode) {
		throw new NotImplementedException();
	}

	public bool ModifyShortcutTarget(ReadOnlySpan<char> linkFileName, ReadOnlySpan<char> targetPath, ReadOnlySpan<char> arguments, ReadOnlySpan<char> workingDirectory) {
		throw new NotImplementedException();
	}

	double FrameTime;

	public void RunFrame() {
		FrameTime = GetCurrentTime();	
	}

	public void SaveUserConfigFile() {
		throw new NotImplementedException();
	}

	public void SetClipboardImage(IWindow wnd, int x1, int y1, int x2, int y2) {
		throw new NotImplementedException();
	}

	public void SetClipboardText(ReadOnlySpan<char> text, int textLen) {
		throw new NotImplementedException();
	}

	public bool SetRegistryInteger(ReadOnlySpan<char> key, int value) {
		throw new NotImplementedException();
	}

	public bool SetRegistryString(ReadOnlySpan<char> key, ReadOnlySpan<char> value) {
		throw new NotImplementedException();
	}

	public void SetUserConfigFile(ReadOnlySpan<char> fileName, ReadOnlySpan<char> pathName) {
		throw new NotImplementedException();
	}

	public bool SetWatchForComputerUse(bool state) {
		throw new NotImplementedException();
	}

	public void ShellExecute(ReadOnlySpan<char> command, ReadOnlySpan<char> file) {
		throw new NotImplementedException();
	}

	public void ShellExecuteEx(ReadOnlySpan<char> command, ReadOnlySpan<char> file, ReadOnlySpan<char> pParams) {
		throw new NotImplementedException();
	}
}
