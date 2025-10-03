using CommunityToolkit.HighPerformance;

using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Common.Commands;
using Source.Common.Engine;
using Source.Common.Formats.BSP;
using Source.Common.GUI;
using Source.Common.MaterialSystem;
using Source.Common.Mathematics;

using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Source.Engine;

public struct MaterialList
{
	public short NextBlock;
	public short Count;
	public InlineArray15<BSPMSurface2> Surfaces;
}

public struct SurfaceSortGroup
{
	public short ListHead;
	public short ListTail;
	public ushort VertexCount;
	public short GroupListIndex;
	public ushort VertexCountNoDetail;
	public ushort IndexCountNoDetail;
	public ushort TriangleCount;
	public ushort SurfaceCount;
}

public class MSurfaceSortList
{
	const int MAX_MAT_SORT_GROUPS = 4;

	int MaxSortIDs;
	public void Init(int maxSortIDs, int minMaterialLists) {
		List.Clear();
		List.EnsureCapacity(minMaterialLists);

		MaxSortIDs = maxSortIDs;

		int groupMax = maxSortIDs * MAX_MAT_SORT_GROUPS;
		Groups.EnsureCount(groupMax);
		memreset(Groups.AsSpan());

		int groupBytes = (groupMax + 7) >> 3;
		GroupUsed.EnsureCount(groupBytes);
		memreset(GroupUsed.AsSpan());

		for (int i = 0; i < SortGroupLists.Length; i++) {
			ref List<SurfaceSortGroup> list = ref SortGroupLists.AsSpan()[i];
			list ??= [];
			if (i == 0) {
				list.Clear();
				list.EnsureCapacity(128);
				GroupOffset[0] = 0;
			}
			else {
				list.Clear();
				list.EnsureCapacity(16);
				GroupOffset[i] = maxSortIDs * i;
			}
		}

		InitGroup(ref EmptyGroup);
	}

	readonly int[] GroupOffset = new int[MAX_MAT_SORT_GROUPS];
	readonly List<byte> GroupUsed = [];
	readonly List<MaterialList> List = [];
	readonly List<SurfaceSortGroup> Groups = [];
	readonly List<SurfaceSortGroup>[] SortGroupLists = new List<SurfaceSortGroup>[MAX_MAT_SORT_GROUPS];

	public void InitGroup(ref SurfaceSortGroup group) {
		group.ListHead = -1;
		group.ListTail = -1;
		group.VertexCount = 0;
		group.GroupListIndex = -1;
		group.VertexCountNoDetail = 0;
		group.IndexCountNoDetail = 0;
		group.TriangleCount = 0;
		group.SurfaceCount = 0;
	}
	public bool IsGroupUsed(int groupIndex) => (GroupUsed[(groupIndex >> 3)] & (1 << (groupIndex & 7))) != 0;
	public void MarkGroupUsed(int groupIndex) => GroupUsed[groupIndex >> 3] |= checked((byte)(1 << (groupIndex & 7)));
	public void MarkGroupNotUsed(int groupIndex) => GroupUsed[groupIndex >> 3] &= checked((byte)~(1 << (groupIndex & 7)));

