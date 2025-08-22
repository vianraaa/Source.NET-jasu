namespace Source.MaterialSystem;

public class DynamicMeshGl46 : MeshGl46 {
	bool HasDrawn = false;
	public override void MarkAsDrawn() {
		base.MarkAsDrawn();
		HasDrawn = true;
	}

	public override void Draw(int firstIndex = -1, int indexCount = 0) {
		if (!g_ShaderUtil.OnDrawMesh(this, firstIndex, indexCount)) {
			MarkAsDrawn();
			return;
		}

		HasDrawn = true;
	}
}
