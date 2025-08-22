using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public abstract class IndexBufferBase : IIndexBuffer
{
	public abstract void BeginCastBuffer(MaterialIndexFormat format);
	public abstract void EndCastBuffer();
	public abstract int IndexCount();
	public abstract MaterialIndexFormat IndexFormat();
	public abstract bool IsDynamic();
	public abstract bool Lock(int maxIndexCount, bool append, ref IndexDesc desc);
	public abstract bool Unlock(int writtenIndexCount, ref IndexDesc desc);
}