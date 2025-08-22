using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public struct PrimList
{
	public uint FirstIndex;
	public uint NumIndices;
}

public unsafe class DynamicMeshGl46 : MeshGl46
{
	bool HasDrawn = false;

	public override void MarkAsDrawn() {
		base.MarkAsDrawn();
		HasDrawn = true;
	}

	public override void Draw(int firstIndex = -1, int indexCount = 0) {
		if (!ShaderUtil.OnDrawMesh(this, firstIndex, indexCount)) {
			MarkAsDrawn();
			return;
		}

		HasDrawn = true;

		PrimList* prim = stackalloc PrimList[1];
		prim->FirstIndex = (uint)firstIndex;
		prim->NumIndices = (uint)indexCount;
		s_pPrims = prim;
		s_nPrims = 1;

		DrawMesh();

		s_pPrims = null;
	}

	internal void OverrideVertexBuffer(IVertexBuffer vertexBuffer) {
		throw new NotImplementedException();
	}

	internal void OverrideIndexBuffer(IIndexBuffer indexBuffer) {
		throw new NotImplementedException();
	}
}
