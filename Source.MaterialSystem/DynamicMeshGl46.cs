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

	public override void LockMesh(int vertexCount, int indexCount, ref MeshDesc desc) {
		Lock(vertexCount, false, ref desc.Vertex);
		Lock(vertexCount, false, ref desc.Index);

		FirstVertex = desc.Vertex.FirstVertex;
		FirstIndex = (int)desc.Index.FirstIndex;

		desc.Index.Indices = (ushort*)IndexBuffer.SysmemBuffer;
	}

	public override bool NeedsVertexFormatReset(VertexFormat format) {
		return VertexOverride || IndexOverride || VertexFormat != format;
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

	public override void SetVertexFormat(VertexFormat format) {
		if (ShaderDevice.IsDeactivated())
			return;

		if(format.CompressionType() != VertexCompressionType.None) {
			Warning("ERROR: dynamic meshes cannot use compressed vertices!\n");
			Assert(false);
			format &= ~VertexFormat.Compressed;
		}

		if(format != VertexFormat || VertexOverride || IndexOverride) {
			VertexFormat = format;
			UseVertexBuffer(MeshMgr.FindOrCreateVertexBuffer(BufferID, format));
			if(BufferID == 0) {
				UseIndexBuffer(MeshMgr.GetDynamicIndexBuffer());
			}
			VertexOverride = IndexOverride = false;
		}
	}

	private void UseIndexBuffer(IndexBuffer indexBuffer) {
		throw new NotImplementedException();
	}

	private void UseVertexBuffer(VertexBuffer vertexBuffer) {
		throw new NotImplementedException();
	}

	public override void Draw(int firstIndex = -1, int indexCount = 0) {
		if (!ShaderUtil.OnDrawMesh(this, firstIndex, indexCount)) {
			MarkAsDrawn();
			return;
		}

		HasDrawn = true;

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

		return true;
	}
}
