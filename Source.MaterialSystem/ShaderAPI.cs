using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.MaterialSystem;

public struct SamplerShadowState
{
	public bool TextureEnable;
}

public unsafe struct ShadowState {
	public const int MAX_SAMPLERS = 16;
	public const int MAX_TEXTURE_STAGES = 16;
}

public struct ShadowShaderState
{
	public VertexShaderHandle VertexShader;
	public PixelShaderHandle PixelShader;

	public VertexFormat VertexUsage;
	public bool ModulateConstantColor;
}

public class ShaderShadowGl46 : IShaderShadow
{
	ShadowState ShadowState;
	ShadowShaderState ShadowShaderState;
	public ref ShadowState GetShadowState() {
		return ref ShadowState;
	}
	public ref ShadowShaderState GetShadowShaderState() {
		return ref ShadowShaderState;
	}
	public void ComputeAggregateShadowState() {

	}
}

public class ShaderAPIGl46 : IShaderAPI
{
	public TransitionTable TransitionTable = new();
	public StateSnapshot_t CurrentSnapshot;
	public MeshMgr MeshMgr;

	public VertexFormat ComputeVertexFormat(Span<StateSnapshot_t> snapshots) {
		return ComputeVertexUsage(snapshots);
	}

	public VertexFormat ComputeVertexUsage(Span<StateSnapshot_t> snapshots) {
		if (snapshots.Length == 0)
			return 0;

		if (snapshots.Length == 1) {
			ref ShadowShaderState state = ref TransitionTable.GetSnapshotShader(snapshots[0]);
			return state.VertexUsage;
		}

		VertexCompressionType compression = VertexCompressionType.None;
		int userDataSize = 0, numBones = 0, flags = 0;
		Span<int> texCoordSize = [0, 0, 0, 0, 0, 0, 0, 0];
		for (int i = snapshots.Length; --i >= 0;) {
			ref ShadowShaderState state = ref TransitionTable.GetSnapshotShader(snapshots[i]);
			VertexFormat fmt = state.VertexUsage;
			flags |= fmt.VertexFlags();

			VertexCompressionType newCompression = fmt.CompressionType();
			if(compression != newCompression && compression != VertexCompressionType.Invalid) {
				Warning("Encountered a material with two passes that specify different vertex compression types!\n");
				compression = VertexCompressionType.Invalid;
			}

			int newNumBones = fmt.NumBoneWeights();
			if((numBones != newNumBones) && newNumBones != 0) {
				if (numBones != 0) {
					Warning("Encountered a material with two passes that use different numbers of bones!\n");
				}
				numBones = newNumBones;
			}

			int newUserSize = fmt.UserDataSize();
			if ((userDataSize != newUserSize) && (newUserSize != 0)) {
				if (userDataSize != 0) {
					Warning("Encountered a material with two passes that use different user data sizes!\n");
				}
				userDataSize = newUserSize;
			}

			for (int j = 0; j < IMesh.VERTEX_MAX_TEXTURE_COORDINATES; ++j) {
				int newSize = fmt.TexCoordSize(j);
				if ((texCoordSize[j] != newSize) && (newSize != 0)) {
					if (texCoordSize[j] != 0) {
						Warning("Encountered a material with two passes that use different texture coord sizes!\n");
					}
					if (texCoordSize[j] < newSize) {
						texCoordSize[j] = newSize;
					}
				}
			}
		}

		return MeshMgr.ComputeVertexFormat(flags, IMesh.VERTEX_MAX_TEXTURE_COORDINATES, texCoordSize, numBones, userDataSize);
	}
}