using Source.Common.MaterialSystem;

namespace Source.ShaderAPI.Gl46;

public abstract class VertexBufferBase : IVertexBuffer
{
	public abstract void BeginCastBuffer(VertexFormat format);
	public abstract void EndCastBuffer();
	public abstract int GetRoomRemaining();
	public abstract VertexFormat GetVertexFormat();
	public abstract bool IsDynamic();
	public abstract bool Lock(int vetexCount, bool append, in VertexDesc desc);
	public abstract void Spew(int vertexCount, out VertexDesc desc);
	public abstract bool Unlock(int vetexCount, in VertexDesc desc);
	public abstract void ValidateData(int vertexCount, out VertexDesc desc);
	public abstract int VertexCount();
}