	internal void AddSurfaceToTail(ref BSPMSurface2 surface, int sortGroup, short sortID) {
		Span<SurfaceSortGroup> groups = Groups.AsSpan();
		int index = GroupOffset[sortGroup] + sortID;
		ref SurfaceSortGroup group = ref groups[index];
		if (!IsGroupUsed(index)) {
			MarkGroupUsed(index);
			InitGroup(ref group);
		}
		ref MaterialList list = ref Unsafe.NullRef<MaterialList>();
		Span<MaterialList> m_list = List.AsSpan();
		short prevIndex = -1;
		int vertCount = ModelLoader.MSurf_VertCount(ref surface);
		int triangleCount = vertCount - 2;
		group.TriangleCount += (ushort)triangleCount;
		group.SurfaceCount++;
		group.VertexCount += (ushort)vertCount;
		if ((ModelLoader.MSurf_Flags(ref surface) & SurfDraw.Node) != 0) {
			group.VertexCountNoDetail += (ushort)vertCount;
			group.IndexCountNoDetail += (ushort)(triangleCount * 3);
		}
		if (group.ListTail != -1) {
			list = ref m_list[group.ListTail];
			if (list.Count >= 15 /* list.Surfaces length */) {
				prevIndex = group.ListTail;
				list = ref Unsafe.NullRef<MaterialList>();
			}
		}
		if (!Unsafe.IsNullRef(ref list)) {
			list.Surfaces[list.Count] = surface;
			list.Count++;
		}
		else {
			List.Add(default);
			short nextBlock = (short)(List.Count - 1);
			// m_list may be invalid now! remake the span
			m_list = List.AsSpan();

			if (prevIndex >= 0)
				m_list[prevIndex].NextBlock = nextBlock;

			group.ListTail = nextBlock;
			if (group.ListHead == -1) {
				SortGroupLists[sortGroup].Add(group);
				index = (short)(SortGroupLists[sortGroup].Count - 1);
				group.GroupListIndex = (short)index;
				group.ListHead = nextBlock;
			}

			list = ref m_list[nextBlock];
			list.NextBlock = -1;
			list.Count = 1;
			list.Surfaces[0] = surface;
		}
	}

	internal ref SurfaceSortGroup GetGroupForSortID(int sortGroup, int sortID) {
		return ref GetGroupByIndex(GetIndexForSortID(sortGroup, sortID));
	}

	internal ref MaterialList GetSurfaceBlock(short index) {
		return ref List.AsSpan()[index];
	}

	private int GetIndexForSortID(int sortGroup, int sortID) {
		return GroupOffset[sortGroup] + sortID;
	}

	SurfaceSortGroup EmptyGroup;

	private ref SurfaceSortGroup GetGroupByIndex(int groupIndex) {
		if (!IsGroupUsed(groupIndex))
			return ref EmptyGroup;
		return ref Groups.AsSpan()[groupIndex];
	}

