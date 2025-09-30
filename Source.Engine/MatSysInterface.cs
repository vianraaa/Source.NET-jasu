
using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Common.Commands;
using Source.Common.Engine;
using Source.Common.MaterialSystem;

namespace Source.Engine;


public class MatSysInterface(IMaterialSystem materials, IServiceProvider services)
{
	public readonly TextureReference FullFrameFBTexture0 = new();
	public readonly TextureReference FullFrameFBTexture1 = new();

	CommonHostState host_state;

	public void Init() {
		host_state = services.GetRequiredService<CommonHostState>();
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

	int FrameCount = 0;
	struct MeshList {
		public IMesh Mesh;
		public IMaterial Material;
		public int VertCount;
		public VertexFormat VertexFormat;
	}
	readonly List<MeshList> Meshes = [];
	readonly List<IMesh> WorldStaticMeshes = [];
	ConVar mat_max_worldmesh_vertices = new("65536", 0);
	public void WorldStaticMeshCreate() {
		FrameCount = 1;
		WorldStaticMeshDestroy();
		Assert(WorldStaticMeshes.Count == 0);

		
	}
	public void WorldStaticMeshDestroy() {

	}
}