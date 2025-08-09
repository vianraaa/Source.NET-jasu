using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using static OpenGL.Gl46;
namespace Source.ShaderAPI.Gl46;

public class IndexBufferGl46 : IndexBufferBase
{
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
}
