using Source.Common.Engine;
using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public class MeshMgr
{
	internal MaterialSystem Materials;

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
		ret.ShaderAPI = Materials.ShaderAPI;
		ret.ShaderUtil = Materials;
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
