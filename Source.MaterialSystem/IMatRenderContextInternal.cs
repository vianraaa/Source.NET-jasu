using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public interface IMatRenderContextInternal : IMatRenderContext
{
	bool OnDrawMesh(IMesh mesh, int firstIndex, int indexCount);
}