	internal ref BSPMSurface2 GetSurfaceAtHead(in SurfaceSortGroup group) {
		if (group.ListHead == -1)
			return ref Unsafe.NullRef<BSPMSurface2>();
		return ref List.AsSpan()[group.ListHead].Surfaces[0];
	}
}

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
		MaterialEmpty = GL_LoadMaterial("debug/debugempty", MaterialDefines.TEXTURE_GROUP_OTHER)!;
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
	internal struct MeshList
	{
		public IMesh Mesh;
		public IMaterial Material;
		public int VertCount;
		public VertexFormat VertexFormat;
	}
	internal readonly List<MeshList> Meshes = [];
	internal readonly List<IMesh?> WorldStaticMeshes = [];
	ConVar mat_max_worldmesh_vertices = new((32767 / 3).ToString(), 0);
	public static int VertexCountForSurfaceList(MSurfaceSortList list, in SurfaceSortGroup group) {
		int vertexCount = 0;

		for (short _blockIndex = group.ListHead; _blockIndex != -1; _blockIndex = list.GetSurfaceBlock(_blockIndex).NextBlock) {
			ref MaterialList matList = ref list.GetSurfaceBlock(_blockIndex);
			for (int _index = 0; _index < matList.Count; ++_index) {
				ref BSPMSurface2 surfID = ref matList.Surfaces[_index];
				vertexCount += ModelLoader.MSurf_VertCount(ref surfID);
			}
		}

		return vertexCount;
	}
	public const uint TEXINFO_USING_BASETEXTURE2 = 0x0001;
	public void WorldStaticMeshCreate() {
		FrameCount = 1;
		WorldStaticMeshDestroy();
		Meshes.Clear();

		int sortIDs = materials.GetNumSortIDs();

		Assert(WorldStaticMeshes.Count == 0);
		WorldStaticMeshes.EnsureCountDefault(sortIDs);

		MSurfaceSortList matSortArray = new();
		matSortArray.Init(sortIDs, 512);
		Span<int> sortIndex = stackalloc int[WorldStaticMeshes.Count];

		for (int surfaceIndex = 0; surfaceIndex < host_state.WorldBrush!.NumSurfaces; surfaceIndex++) {
			ref BSPMSurface2 surfID = ref ModelLoader.SurfaceHandleFromIndex(surfaceIndex);
			ModelLoader.MSurf_Flags(ref surfID) &= ~SurfDraw.TangentSpace;

			if (ModelLoader.SurfaceHasDispInfo(ref surfID)) {
				ModelLoader.MSurf_VertBufferIndex(ref surfID) = 0xFFFF;
				continue;
			}

			matSortArray.AddSurfaceToTail(ref surfID, 0, ModelLoader.MSurf_MaterialSortID(ref surfID));
		}

		for (int i = 0; i < WorldStaticMeshes.Count; i++) {
			ref readonly SurfaceSortGroup group = ref matSortArray.GetGroupForSortID(0, i);
			int vertexCount = VertexCountForSurfaceList(matSortArray, group);

			ref BSPMSurface2 surfID = ref matSortArray.GetSurfaceAtHead(in group);
			WorldStaticMeshes[i] = null;
			sortIndex[i] = !Unsafe.IsNullRef(ref surfID) ? FindOrAddMesh(ModelLoader.MSurf_TexInfo(ref surfID).Material, vertexCount) : -1;
		}


		using MatRenderContextPtr renderContext = new(materials);
		var meshes = Meshes.AsSpan();
		for (int i = 0; i < Meshes.Count; i++) {
			VertexFormat format = meshes[i].Material.GetVertexFormat();
			meshes[i].Mesh = renderContext.CreateStaticMesh(format, MaterialDefines.TEXTURE_GROUP_STATIC_VERTEX_BUFFER_WORLD, meshes[i].Material);
			int vertBufferIndex = 0;
			MeshBuilder meshBuilder = new MeshBuilder();
			meshBuilder.Begin(meshes[i].Mesh, MaterialPrimitiveType.Triangles, meshes[i].VertCount);
			for (int j = 0; j < WorldStaticMeshes.Count; j++) {
				int meshId = sortIndex[j];
				if (meshId == i) {
					WorldStaticMeshes[j] = Meshes[i].Mesh;
					ref readonly SurfaceSortGroup group = ref matSortArray.GetGroupForSortID(0, j);
					for (short _blockIndex = group.ListHead; _blockIndex != -1; _blockIndex = matSortArray.GetSurfaceBlock(_blockIndex).NextBlock) {
						ref MaterialList matList = ref matSortArray.GetSurfaceBlock(_blockIndex);
						for (int _index = 0; _index < matList.Count; ++_index) {
							ref BSPMSurface2 surfID = ref matList.Surfaces[_index];
							ModelLoader.MSurf_VertBufferIndex(ref surfID) = (ushort)vertBufferIndex;
							BuildMSurfaceVertexArrays(host_state.WorldBrush!, ref surfID, IMaterialSystem.OVERBRIGHT, ref meshBuilder);
							vertBufferIndex += ModelLoader.MSurf_VertCount(ref surfID);
						}
					}
				}
			}

			meshBuilder.End();
			Assert(vertBufferIndex == Meshes[i].VertCount);
			meshBuilder.Dispose();
		}
	}

	struct SurfaceCtx {
		public InlineArray2<int> LightmapSize;
		public InlineArray2<int> LightmapPageSize;
		public float BumpSTexCoordOffset;
		public Vector2 Offset;
		public Vector2 Scale;
	}

	private void BuildMSurfaceVertexArrays(WorldBrushData brushData, ref BSPMSurface2 surfID, float overbright, ref MeshBuilder builder) {
		SurfaceCtx ctx = default;
		SurfSetupSurfaceContext(ref ctx, ref surfID);

		Color flatColor = new(255, 255, 255, 255);

		Vector3 vect = default;
		bool negate = false;
		// if ((ModelLoader.MSurf_Flags(ref surfID) & SurfDraw.TangentSpace) != 0) 
		// negate = TangentSpaceSurfaceSetup(ref surfID, vect);

		// CheckMSurfaceBaseTexture2(pBrushData, surfID);
		ushort first = (ushort)builder.GetCurrentVertex();
		int vertCount = ModelLoader.MSurf_VertCount(ref surfID);
		for (int i = 0; i < vertCount; i++) {
			int vertIndex = brushData.VertIndices![ModelLoader.MSurf_FirstVertIndex(ref surfID) + i];

			ref Vector3 vec = ref brushData.Vertexes![vertIndex].Position;

			builder.Position3fv(vec);

			Vector2 uv = default;
			SurfComputeTextureCoordinate(ref ctx, ref surfID, ref vec, ref uv);
			builder.TexCoord2fv(0, uv);

			SurfComputeLightmapCoordinate(ref ctx, ref surfID, ref vec, ref uv);
			builder.TexCoord2fv(1, uv);

			if ((ModelLoader.MSurf_Flags(ref surfID) & SurfDraw.BumpLight) != 0) {
				if (uv.X + ctx.BumpSTexCoordOffset * 3 > 1.00001f) {
					Assert(0);

					SurfComputeLightmapCoordinate(ref ctx, ref surfID, ref vec, ref uv);
				}
				builder.TexCoord2f(2, ctx.BumpSTexCoordOffset, 0.0f);
			}

			ref Vector3 normal = ref brushData.VertNormals![brushData.VertNormalIndices![ModelLoader.MSurf_FirstVertNormal(ref surfID) + i]];
			builder.Normal3fv(normal);

			if ((ModelLoader.MSurf_Flags(ref surfID) & SurfDraw.TangentSpace) != 0) {
				// TangentSpaceComputeBasis(out Vector3 tangentS, out Vector3 tangentT, normal, out vect, negate);
				// builder.TangentS3fv(tangentS);
				// builder.TangentT3fv(tangentT);
			}

			if (!ModelLoader.SurfaceHasDispInfo(ref surfID) && (ModelLoader.MSurf_TexInfo(ref surfID).TexInfoFlags & TEXINFO_USING_BASETEXTURE2) != 0) {
				bool warned = false;
				if (!warned) {
					ReadOnlySpan<char> materialName = ModelLoader.MSurf_TexInfo(ref surfID).Material!.GetName();
					warned = true;
					Warning($"Warning: WorldTwoTextureBlend found on a non-displacement surface (material: {materialName}). This wastes perf for no benefit.\n");
				}

				builder.Color4ub(255, 255, 255, 0);
			}
			else {
				builder.Color3ubv(flatColor);
			}

			builder.AdvanceVertex();
			if(i >= 2) {
				ushort firstOffset = (ushort)(first + (i - 2));
				builder.FastIndex(firstOffset);
				builder.FastIndex((ushort)(firstOffset + i));
				builder.FastIndex((ushort)(firstOffset + i + 1));
			}
		}
	}

	MaterialSystem_SortInfo[]? materialSortInfoArray;
	private int SortInfoToLightmapPage(int sortID) => materialSortInfoArray![sortID].LightmapPageID;

	private void SurfSetupSurfaceContext(ref SurfaceCtx ctx, ref BSPMSurface2 surfID) {
		materials.GetLightmapPageSize(SortInfoToLightmapPage(ModelLoader.MSurf_MaterialSortID(ref surfID)), ref ctx.LightmapPageSize[0], ref ctx.LightmapPageSize[1]);
		ctx.LightmapSize[0] = ModelLoader.MSurf_LightmapExtents(ref surfID)[0] + 1;
		ctx.LightmapSize[1] = ModelLoader.MSurf_LightmapExtents(ref surfID)[1] + 1;

		ctx.Scale.X = 1.0f / ctx.LightmapPageSize[0];
		ctx.Scale.Y = 1.0f / ctx.LightmapPageSize[1];

		ctx.Offset.X = (float)ModelLoader.MSurf_OffsetIntoLightmapPage(ref surfID)[0] * ctx.Scale.X;
		ctx.Offset.Y = (float)ModelLoader.MSurf_OffsetIntoLightmapPage(ref surfID)[1] * ctx.Scale.Y;

		if (ctx.LightmapPageSize[0] != 0.0f) 
			ctx.BumpSTexCoordOffset = (float)ctx.LightmapSize[0] / ctx.LightmapPageSize[0];
		else 
			ctx.BumpSTexCoordOffset = 0.0f;
	}

	private void SurfComputeLightmapCoordinate(ref SurfaceCtx ctx, ref BSPMSurface2 surfID, ref Vector3 vec, ref Vector2 uv) {
		if ((ModelLoader.MSurf_Flags(ref surfID) & SurfDraw.NoLight) != 0) 
			uv.X = uv.Y = 0.5f;
		
		else if (ModelLoader.MSurf_LightmapExtents(ref surfID)[0] == 0) {
			uv = (0.5f * ctx.Scale + ctx.Offset);
		}
		else {
			ref ModelTexInfo texInfo = ref ModelLoader.MSurf_TexInfo(ref surfID);

			uv.X = Vector3.Dot(vec, texInfo.LightmapVecsLuxelsPerWorldUnits[0].AsVector3()) + texInfo.LightmapVecsLuxelsPerWorldUnits[0][3];
			uv.X -= ModelLoader.MSurf_LightmapMins(ref surfID)[0];
			uv.X += 0.5f;

			uv.Y = Vector3.Dot(vec, texInfo.LightmapVecsLuxelsPerWorldUnits[1].AsVector3()) + texInfo.LightmapVecsLuxelsPerWorldUnits[1][3];
			uv.Y -= ModelLoader.MSurf_LightmapMins(ref surfID)[1];
			uv.Y += 0.5f;

			uv *= ctx.Scale;
			uv += ctx.Offset;

			Assert(uv.IsValid());
		}
		uv.X = Math.Clamp(uv.X, 0.0f, 1.0f);
		uv.Y = Math.Clamp(uv.Y, 0.0f, 1.0f);
	}

	private void SurfComputeTextureCoordinate(ref SurfaceCtx ctx, ref BSPMSurface2 surfID, ref Vector3 vec, ref Vector2 uv) {
		ref ModelTexInfo texInfo = ref ModelLoader.MSurf_TexInfo(ref surfID);

		// base texture coordinate
		uv.X = Vector3.Dot(vec, texInfo.TextureVecsTexelsPerWorldUnits[0].AsVector3()) + texInfo.TextureVecsTexelsPerWorldUnits[0][3];
		uv.X /= texInfo.Material!.GetMappingWidth();

		uv.Y = Vector3.Dot(vec, texInfo.TextureVecsTexelsPerWorldUnits[1].AsVector3()) + texInfo.TextureVecsTexelsPerWorldUnits[1][3];
		uv.Y /= texInfo.Material!.GetMappingHeight();
	}

	public static int CompareSurfID(ref BSPMSurface2 surfID1, ref BSPMSurface2 surfID2) {
		bool hasLightmap1 = (ModelLoader.MSurf_Flags(ref surfID1) & SurfDraw.NoLight) == 0;
		bool hasLightmap2 = (ModelLoader.MSurf_Flags(ref surfID2) & SurfDraw.NoLight) == 0;

		if (hasLightmap1 != hasLightmap2)
			return (hasLightmap2 ? 1 : 0) - (hasLightmap1 ? 1 : 0);

		IMaterial? material1 = ModelLoader.MSurf_TexInfo(ref surfID1).Material;
		IMaterial? material2 = ModelLoader.MSurf_TexInfo(ref surfID2).Material;
		int enum1 = material1!.GetEnumerationID();
		int enum2 = material2!.GetEnumerationID();
		if (enum1 != enum2)
			return enum1 - enum2;

		bool hasLightstyle1 = (ModelLoader.MSurf_Flags(ref surfID1) & SurfDraw.HasLightstyles) == 0;
		bool hasLightstyle2 = (ModelLoader.MSurf_Flags(ref surfID2) & SurfDraw.HasLightstyles) == 0;

		if (hasLightstyle1 != hasLightstyle2)
			return (hasLightstyle2 ? 1 : 0) - (hasLightstyle1 ? 1 : 0);

		int area1 = ModelLoader.MSurf_LightmapExtents(ref surfID1)[0] * ModelLoader.MSurf_LightmapExtents(ref surfID1)[1];
		int area2 = ModelLoader.MSurf_LightmapExtents(ref surfID2)[0] * ModelLoader.MSurf_LightmapExtents(ref surfID2)[1];
		return area2 - area1;
	}

	public const int NUM_BUMP_VECTS = 3;
	private static bool SurfNeedsBumpedLightmaps(ref BSPMSurface2 surfID) => ModelLoader.MSurf_TexInfo(ref surfID).Material!.GetPropertyFlag(MaterialPropertyTypes.NeedsBumpedLightmaps);
	private void RegisterUnlightmappedSurface(ref BSPMSurface2 surfID) {
		ModelLoader.MSurf_MaterialSortID(ref surfID) = materials.AllocateWhiteLightmap(ModelLoader.MSurf_TexInfo(ref surfID).Material);
		ModelLoader.MSurf_OffsetIntoLightmapPage(ref surfID)[0] = 0;
		ModelLoader.MSurf_OffsetIntoLightmapPage(ref surfID)[1] = 0;
	}
	private void RegisterLightmappedSurface(ref BSPMSurface2 surfID) {
		Span<int> lightmapSize = stackalloc int[2];
		int allocationWidth, allocationHeight;
		bool needsBumpmap;

		lightmapSize[0] = ModelLoader.MSurf_LightmapExtents(ref surfID)[0] + 1;
		lightmapSize[1] = ModelLoader.MSurf_LightmapExtents(ref surfID)[1] + 1;

		needsBumpmap = SurfNeedsBumpedLightmaps(ref surfID);
		if (needsBumpmap) {
			ModelLoader.MSurf_Flags(ref surfID) |= SurfDraw.BumpLight;
			allocationWidth = lightmapSize[0] * (NUM_BUMP_VECTS + 1);
		}
		else {
			ModelLoader.MSurf_Flags(ref surfID) &= ~SurfDraw.BumpLight;
			allocationWidth = lightmapSize[0];
		}

		allocationHeight = lightmapSize[1];

		Span<int> offsetIntoLightmapPage = stackalloc int[2];
		ModelLoader.MSurf_MaterialSortID(ref surfID) = materials.AllocateLightmap(
			allocationWidth,
			allocationHeight,
			offsetIntoLightmapPage,
			ModelLoader.MSurf_TexInfo(ref surfID).Material);

		ModelLoader.MSurf_OffsetIntoLightmapPage(ref surfID)[0] = (short)offsetIntoLightmapPage[0];
		ModelLoader.MSurf_OffsetIntoLightmapPage(ref surfID)[1] = (short)offsetIntoLightmapPage[1];
	}
	internal void RegisterLightmapSurfaces() {
		ref BSPMSurface2 surfID = ref Unsafe.NullRef<BSPMSurface2>();
		materials.BeginLightmapAllocation();

		List<nint> surfaces = new();
		for (int surfaceIndex = 0; surfaceIndex < host_state.WorldBrush!.NumSurfaces; surfaceIndex++) {
			surfID = ref ModelLoader.SurfaceHandleFromIndex(surfaceIndex);
			if ((ModelLoader.MSurf_TexInfo(ref surfID).Flags & Surf.NoLight) != 0 ||
				(ModelLoader.MSurf_Flags(ref surfID) & SurfDraw.NoLight) != 0) {
				ModelLoader.MSurf_Flags(ref surfID) |= SurfDraw.NoLight;
			}
			else
				ModelLoader.MSurf_Flags(ref surfID) &= ~SurfDraw.NoLight;

			surfaces.Add(surfID.SurfNum);
		}
		surfaces.Sort((sn1, sn2) => {
			ref BSPMSurface2 surfID1 = ref host_state.WorldBrush!.Surfaces2![sn1];
			ref BSPMSurface2 surfID2 = ref host_state.WorldBrush!.Surfaces2![sn2];
			return CompareSurfID(ref surfID1, ref surfID2);
		});

		surfID = ref Unsafe.NullRef<BSPMSurface2>();
		foreach (var surfIDidx in surfaces) {
			surfID = ref host_state.WorldBrush!.Surfaces2![surfIDidx];
			bool hasLightmap = (ModelLoader.MSurf_Flags(ref surfID) & SurfDraw.NoLight) == 0;
			if (hasLightmap)
				RegisterLightmappedSurface(ref surfID);
			else
				RegisterUnlightmappedSurface(ref surfID);
		}

		materials.EndLightmapAllocation();
	}
	private int FindOrAddMesh(IMaterial? material, int vertexCount) {
		VertexFormat format = material.GetVertexFormat();

		using MatRenderContextPtr renderContext = new(materials);

		int nMaxVertices = renderContext.GetMaxVerticesToRender(material);
		int worldLimit = mat_max_worldmesh_vertices.GetInt();
		worldLimit = Math.Max(worldLimit, 1024);
		if (nMaxVertices > worldLimit) 
			nMaxVertices = mat_max_worldmesh_vertices.GetInt();

		Span<MeshList> meshes = Meshes.AsSpan();

		for (int i = 0; i < meshes.Length; i++) {
			if (meshes[i].VertexFormat != format)
				continue;

			if (meshes[i].VertCount + vertexCount > nMaxVertices)
				continue;

			meshes[i].VertCount += vertexCount;
			return i;
		}

		Meshes.Add(new() {
			VertCount = vertexCount,
			VertexFormat = format,
			Material = material
		});

		return Meshes.Count - 1;
	}

	public void WorldStaticMeshDestroy() {

	}

	public ConVar mat_loadtextures = new("1", 0);
	public IMaterial MaterialEmpty;

	public IMaterial GL_LoadMaterial(ReadOnlySpan<char> name, ReadOnlySpan<char> textureGroupName) {
		IMaterial? material = GL_LoadMaterialNoRef(name, textureGroupName);
		return material;
	}

	private IMaterial GL_LoadMaterialNoRef(ReadOnlySpan<char> name, ReadOnlySpan<char> textureGroupName) {
		if (mat_loadtextures.GetInt() != 0)
			return materials.FindMaterial(name, textureGroupName);
		else
			return MaterialEmpty;
	}

	internal void DestroySortInfo() {

	}

	internal void CreateSortInfo() {
		Assert(materialSortInfoArray == null);
		int sortIDs = materials.GetNumSortIDs();
		materialSortInfoArray = new MaterialSystem_SortInfo[sortIDs];
		materials.GetSortInfo(materialSortInfoArray);
		WorldStaticMeshCreate();
	}

	internal float GetScreenAspect() {
		// r_aspectratio todo
		IMatRenderContext renderContext = materials.GetRenderContext();

		renderContext.GetRenderTargetDimensions(out int width, out int height);
		return (height != 0) ? ((float)width / (float)height) : 1.0f;
	}
}