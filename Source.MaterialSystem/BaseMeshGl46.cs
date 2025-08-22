using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

namespace Source.MaterialSystem;
public class BaseMeshGl46 : MeshBase {
	[Imported] public IShaderAPI ShaderAPI;
	[Imported] public IShaderUtil ShaderUtil;

	protected VertexFormat VertexFormat;
	protected IMaterialInternal Material;
	protected bool IsDrawing;

	public void DrawMesh() {
		Assert(!IsDrawing);
		IsDrawing = true;
		ShaderAPI.DrawMesh(this);
		IsDrawing = false;
	}
	public virtual void HandleLateCreation() {

	}

	public IVertexBuffer GetVertexBuffer() {
		throw new NotImplementedException();
	}

	public IIndexBuffer GetIndexBuffer() {
		throw new NotImplementedException();
	}

	internal void SetMaterial(IMaterial material) {
		throw new NotImplementedException();
	}

	internal void SetVertexFormat(VertexFormat fmt) {
		throw new NotImplementedException();
	}
}
