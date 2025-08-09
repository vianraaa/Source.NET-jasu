using Source.Common.MaterialSystem;

namespace Source.ShaderAPI.Gl46;

public abstract class IndexBufferBase : IIndexBuffer
{
	public abstract void BeginCastBuffer(MaterialIndexFormat format);
	public abstract void EndCastBuffer();
	public abstract MaterialIndexFormat GetIndexFormat();
	public abstract int GetRoomRemaining();
	public abstract int IndexCount();
	public abstract bool IsDynamic();
	public abstract bool Lock(int vetexCount, bool append, in IndexDesc desc);
	public abstract void Spew(int vertexCount, out IndexDesc desc);
	public abstract bool Unlock(int vetexCount, in IndexDesc desc);
	public abstract void ValidateData(int vertexCount, out IndexDesc desc);
}
