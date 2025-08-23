using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

namespace Source.MaterialSystem;
public class BaseMeshGl46 : MeshBase {
	public IShaderAPI ShaderAPI;
	public IShaderUtil ShaderUtil;
	public MeshMgr MeshMgr;
	public IShaderDevice ShaderDevice;

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

	public override VertexFormat GetVertexFormat() {
		return VertexFormat;
	}

	public IVertexBuffer GetVertexBuffer() {
		throw new NotImplementedException();
	}

	public IIndexBuffer GetIndexBuffer() {
		throw new NotImplementedException();
	}

	internal void SetMaterial(IMaterial material) {
		Material = (IMaterialInternal)material!;
	}

	internal void SetVertexFormat(VertexFormat fmt) {
		VertexFormat = fmt;
	}

	public virtual void PreLock() {
		throw new NotImplementedException();
	}

	public virtual bool HasEnoughRoom(int vertexCount, int indexCount) {
		throw new NotImplementedException();
	}
}
