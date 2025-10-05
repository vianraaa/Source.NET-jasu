namespace Source.Common.Filesystem;

public interface IFileHandle : IDisposable
{
	public bool IsOK();
	public Stream Stream { get; }
	public FileNameHandle_t FileNameHandle { get; }
	public ReadOnlySpan<char> GetPath();
}
