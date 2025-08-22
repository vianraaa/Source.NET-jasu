using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public class BufferedMeshGl46 : BaseMeshGl46 {
	BaseMeshGl46? Mesh;
	bool IsFlushing;
	bool FlushNeeded;

	public void ResetRendered() { }
	public bool WasNotRendered() => throw new NotImplementedException();
	public void SetMesh(BaseMeshGl46? mesh) {
		if(Mesh != mesh) {
			ShaderAPI.FlushBufferedPrimitives();
			Mesh = mesh;
		}
	}
	public BaseMeshGl46? GetMesh() => Mesh;
	public virtual void Spew(int vertexCount, int indexCount, out MeshDesc spewDesc) {
		spewDesc = default;
	}
	public virtual void SetVertexFormat(VertexFormat format) {

	}
	public override VertexFormat GetVertexFormat() {
		throw new NotImplementedException();
	}
	public void SetMaterial(IMaterial material) {

	}

	public override void HandleLateCreation() {
		Mesh?.HandleLateCreation();
	}

	public void Flush() {
		IsFlushing = true;
		((IMesh)Mesh!)!.Draw();
		IsFlushing = false;
		FlushNeeded = false;
	}
}