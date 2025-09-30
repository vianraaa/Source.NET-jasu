global using static Source.Engine.CollisionBSPDataStatic;

using CommunityToolkit.HighPerformance;

using Source.Common.Engine;
using Source.Common.Formats.BSP;
using Source.Common.MaterialSystem;
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
	public readonly List<string> TextureNames = [];

	IMaterialSystem? materials;

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
	internal void LoadTextures() {
		MapLoadHelper lh = new MapLoadHelper(LumpIndex.TexData);
		MapLoadHelper lhStringData = new MapLoadHelper(LumpIndex.TexDataStringData);
		MapLoadHelper lhStringTable = new MapLoadHelper(LumpIndex.TexDataStringTable);
		Span<char> stringData = lhStringData.LoadLumpData<char>();
		Span<int> stringTable = lhStringTable.LoadLumpData<int>();

		BSPTexData[] inData = lh.LoadLumpData<BSPTexData>(throwIfNoElements: true, maxElements: BSPFileCommon.MAX_MAP_TEXDATA, sysErrorIfOOB: true);
		IMaterial? material;
		MapSurfaces.Clear(); MapSurfaces.EnsureCapacity(inData.Length);
		TextureNames.Clear(); TextureNames.EnsureCapacity(inData.Length);
		int lastNull = -1;
		for (int i = 0; i < stringData.Length; i++) {
			ref char c = ref stringData[i];
			if (c == 0) {
				lastNull = i;
				TextureNames.Add(new string(stringData[(lastNull + 1)..i]));
			}
		}
		NumTextures = inData.Length;

		for (int i = 0; i < inData.Length; i++) {
			ref BSPTexData _in = ref inData[i];
			Assert(_in.NameStringTableID >= 0);
			Assert(stringTable[_in.NameStringTableID] > 0);

			int index = stringTable[_in.NameStringTableID];
			Span<char> inName = stringData[index..];

			material = materials!.FindMaterial(MapSurfaces[i].Name, MaterialDefines.TEXTURE_GROUP_WORLD, true);
			if (!material.IsErrorMaterial()) {
				IMaterialVar var;
				bool varFound;
				var = material.FindVar("$surfaceprop", out varFound, false);
				if (varFound) {
					ReadOnlySpan<char> props = var.GetStringValue();
					// TODO: set surface properties.
				}
			}

			MapSurfaces.Add(new CollisionSurface() {
				Name = TextureNames[index],
				SurfaceProps = 0,
				Flags = 0
			});
		}
	}
	internal void LoadTexinfo(List<ushort> map_texinfo) {
		MapLoadHelper lh = new MapLoadHelper(LumpIndex.TexInfo);
		BSPTexInfo[] inData = lh.LoadLumpData<BSPTexInfo>(throwIfNoElements: true, BSPFileCommon.MAX_MAP_TEXINFO, sysErrorIfOOB: true);
		map_texinfo.Clear(); map_texinfo.EnsureCount(inData.Length);
		ushort _out;
		Span<CollisionSurface> mapSurfaces = MapSurfaces.AsSpan();
		for (int i = 0; i < inData.Length; i++) {
			ref BSPTexInfo _in = ref inData[i];
			_out = (ushort)_in.TexData;
			if (_out >= NumTextures)
				_out = 0;
			mapSurfaces[_out].Flags |= (ushort)_in.Flags;
			map_texinfo.Add(_out);
		}
	}
	internal void LoadLeafs() { 
	
	}
	internal void LoadLeafBrushes() { 
	
	}
	internal void LoadPlanes() { 
	
	}
	internal void LoadBrushes() { 
	
	}
	internal void LoadBrushSides(List<ushort> map_texinfo) { 
	
	}
	internal void LoadSubmodels() { 
	
	}
	internal void LoadNodes() { 
	
	}
	internal void LoadAreas() { 
	
	}
	internal void LoadAreaPortals() { 
	
	}
	internal void LoadVisibility() { 
	
	}
	internal void LoadEntityString() { 
	
	}
	internal void LoadPhysics() { 
	
	}
	internal void LoadDispInfo() { 
	
	}
	internal bool Load(ReadOnlySpan<char> name) {
		List<ushort> map_texinfo = [];

		MapName = new(name);

		materials = Singleton<IMaterialSystem>();

		LoadTextures();
		LoadTexinfo(map_texinfo);
		LoadLeafs();
		LoadLeafBrushes();
		LoadPlanes();
		LoadBrushes();
		LoadBrushSides(map_texinfo);
		LoadSubmodels();
		LoadNodes();
		LoadAreas();
		LoadAreaPortals();
		LoadVisibility();
		LoadEntityString();
		LoadPhysics();
		LoadDispInfo();

		return true;
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

	}

	private void InitPortalOpenState(CollisionBSPData bspData) {

	}

	private void DispTreeLeafnum(CollisionBSPData bspData) {

	}
}