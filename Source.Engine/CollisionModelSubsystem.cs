global using static Source.Engine.CollisionBSPDataStatic;

using CommunityToolkit.HighPerformance;

using Source.Common.Engine;
using Source.Common.Formats.BSP;
using Source.Common.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Source.Engine;

public static class CollisionBSPDataStatic
{
	static readonly CollisionBSPData g_BSPData = new();
	public static CollisionBSPData GetCollisionBSPData() => g_BSPData;
}

public class CollisionBSPData
{
	public string? MapName;
	public string? MapNullName;
	public readonly List<CollisionModel> MapCollisionModels = [];
	public readonly List<CollisionSurface> MapSurfaces = [];

	public BSPVis[]? MapVis;

	public int NumSurfaces;
	public int NumLeafs;
	public int NumAreas;
	public int NumClusters;
	public int NumTextures;

	internal bool Init() {
		NumLeafs = 1;
		MapVis = null;
		NumAreas = 1;
		NumClusters = 1;
		MapNullName = "**empty**";
		NumTextures = 0;

		return true;
	}
	internal void PreLoad() {
		Init();
	}

	internal void Load(ReadOnlySpan<char> name) {
		throw new NotImplementedException();
	}
}

/// <summary>
/// Analog of the CM_ methods.
/// </summary>
[EngineComponent]
public class CollisionModelSubsystem()
{
	static uint last_checksum = uint.MaxValue;
	public void LoadMap(ReadOnlySpan<char> name, bool allowReusePrevious, out uint checksum) {
		CollisionBSPData bspData = GetCollisionBSPData();
		if (name.Equals(bspData.MapName, StringComparison.OrdinalIgnoreCase) && allowReusePrevious) {
			checksum = last_checksum;
			return;
		}

		bspData.PreLoad();
		if (name.IsEmpty) {
			checksum = 0;
			return;
		}

		MapLoadHelper.Init(null, name);
		bspData.Load(name);
		MapLoadHelper.Shutdown();

		DispTreeLeafnum(bspData);
		InitPortalOpenState(bspData);
		FloodAreaConnections(bspData);

		checksum = 0; // << Wtf, this never gets set in the engine? What's the point then???
		return;
	}

	private void FloodAreaConnections(CollisionBSPData bspData) {
		throw new NotImplementedException();
	}

	private void InitPortalOpenState(CollisionBSPData bspData) {
		throw new NotImplementedException();
	}

	private void DispTreeLeafnum(CollisionBSPData bspData) {
		throw new NotImplementedException();
	}
}