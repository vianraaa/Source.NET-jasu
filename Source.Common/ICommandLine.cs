using System.Diagnostics.CodeAnalysis;

namespace Source.Common;

public interface ICommandLine {
	public void CreateCmdLine(ReadOnlySpan<char> commandLine);
	public void CreateCmdLine(IEnumerable<string> commandLine);
	public string? GetCmdLine();

	public bool CheckParm(string name, out IEnumerable<string> values);
	public bool CheckParm(string name) => CheckParm(name, out _);
	public bool HasParm(string name);
	public void RemoveParm(string name);
	public void AppendParm(string name, string? values = null);

	[return: NotNullIfNotNull(nameof(defaultValue))] public string? ParmValue(string name, string? defaultValue = null);
	public int ParmValue(string name, int defaultValue);
	public float ParmValue(string name, float defaultValue);
	public double ParmValue(string name, double defaultValue);

	public int ParmCount();
	public int FindParm(string name);
	public string GetParm(int index);
	public void SetParm(int index, string newParm);
	[return: NotNullIfNotNull(nameof(defaultValue))] public string? ParmValueByIndex(int index, string? defaultValue = null);
}