using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

namespace Source.ShaderAPI.Gl46;

public class VertexBufferGl46 : VertexBufferBase
{
	private uint vertexBuffer;
	private VertexFormat vertexFormat;
	private int vertexCount;
	private int bufferSize;
	private int firstUnwrittenOffset;
	private bool locked;
	private bool dynamic;
	private bool flush;

	public VertexBufferGl46(ShaderBufferType type, VertexFormat format, int count) {
		vertexBuffer = 0;
		vertexFormat = format;
		vertexCount = count;
		bufferSize = (format == VertexFormat.Unknown) ? vertexCount : VertexSize();
		dynamic = type == ShaderBufferType.Dynamic;
	}

	private int VertexSize() {
		return 0;
	}

	public override void BeginCastBuffer(VertexFormat format) {
		throw new NotImplementedException();
	}

	public override void EndCastBuffer() {
		throw new NotImplementedException();
	}

	public override int GetRoomRemaining() {
		throw new NotImplementedException();
	}

	public override VertexFormat GetVertexFormat() {
		throw new NotImplementedException();
	}

	public override bool IsDynamic() {
		throw new NotImplementedException();
	}

	public override bool Lock(int vetexCount, bool append, in VertexDesc desc) {
		throw new NotImplementedException();
	}

	public override void Spew(int vertexCount, out VertexDesc desc) {
		throw new NotImplementedException();
	}

	public override bool Unlock(int vetexCount, in VertexDesc desc) {
		throw new NotImplementedException();
	}

	public override void ValidateData(int vertexCount, out VertexDesc desc) {
		throw new NotImplementedException();
	}

	public override int VertexCount() {
		throw new NotImplementedException();
	}

	internal void Free() {
		throw new NotImplementedException();
	}
}
