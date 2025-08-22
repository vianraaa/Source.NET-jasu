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

	BufferedMeshGl46 BufferedMesh = new();
	DynamicMeshGl46 DynamicMesh = new();
	DynamicMeshGl46 DynamicFlexMesh = new();
	VertexBufferGl46 DynamicVertexBuffer = new();
	IndexBufferGl46 DynamicIndexBuffer = new();
	TempMeshGl46 DynamicTempMesh = new();
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
}
