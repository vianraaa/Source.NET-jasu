global using static Source.Engine.CollisionBSPDataStatic;

using CommunityToolkit.HighPerformance;

using Source.Common.Engine;
using Source.Common.Formats.BSP;
using Source.Common.MaterialSystem;
using Source.Common.Mathematics;
using Source.Common.Utilities;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Source.Engine;

public static class CollisionBSPDataStatic
{
	static readonly CollisionBSPData g_BSPData = new();
	public static CollisionBSPData GetCollisionBSPData() => g_BSPData;
}

[DebuggerDisplay("Source BSP Collision Model @ {Origin} [{Mins} -> {Maxs}] (head-node {HeadNode})")]
public struct CollisionModel
{
	public Vector3 Mins, Maxs, Origin;
	public int HeadNode;

	// Analog of CM_PointLeafnum
	public static int PointLeafnum(in Vector3 point) {
		CollisionBSPData bspData = GetCollisionBSPData();
		if (bspData.NumPlanes == 0)
			return 0;
		return PointLeafnum_r(bspData, in point, 0);
	}

	public static int PointLeafnum_r(CollisionBSPData bspData, in Vector3 point, int num) {
		float d;
		ref CollisionNode node = ref Unsafe.NullRef<CollisionNode>();
		ref CollisionPlane plane = ref Unsafe.NullRef<CollisionPlane>();

		while (num >= 0) {
			node = ref bspData.MapNodes.AsSpan()[bspData.MapRootNode + num];
			plane = ref bspData.MapPlanes.AsSpan()[node.CollisionPlaneIdx];

			if ((int)plane.Type < 3)
				d = point[(int)plane.Type] - plane.Dist;
			else
				d = Vector3.Dot(plane.Normal, point) - plane.Dist;

			if (d < 0)
				num = node.Children[1];
			else
				num = node.Children[0];
		}

		return -1 - num;
	}
	// vcollide_t research todo
}

public class CollisionBSPData
{
	public string? MapName;
	public string? MapNullName;
	public readonly List<CollisionModel> MapCollisionModels = [];
	public readonly List<CollisionSurface> MapSurfaces = [];
	public readonly List<CollisionPlane> MapPlanes = [];
	public readonly List<CollisionNode> MapNodes = [];
	public readonly List<CollisionLeaf> Leafs = [];
	public readonly List<ushort> MapLeafBrushes = [];
	public readonly List<string?> TextureNames = [];

	IMaterialSystem? materials;

	public BSPVis[]? MapVis;

	public int MapRootNode;

	public int NumSurfaces;
	public int NumLeafs;
	public int NumAreas;
	public int NumPlanes;
	public int NumClusters;
	public int NumNodes;
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
		Span<byte> stringData = lhStringData.LoadLumpData<byte>();
		Span<int> stringTable = lhStringTable.LoadLumpData<int>();

		BSPTexData[] inData = lh.LoadLumpData<BSPTexData>(throwIfNoElements: true, maxElements: BSPFileCommon.MAX_MAP_TEXDATA, sysErrorIfOOB: true);
		IMaterial? material;
		MapSurfaces.Clear(); MapSurfaces.EnsureCapacity(inData.Length);
		TextureNames.Clear(); TextureNames.EnsureCapacity(inData.Length);
		int lastNull = -1;
		for (int i = 0; i < stringData.Length; i++) {
			ref byte c = ref stringData[i];
			if (c == 0) {
				TextureNames.Add(Encoding.ASCII.GetString(stringData[(lastNull + 1)..i]));
				lastNull = i;
			}
		}
		NumTextures = inData.Length;

		for (int i = 0; i < inData.Length; i++) {
			ref BSPTexData _in = ref inData[i];
			Assert(_in.NameStringTableID >= 0);
			Assert(stringTable[_in.NameStringTableID] > 0);

			int index = _in.NameStringTableID;

			MapSurfaces.Add(new CollisionSurface() {
				Name = TextureNames[index]!,
				SurfaceProps = 0,
				Flags = 0
			});

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
		MapLoadHelper lh = new(LumpIndex.Leafs);
		switch (lh.LumpVersion) {
			case 0:
				CollisionBSPData_LoadLeafs_Version_0(lh);
				break;
			case 1:
				CollisionBSPData_LoadLeafs_Version_1(lh);
				break;
			default:
				Assert(0);
				Error("Unknown LUMP_LEAFS version\n");
				break;
		}
	}

	private void CollisionBSPData_LoadLeafs_Version_1(MapLoadHelper lh) {
		throw new NotImplementedException();
	}

	private void CollisionBSPData_LoadLeafs_Version_0(MapLoadHelper lh) {
		throw new NotImplementedException();
	}

	internal void LoadLeafBrushes() {
		MapLoadHelper lh = new MapLoadHelper(LumpIndex.LeafBrushes);
		ushort[] inData = lh.LoadLumpData<ushort>(throwIfNoElements: true, BSPFileCommon.MAX_MAP_LEAFBRUSHES, sysErrorIfOOB: true);

		MapLeafBrushes.Clear(); MapLeafBrushes.AddRange(inData);
	}

	internal void LoadPlanes() {
		MapLoadHelper lh = new MapLoadHelper(LumpIndex.Planes);
		BSPPlane[] inData = lh.LoadLumpData<BSPPlane>(throwIfNoElements: true, BSPFileCommon.MAX_MAP_PLANES, sysErrorIfOOB: true);
		MapPlanes.Clear(); MapPlanes.EnsureCount(inData.Length);

		Span<CollisionPlane> planes = MapPlanes.AsSpan();
		int count = inData.Length;
		for (int i = 0; i < count; i++) {
			ref readonly BSPPlane _in = ref inData[i];
			ref CollisionPlane _out = ref planes[i];
			int bits = 0;
			for (int j = 0; j < 3; j++) {
				_out.Normal[j] = _in.Normal[j];
				if (_out.Normal[j] < 0)
					bits |= 1 << j;
			}

			_out.Dist = _in.Dist;
			_out.Type = (PlaneType)_in.Type;
			_out.SignBits = (byte)bits;
		}
	}
	internal void LoadBrushes() {

	}
	internal void LoadBrushSides(List<ushort> map_texinfo) {

	}
	internal void LoadSubmodels() {

	}
	internal void LoadNodes() {
		MapLoadHelper lh = new MapLoadHelper(LumpIndex.Nodes);
		BSPNode[] inData = lh.LoadLumpData<BSPNode>(throwIfNoElements: true, BSPFileCommon.MAX_MAP_NODES, sysErrorIfOOB: true);
		int count = inData.Length;
		MapNodes.Clear(); MapNodes.EnsureCount(count + 6);

		NumNodes = count;
		MapRootNode = 0;

		Span<CollisionNode> outNodes = MapNodes.AsSpan();
		Span<CollisionPlane> planes = MapPlanes.AsSpan();
		for (int i = 0; i < count; i++) {
			ref BSPNode _in = ref inData[i];
			ref CollisionNode _out = ref outNodes[i];
			_out.CollisionPlaneIdx = _in.PlaneNum;
			for (int j = 0; j < 2; j++)
				_out.Children[j] = _in.Children[j];
		}

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