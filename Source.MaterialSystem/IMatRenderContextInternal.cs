using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public interface IMatRenderContextInternal : IMatRenderContext
{
	void BeginFrame();
	void EndFrame();
	void MarkRenderDataUnused(bool v);
	bool OnDrawMesh(IMesh mesh, int firstIndex, int indexCount);
	void SetFrameTime(double frameTime);
}