using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;
public class IndexBufferGl46 : IndexBufferBase
{
	public override void BeginCastBuffer(MaterialIndexFormat format) {
		throw new NotImplementedException();
	}

	public override void EndCastBuffer() {
		throw new NotImplementedException();
	}

	public override int IndexCount() {
		throw new NotImplementedException();
	}

	public override MaterialIndexFormat IndexFormat() {
		throw new NotImplementedException();
	}

	public override bool IsDynamic() {
		throw new NotImplementedException();
	}

	public override bool Lock(int maxIndexCount, bool append, ref IndexDesc desc) {
		throw new NotImplementedException();
	}

	public override bool Unlock(int writtenIndexCount, ref IndexDesc desc) {
		throw new NotImplementedException();
	}
}