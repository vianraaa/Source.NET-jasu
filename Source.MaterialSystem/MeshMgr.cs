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
		if (IsPC()) {
			BufferedMesh.HandleLateCreation();
			BufferedMesh.Flush();
		}
	}

	public IMesh GetDynamicMesh(IMaterial? material, VertexFormat vertexFormat, int hwSkinBoneCount, bool buffered, IMesh vertexOverride, IMesh indexOverride) {
		Assert(material == null || ((IMaterialInternal)material).IsRealTimeVersion());

		if(BufferedMode != buffered && BufferedMode) {
			BufferedMesh.SetMesh(null);
		}
		BufferedMode = buffered;

		IMaterialInternal matInternal = (IMaterialInternal)material!;
		bool needTempMesh = ShaderAPI.IsInSelectionMode();

		BaseMeshGl46 mesh;
		if (needTempMesh) {
			Assert(vertexOverride == null);

			if(indexOverride  != null) {
				// not doing all that right now
				AssertMsg(false, "TODO");
			}
			mesh = DynamicTempMesh;
		}
		else {
			mesh = DynamicMesh;
		}

		if (BufferedMode) {
			Assert(!BufferedMesh.WasNotRendered());
			BufferedMesh.SetMesh(mesh);
			mesh = BufferedMesh;
		}

		if(vertexOverride == null) {
			VertexFormat materialFormat = matInternal.GetVertexFormat() & ~VertexFormat.Compressed;
			VertexFormat fmt = (vertexFormat != 0) ? vertexFormat : materialFormat;
			if(vertexFormat != 0) {
				int nVertexFormatBoneWeights = vertexFormat.NumBoneWeights();
				if (hwSkinBoneCount < nVertexFormatBoneWeights) {
					hwSkinBoneCount = nVertexFormatBoneWeights;
				}
			}

			fmt &= (VertexFormat)~VertexFormatFlags.VertexBoneWeightMask;
			if(hwSkinBoneCount > 0) {
				fmt |= VERTEX_BONEWEIGHT(2);
				fmt |= (VertexFormat)VertexFormatFlags.VertexFormatBoneIndex;
			}

			mesh.SetVertexFormat(fmt);
		}
		else {
			BaseMeshGl46 gl46Mesh = (BaseMeshGl46)vertexOverride;
			mesh.SetVertexFormat(gl46Mesh.GetVertexFormat());
		}

		mesh.SetMaterial(matInternal);
		if(mesh == DynamicMesh) {
			BaseMeshGl46? baseVertex = (BaseMeshGl46?)vertexOverride;
			if (baseVertex != null) DynamicMesh.OverrideVertexBuffer(baseVertex.GetVertexBuffer());
			BaseMeshGl46? baseIndex = (BaseMeshGl46?)vertexOverride;
			if (baseIndex != null) DynamicMesh.OverrideIndexBuffer(baseIndex.GetIndexBuffer());
		}

		return mesh;
	}

	internal IShaderAPI ShaderAPI;

	BufferedMeshGl46 BufferedMesh;
	DynamicMeshGl46 DynamicMesh;
	DynamicMeshGl46 DynamicFlexMesh;
	VertexBufferGl46 DynamicVertexBuffer;
	IndexBufferGl46 DynamicIndexBuffer;
	TempMeshGl46 DynamicTempMesh;
	bool BufferedMode;
	bool UsingFatVertices;

	VertexBufferGl46? CurrentVertexBuffer;
	VertexFormat CurrentVertexFormat;
	IndexBufferBase? CurrentIndexBuffer;
	int IndexBufferOffset;
	MaterialPrimitiveType PrimitiveTYpe;
	int FirstIndex;
	int NumIndices;

	internal void RenderPassWithVertexAndIndexBuffers() {
		throw new NotImplementedException();
	}

	// We can't rely on imported engineAPI.New<>() calls here because it makes the dependency injection
	// system crash and burn
	private TMesh InitMesh<TMesh>() where TMesh : BaseMeshGl46, new() {
		TMesh ret = new TMesh();
		ret.ShaderAPI = MaterialSystem.ShaderAPI;
		ret.ShaderUtil = MaterialSystem;
		return ret;
	}

	internal void Init() {
		BufferedMesh = InitMesh<BufferedMeshGl46>();
		DynamicMesh = InitMesh<DynamicMeshGl46>();
		DynamicFlexMesh = InitMesh<DynamicMeshGl46>();
		DynamicVertexBuffer = new VertexBufferGl46();
		DynamicIndexBuffer = new IndexBufferGl46();
		DynamicTempMesh = InitMesh<TempMeshGl46>();
	}
}
