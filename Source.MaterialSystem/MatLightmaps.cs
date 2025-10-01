namespace Source.MaterialSystem;

public class MatLightmaps
{
	private readonly MaterialSystem MaterialSystem;

	public MatLightmaps(MaterialSystem materialSystem) {
		MaterialSystem = materialSystem;
	}

	public int NumSortIDs = 0;

	internal int GetNumSortIDs() => NumSortIDs;

	public void BeginLightmapAllocation() {
		NumSortIDs = 0;
	}

	public int AllocateLightmap(int width, int height, InlineArray2<int> offsetIntoLightmapPage, IMaterialInternal imaterial) {
		if (imaterial is not IMaterialInternal material) {
			Warning("Programming error: MatLightmaps.AllocateLightmap: NULL material\n");
			return NumSortIDs;
		}
		return 0; // << TODO rest of this logic, when we care about lighting 
	}

	public void EndLightmapAllocation() {
		NumSortIDs++;
	}
}
