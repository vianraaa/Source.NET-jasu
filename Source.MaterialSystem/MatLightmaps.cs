using Source.Common.MaterialSystem;

using System.Runtime.CompilerServices;

namespace Source.MaterialSystem;

public class MatLightmaps
{
	private readonly MaterialSystem MaterialSystem;

	public MatLightmaps(MaterialSystem materialSystem) {
		MaterialSystem = materialSystem;
	}

	public int NumSortIDs = 0;
	public int NumLightmapPages = 0;

	internal int GetNumSortIDs() => NumSortIDs;

	public void BeginLightmapAllocation() {
		NumSortIDs = 0;
		ImagePackers.Clear();
		ImagePackers.Add(new(MaterialSystem));
		ImagePackers[0].Reset(0, GetMaxLightmapPageWidth(), GetMaxLightmapPageHeight());
		SetCurrentMaterialInternal(null);
		CurrentWhiteLightmapMaterial = null;
		NumSortIDs = 0;
		ResetMaterialLightmapPageInfo();
		EnumerateMaterials();
	}

	public void ResetMaterialLightmapPageInfo() {
		foreach (var material in MaterialSystem.MaterialDict) {
			material.SetMinLightmapPageID(9999);
			material.SetMaxLightmapPageID(-9999);
			material.SetNeedsWhiteLightmap(false);
		}
	}

	public void EnumerateMaterials() {
		int id = 0;
		foreach (var material in MaterialSystem.MaterialDict)
			material.SetEnumerationID(id++);
	}

	struct LightmapPageInfo
	{
		public ushort Width;
		public ushort Height;
		public int Flags;
	}

	readonly List<ImagePacker> ImagePackers = [];
	LightmapPageInfo[]? LightmapPages;

