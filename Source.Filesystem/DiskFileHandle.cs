// TODO: Logging calls when things go wrong, ie. try/catches


using Source.Common.Filesystem;

namespace Source.FileSystem;

public class DiskFileHandle(IBaseFileSystem filesystem, FileStream data) : IFileHandle, IDisposable
{
	private bool disposedValue;

	public Stream Stream => data;

	protected virtual void Dispose(bool disposing) {
		if (!disposedValue && disposing)
			data.Dispose();
	}

	public void Dispose() {
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public bool IsOK() => !disposedValue && data != null;
}
