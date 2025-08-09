using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using static OpenGL.Gl46;
namespace Source.ShaderAPI.Gl46;

public class IndexBufferGl46 : IndexBufferBase
{
	uint glBuffer;
	private ShaderBufferType type;
	private MaterialIndexFormat format;
	private int count;

	public IndexBufferGl46(ShaderBufferType type, MaterialIndexFormat format, int count) {
		this.type = type;
		this.format = format;
		this.count = count;
	}

	public override void BeginCastBuffer(MaterialIndexFormat format) {
		throw new NotImplementedException();
	}

	public override void EndCastBuffer() {
		throw new NotImplementedException();
	}

	public override MaterialIndexFormat GetIndexFormat() {
		throw new NotImplementedException();
	}

	public override int GetRoomRemaining() {
		throw new NotImplementedException();
	}

	public override int IndexCount() {
		throw new NotImplementedException();
	}

	public override bool IsDynamic() {
		throw new NotImplementedException();
	}

	public override bool Lock(int vetexCount, bool append, in IndexDesc desc) {
		throw new NotImplementedException();
	}

	public override void Spew(int vertexCount, out IndexDesc desc) {
		throw new NotImplementedException();
	}

	public override bool Unlock(int vetexCount, in IndexDesc desc) {
		throw new NotImplementedException();
	}

	public override void ValidateData(int vertexCount, out IndexDesc desc) {
		throw new NotImplementedException();
	}

	internal void Free() {
		throw new NotImplementedException();
	}
}
