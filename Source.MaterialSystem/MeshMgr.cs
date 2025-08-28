using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

namespace Source.MaterialSystem;

public enum DeviceState {
	OK,
	NeedsReset
}

public class MeshMgr
{
	internal MaterialSystem MaterialSystem;

	internal void Flush() {
		if (IsPC()) {
			BufferedMesh.Flush();
		}
	}

	public IMesh GetDynamicMesh(IMaterial? material, VertexFormat vertexFormat, int hwSkinBoneCount, bool buffered, IMesh? vertexOverride, IMesh? indexOverride) {
		Assert(material == null || ((IMaterialInternal)material).IsRealTimeVersion());

		if (BufferedMode != buffered && BufferedMode) {
			BufferedMesh.SetMesh(null);
		}
		BufferedMode = buffered;

		IMaterialInternal matInternal = (IMaterialInternal)material!;
		Mesh mesh = DynamicMesh;

		if (BufferedMode) {
			Assert(!BufferedMesh.WasNotRendered());
			BufferedMesh.SetMesh(mesh);
			mesh = BufferedMesh;
		}

		if (vertexOverride == null) {
			VertexFormat fmt = matInternal.GetVertexFormat();
			mesh.SetVertexFormat(fmt);
		}
		else {
			Mesh vertexMesh = (Mesh)vertexOverride;
			mesh.SetVertexFormat(vertexMesh.GetVertexFormat());
		}

		mesh.SetMaterial(matInternal);
		if (mesh == DynamicMesh) {
			Mesh? baseVertex = (Mesh?)vertexOverride;
			if (baseVertex != null)
				DynamicMesh.OverrideVertexBuffer(baseVertex.GetVertexBuffer());
			Mesh? baseIndex = (Mesh?)vertexOverride;
			if (baseIndex != null)
				DynamicMesh.OverrideIndexBuffer(baseIndex.GetIndexBuffer());
		}

		return mesh;
	}

	internal IShaderAPI ShaderAPI;

	bool BufferedMode;
	bool UsingFatVertices;

	VertexFormat CurrentVertexFormat;
	int IndexBufferOffset;
	MaterialPrimitiveType PrimitiveTYpe;
	int FirstIndex;
	int NumIndices;

	internal void RenderPassWithVertexAndIndexBuffers() {
		throw new NotImplementedException();
	}

	// We can't rely on imported engineAPI.New<>() calls here because it makes the dependency injection
	// system crash and burn
	private TMesh InitMesh<TMesh>() where TMesh : Mesh, new() {
		TMesh ret = new TMesh();
		ret.ShaderAPI = MaterialSystem.ShaderAPI;
		ret.ShaderUtil = MaterialSystem;
		ret.MeshMgr = MaterialSystem.MeshMgr;
		ret.ShaderDevice = MaterialSystem.ShaderDevice;
		return ret;
	}


	List<VertexBuffer> DynamicVertexBuffers = [];
	IndexBuffer? DynamicIndexBuffer;

	BufferedMesh BufferedMesh;
	DynamicMesh DynamicMesh;

	internal void Init() {
		BufferedMesh = InitMesh<BufferedMesh>();
		DynamicMesh = InitMesh<DynamicMesh>();
		DynamicMesh.Init(0);
		CreateDynamicIndexBuffer();
		CreateZeroVertexBuffer();
		BufferedMode = true;
	}

	private void CreateDynamicIndexBuffer() {
		DestroyDynamicIndexBuffer();
		DynamicIndexBuffer = new IndexBuffer(IMesh.INDEX_BUFFER_SIZE, true);
	}
	private void DestroyDynamicIndexBuffer() {
		DynamicIndexBuffer?.Dispose();
		DynamicIndexBuffer = null;
	}

	private void CreateZeroVertexBuffer() {
		// Todo...
	}


	public const int VERTEX_BUFFER_SIZE = 32768;
	public const int MAX_QUAD_INDICES = 16384;

	internal VertexBuffer FindOrCreateVertexBuffer(int dynamicBufferID, VertexFormat vertexFormat) {
		int vertexSize = VertexFormatSize(vertexFormat);

		while (DynamicVertexBuffers.Count <= dynamicBufferID) {
			int bufferMemory = ShaderAPI.GetCurrentDynamicVBSize();
			VertexBuffer vertexBuffer = new VertexBuffer();
			vertexBuffer.VertexSize = 0;
			int initVertexSize = bufferMemory / VERTEX_BUFFER_SIZE, initVertexCount = VERTEX_BUFFER_SIZE;
			vertexBuffer.BufferSize = initVertexSize * initVertexCount;
			DynamicVertexBuffers.Add(vertexBuffer);
		}

		VertexBuffer buffer = DynamicVertexBuffers[dynamicBufferID];

		if (buffer.VertexSize != vertexSize) {
			int bufferMemory = ShaderAPI.GetCurrentDynamicVBSize();
			buffer.VertexSize = vertexSize;
			buffer.ChangeConfiguration(vertexFormat, vertexSize, bufferMemory);
		}

		return DynamicVertexBuffers[dynamicBufferID];
	}

	internal unsafe int VertexFormatSize(VertexFormat vertexFormat) {
		MeshDesc desc = new();
		VertexBuffer.ComputeVertexDescription(null, vertexFormat, ref desc.Vertex);
		return desc.Vertex.ActualVertexSize;
	}

	internal IndexBuffer GetDynamicIndexBuffer() {
		return DynamicIndexBuffer!;
	}

	internal void RestoreBuffers() {
		Init();
	}
}
