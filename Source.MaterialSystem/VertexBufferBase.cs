using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public abstract class VertexBufferBase : IVertexBuffer
{
	public abstract void BeginCastBuffer(VertexFormat format);
	public abstract void EndCastBuffer();
	public abstract int GetRoomRemaining();
	public abstract VertexFormat GetVertexFormat();
	public abstract bool IsDynamic();
	public abstract bool Lock(int vertexCount, bool append, ref VertexDesc desc);
	public abstract bool Unlock(int vertexCount, ref VertexDesc desc);
	public abstract int VertexCount();
}
