using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

namespace Source.MaterialSystem;

public class MeshMgr
{
	internal MaterialSystem MaterialSystem;

	public VertexFormat ComputeVertexFormat(int flags, int texCoordArraySize, Span<int> texCoordDimensions, int numBonesWeights, int userDataSize) {
		VertexFormat fmt = (VertexFormat)(flags & ~VertexFormatFlags.VertexFormatUseExactFormat);
		if (true) {
			fmt &= ~VertexFormat.Compressed;
		}

		Assert(numBonesWeights <= 4);
		if (numBonesWeights > 0) {
			fmt |= VERTEX_BONEWEIGHT(2);
		}

		Assert(userDataSize <= 4);
		fmt |= VERTEX_USERDATA_SIZE(userDataSize);

		texCoordArraySize = Math.Min(texCoordArraySize, IMesh.VERTEX_MAX_TEXTURE_COORDINATES);
		for (int i = 0; i < texCoordArraySize; i++) {
			if (texCoordDimensions != null) {
				Assert(texCoordDimensions[i] >= 0 && texCoordDimensions[i] <= 4);
				fmt |= VERTEX_TEXCOORD_SIZE(i, texCoordDimensions[i]);
			}
			else {
				fmt |= VERTEX_TEXCOORD_SIZE(i, 2);
			}
		}

		return fmt;
	}

	internal void Flush() {
		if (IsPC()) 
			BufferedMesh.Flush();
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
			VertexFormat materialFormat = matInternal.GetVertexFormat() & ~VertexFormat.Compressed;
			VertexFormat fmt = (vertexFormat != 0) ? vertexFormat : materialFormat;
			if (vertexFormat != 0) {
				int nVertexFormatBoneWeights = vertexFormat.NumBoneWeights();
				if (hwSkinBoneCount < nVertexFormatBoneWeights) {
					hwSkinBoneCount = nVertexFormatBoneWeights;
				}
			}

			fmt &= (VertexFormat)~VertexFormatFlags.VertexBoneWeightMask;
			if (hwSkinBoneCount > 0) {
				fmt |= VERTEX_BONEWEIGHT(2);
				fmt |= (VertexFormat)VertexFormatFlags.VertexFormatBoneIndex;
			}

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

	BufferedMesh BufferedMesh;
	DynamicMesh DynamicMesh;

	internal void Init() {
		BufferedMesh = InitMesh<BufferedMesh>();
		DynamicMesh = InitMesh<DynamicMesh>();
	}

	List<VertexBuffer> DynamicVertexBuffers = [];

	public const int VERTEX_BUFFER_SIZE = 32768;
	public const int MAX_QUAD_INDICES = 16384;

	internal VertexBuffer FindOrCreateVertexBuffer(int dynamicBufferID, VertexFormat vertexFormat) {
		int vertexSize = VertexFormatSize(vertexFormat);

		while (DynamicVertexBuffers.Count <= dynamicBufferID) {
			int bufferMemory = ShaderAPI.GetCurrentDynamicVBSize();
			VertexBuffer vertexBuffer = new VertexBuffer(bufferMemory / VERTEX_BUFFER_SIZE, VERTEX_BUFFER_SIZE, true) {

			};
			DynamicVertexBuffers.Add(vertexBuffer);
		}

		if (DynamicVertexBuffers[dynamicBufferID].VertexSize != vertexSize) {
			int bufferMemory = ShaderAPI.GetCurrentDynamicVBSize();
			DynamicVertexBuffers[dynamicBufferID].VertexSize = vertexSize;
			DynamicVertexBuffers[dynamicBufferID].ChangeConfiguration(vertexSize, bufferMemory);
		}

		return DynamicVertexBuffers[dynamicBufferID];
	}

	private static int VertexFormatSize(VertexFormat format) {
		return VertexBuffer.VertexFormatSize(format);
	}

	internal IndexBuffer GetDynamicIndexBuffer() {
		throw new NotImplementedException();
	}
}
