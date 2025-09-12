
using Source.Common;
using Source.Common.MaterialSystem;

namespace Source.Engine;

public class MatSysInterface(IMaterialSystem materials)
{
	public readonly TextureReference FullFrameFBTexture0 = new();
	public readonly TextureReference FullFrameFBTexture1 = new();

	public void Init() {
		InitWellKnownRenderTargets();
		InitDebugMaterials();
	}

	private void InitDebugMaterials() {

	}

	private void InitWellKnownRenderTargets() {
		materials.BeginRenderTargetAllocation();
		FullFrameFBTexture0.Init(CreateFullFrameFBTexture(0));
		FullFrameFBTexture0.Init(CreateFullFrameFBTexture(1));
		materials.EndRenderTargetAllocation();
	}

	private ITexture CreateFullFrameFBTexture(int textureIndex, CreateRenderTargetFlags extraFlags = 0) {
		Span<char> textureName = stackalloc char[256];

		if (textureIndex > 0)
			sprintf(textureName, MaterialDefines.FULL_FRAME_FRAMEBUFFER_INDEXED, textureIndex);
		else
			strcpy(textureName, MaterialDefines.FULL_FRAME_FRAMEBUFFER);

		CreateRenderTargetFlags rtFlags = extraFlags | CreateRenderTargetFlags.HDR;
		return materials.CreateNamedRenderTargetTextureEx(
			textureName.SliceNullTerminatedString(),
			1, 1, RenderTargetSizeMode.FullFrameBuffer,
			materials.GetRenderContext().GetShaderAPI().GetBackBufferFormat(), MaterialRenderTargetDepth.Shared,
			TextureFlags.ClampS | TextureFlags.ClampT,
			rtFlags)!;
	}

	public void WorldStaticMeshCreate() {

	}
}
