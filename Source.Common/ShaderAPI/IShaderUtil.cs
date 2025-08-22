using Source.Common.MaterialSystem;

namespace Source.Common.ShaderAPI;

public interface IShaderUtil
{
	bool OnDrawMesh(IMesh mesh, int firstIndex, int indexCount);
}