	public int AllocateLightmap(int width, int height, Span<int> offsetIntoLightmapPage, IMaterial imaterial) {
		if (imaterial is not IMaterialInternal material) {
			Warning("Programming error: MatLightmaps.AllocateLightmap: NULL material\n");
			return NumSortIDs;
		}

		int i;
		int packCount = ImagePackers.Count;
		if (GetCurrentMaterialInternal() != material) {
			for (i = packCount - 1; --i >= 0;) {
				ImagePackers.RemoveAt(i);
				--packCount;
			}

			if (GetCurrentMaterialInternal() != null) {
				ImagePackers[0].IncrementSortId();
				++NumSortIDs;
			}

			SetCurrentMaterialInternal(material);

			Assert(material.GetMinLightmapPageID() > material.GetMaxLightmapPageID());
			Assert(GetCurrentMaterialInternal() != null);

			GetCurrentMaterialInternal()!.SetMinLightmapPageID(GetNumLightmapPages());
			GetCurrentMaterialInternal()!.SetMaxLightmapPageID(GetNumLightmapPages());
		}

		bool added = false;
		for (i = 0; i < packCount; ++i) {
			added = ImagePackers[i].AddBlock(width, height, out offsetIntoLightmapPage[0], out offsetIntoLightmapPage[1]);
			if (added)
				break;
		}

		if (!added) {
			++NumSortIDs;
			i = ImagePackers.Count; ImagePackers.Add(new(MaterialSystem));
			ImagePackers[i].Reset(NumSortIDs, GetMaxLightmapPageWidth(), GetMaxLightmapPageHeight());
			++NumLightmapPages;
			if (!ImagePackers[i].AddBlock(width, height, out offsetIntoLightmapPage[0], out offsetIntoLightmapPage[1]))
				Error($"MaterialSystem_Interface_t::AllocateLightmap: lightmap ({width}x{height}) too big to fit in page ({GetMaxLightmapPageWidth()}x{GetMaxLightmapPageHeight()})\n");

			GetCurrentMaterialInternal()!.SetMaxLightmapPageID(GetNumLightmapPages());
		}

		return ImagePackers[i].GetSortId();
	}
	int GetMaxLightmapPageHeight() {
		int height = 256;

		if (height > MaterialSystem.HardwareConfig.MaxTextureHeight())
			height = MaterialSystem.HardwareConfig.MaxTextureHeight();

		return height;
	}
	int GetNumLightmapPages() => NumLightmapPages;
	int GetMaxLightmapPageWidth() {
		int width = 512;
		if (width > MaterialSystem.HardwareConfig.MaxTextureWidth())
			width = MaterialSystem.HardwareConfig.MaxTextureWidth();

		return width;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)] public IMaterialInternal? GetCurrentMaterialInternal() => MaterialSystem.GetRenderContextInternal().GetCurrentMaterialInternal();
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public void SetCurrentMaterialInternal(IMaterialInternal? material) => MaterialSystem.GetRenderContextInternal().SetCurrentMaterialInternal(material);

	int FirstDynamicLightmap;

	public void EndLightmapAllocation() {
		NumLightmapPages++;
		NumSortIDs++;

		FirstDynamicLightmap = NumLightmapPages;

		int lastLightmapPageWidth, lastLightmapPageHeight;
		int nLastIdx = ImagePackers.Count;
		ImagePackers[nLastIdx - 1].GetMinimumDimensions(out lastLightmapPageWidth, out lastLightmapPageHeight);
		ImagePackers.Clear();
		LightmapPages = new LightmapPageInfo[GetNumLightmapPages()];
		for (int i = 0; i < GetNumLightmapPages(); i++) {
			bool lastStaticLightmap = (i == (FirstDynamicLightmap - 1));
			LightmapPages[i].Width = (ushort)(lastStaticLightmap ? lastLightmapPageWidth : GetMaxLightmapPageWidth());
			LightmapPages[i].Height = (ushort)(lastStaticLightmap ? lastLightmapPageHeight : GetMaxLightmapPageHeight());
			LightmapPages[i].Flags = 0;

			AllocateLightmapTexture(i);
		}
	}

	private void AllocateLightmapTexture(int i) {
		// todo
	}

	IMaterialInternal? CurrentWhiteLightmapMaterial;

	internal int AllocateWhiteLightmap(IMaterial? imaterial) {
		if (imaterial is not IMaterialInternal material) {
			Warning("Programming error: MatLightmaps.AllocateWhiteLightmap: NULL material\n");
			return NumSortIDs;
		}

		if (CurrentWhiteLightmapMaterial == null || (CurrentWhiteLightmapMaterial != material)) {
			if (GetCurrentMaterialInternal() != null || CurrentWhiteLightmapMaterial != null)
				NumSortIDs++;

			CurrentWhiteLightmapMaterial = material;
			material.SetNeedsWhiteLightmap(true);
		}

		return NumSortIDs;
	}

	internal void GetSortInfo(Span<MaterialSystem_SortInfo> sortInfoArray) {
		int sortId = 0;
		ComputeSortInfo(sortInfoArray, ref sortId, false);
		ComputeWhiteLightmappedSortInfo(sortInfoArray, ref sortId, false);
		Assert(NumSortIDs == sortId);
	}

	private void ComputeSortInfo(Span<MaterialSystem_SortInfo> info, ref int sortId, bool v) {
		int lightmapPageID;
		foreach (var material in MaterialSystem.MaterialDict) {
			if (material.GetMinLightmapPageID() > material.GetMaxLightmapPageID())
				continue;

			for (lightmapPageID = material.GetMinLightmapPageID(); lightmapPageID <= material.GetMaxLightmapPageID(); ++lightmapPageID) {
				info[sortId].Material = material; // queue friendly review later
				info[sortId].LightmapPageID = lightmapPageID;

				++sortId;
			}
		}
	}

	private void ComputeWhiteLightmappedSortInfo(Span<MaterialSystem_SortInfo> info, ref int sortId, bool v) {
		foreach (var material in MaterialSystem.MaterialDict) {
			// TODO FIXME: The original plan was to not rely on reference counts and instead rely on C# object finalizers
			// and pushing unload events to the main thread. However, I think this is a bad idea for several reasons now.
			// I am reminded by it by this     \/--- statement where it checks if the material is referenced.
			if (material.GetNeedsWhiteLightmap()) {
				info[sortId].Material = material;
				if (material.GetPropertyFlag(MaterialPropertyTypes.NeedsBumpedLightmaps))
					info[sortId].LightmapPageID = StandardLightmap.WhiteBump;
				else
					info[sortId].LightmapPageID = StandardLightmap.White;

				sortId++;
			}
		}
	}

	internal void GetLightmapPageSize(int lightmapPageID, ref int width, ref int height) {
		switch (lightmapPageID) {
			default:
				Assert(lightmapPageID >= 0 && lightmapPageID < GetNumLightmapPages());
				width = LightmapPages![lightmapPageID].Width;
				height = LightmapPages![lightmapPageID].Height;
				break;

			case StandardLightmap.UserDefined:
				width = height = 1;
				Assert("Can't use CMatLightmaps to get properties of MATERIAL_SYSTEM_LIGHTMAP_PAGE_USER_DEFINED");
				break;

			case StandardLightmap.White:
			case StandardLightmap.WhiteBump:
				width = height = 1;
				break;
		}
	}
}
