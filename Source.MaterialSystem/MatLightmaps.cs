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
		foreach(var material in MaterialSystem.MaterialDict) { 
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

	readonly List<ImagePacker> ImagePackers = [];

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
			Assert(GetCurrentMaterialInternal());

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

	public void EndLightmapAllocation() {
		NumSortIDs++;
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
}
