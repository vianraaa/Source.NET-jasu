using Source.Common.Engine;
using Source.Common.ShaderAPI;

namespace Source.MaterialSystem;
public class BaseMeshGl46 : MeshBase {
	[Imported] public IShaderAPI ShaderAPI;
	[Imported] public IShaderUtil ShaderUtil;

	public void DrawMesh() {
		
	}
	public virtual void HandleLateCreation() {

	}
}
