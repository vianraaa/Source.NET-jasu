using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public interface IMatRenderContextInternal : IMatRenderContext
{
	void BeginFrame();
	void EndFrame();
	void MarkRenderDataUnused(bool v);
	bool OnDrawMesh(IMesh mesh, int firstIndex, int indexCount);
	bool OnSetPrimitiveType(IMesh mesh, MaterialPrimitiveType type);
	void SetFrameTime(double frameTime);
	void SwapBuffers();
	void SyncMatrices();
	void SyncMatrix(MaterialMatrixMode mode);
	IMaterialInternal? GetCurrentMaterialInternal();
	void SetCurrentMaterialInternal(IMaterialInternal? material);
}