using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public struct PrimList
{
	public uint FirstIndex;
	public uint NumIndices;
}

public unsafe class DynamicMeshGl46 : MeshGl46
{
	int BufferID;
	int TotalVertices;
	int TotalIndices;
	int FirstVertex;
	int FirstIndex;
	bool HasDrawn = false;
	bool VertexOverride;
	bool IndexOverride;

	public override void MarkAsDrawn() {
		base.MarkAsDrawn();
		HasDrawn = true;
	}

	public override void PreLock() {
		if (HasDrawn) {
			ResetVertexAndIndexCounts();
		}
	}

	private void ResetVertexAndIndexCounts() {
		TotalVertices = TotalIndices = 0;
		FirstIndex = FirstVertex = -1;
		HasDrawn = false;
	}

	public override void Draw(int firstIndex = -1, int indexCount = 0) {
		if (!ShaderUtil.OnDrawMesh(this, firstIndex, indexCount)) {
			MarkAsDrawn();
			return;
		}

		HasDrawn = true;

		if (IndexOverride || VertexOverride ||
		((TotalVertices > 0) && (TotalIndices > 0 || Type == MaterialPrimitiveType.Points || Type == MaterialPrimitiveType.InstancedQuads))) {
			Assert(!IsDrawing);

			HandleLateCreation();

			// only have a non-zero first vertex when we are using static indices
			int firstVertex = VertexOverride ? 0 : FirstVertex;
			int actualFirstVertex = IndexOverride ? firstVertex : 0;
			int vertexOffsetInBytes = HasFlexMesh() ? firstVertex * MeshMgr.VertexFormatSize(GetVertexFormat()) : 0;
			int baseIndex = IndexOverride ? 0 : FirstIndex;

			// Overriding with the dynamic index buffer, preserve state!
			if (IndexOverride && IndexBuffer == MeshMgr.GetDynamicIndexBuffer()) {
				baseIndex = FirstIndex;
			}

			VertexFormat fmt = VertexOverride ? GetVertexFormat() : VertexFormat.Invalid;
			if (!SetRenderState(vertexOffsetInBytes, actualFirstVertex, fmt))
				return;

			// Draws a portion of the mesh
			int numVertices = VertexOverride ? VertexBuffer.VertexCount() : TotalVertices;
			if ((firstIndex != -1) && (indexCount != 0)) {
				firstIndex += baseIndex;
			}
			else {
				// by default we draw the whole thing
				FirstIndex = baseIndex;
				if (IndexOverride) {
					indexCount = IndexBuffer.IndexCount();
					Assert(indexCount != 0);
				}
				else {
					indexCount = TotalIndices;
					// Fake out the index count	if we're drawing points/instanced-quads
					if ((Type == MaterialPrimitiveType.Points) || (Type == MaterialPrimitiveType.InstancedQuads)) {
						indexCount = TotalVertices;
					}
					Assert(indexCount != 0);
				}
			}

			// Fix up nFirstVertex to indicate the first vertex used in the data
			if (!HasFlexMesh()) {
				actualFirstVertex = FirstVertex - actualFirstVertex;
			}

			s_FirstVertex = actualFirstVertex;
			s_NumVertices = numVertices;

			// Build a primlist with 1 element..
			PrimList* prim = stackalloc PrimList[1];
			prim->FirstIndex = (uint)firstIndex;
			prim->NumIndices = (uint)indexCount;
			Assert(indexCount != 0);
			s_pPrims = prim;
			s_nPrims = 1;

			DrawMesh();

			s_pPrims = null;
		}
	}

	internal void OverrideVertexBuffer(IVertexBuffer vertexBuffer) {
		throw new NotImplementedException();
	}

	internal void OverrideIndexBuffer(IIndexBuffer indexBuffer) {
		throw new NotImplementedException();
	}

	public override int IndexCount() {
		return TotalIndices;
	}

	public override bool HasEnoughRoom(int vertexCount, int indexCount) {
		if (ShaderDevice.IsDeactivated())
			return false;

		Assert(VertexBuffer != null);

		return VertexBuffer.HasEnoughRoom(vertexCount) && IndexBuffer.HasEnoughRoom(indexCount);
	}
}
