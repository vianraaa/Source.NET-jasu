using Source.Common.Engine;
using Source.Common;
using Source.Common.Filesystem;
using Source.Common.Formats.BSP;
using System.Numerics;
using Source.Common.Mathematics;
using CommunityToolkit.HighPerformance;
namespace Source.Engine;

public ref struct MapLoadHelper
{
	internal static WorldBrushData? Map;
	internal static string? LoadName;
	internal static string? MapName;
	internal static Stream? MapFileHandle;
	internal static BSPHeader MapHeader;
	static Host? Host;
	public static void Init(Model? model, ReadOnlySpan<char> loadName) {
		Host = Singleton<Host>();
		ModelLoader ModelLoader = (ModelLoader)Singleton<IModelLoader>();
		IFileSystem fileSystem = Singleton<IFileSystem>();

		Map = null;
		LoadName = null;
		MapFileHandle = null;

		if (model == null)
			MapName = new(loadName);
		else
			MapName = model.StrName.String();

		MapFileHandle = fileSystem.Open(loadName, FileOpenOptions.Read | FileOpenOptions.Binary)?.Stream;
		if (MapFileHandle == null) {
			Host.Error($"MapLoadHelper.Init, unable to open {MapName}");
			return;
		}

		if (!MapFileHandle.ReadToStruct(ref MapHeader) || MapHeader.Identifier != BSPFileCommon.IDBSPHEADER) {
			MapFileHandle.Close();
			MapFileHandle = null;
			Host.Error($"MapLoadHelper.Init, map {MapName} has wrong identifier\n");
			return;
		}

		if (MapHeader.Version < BSPFileCommon.MINBSPVERSION || MapHeader.Version > BSPFileCommon.BSPVERSION) {
			MapFileHandle.Close();
			MapFileHandle = null;
			Host.Error($"MapLoadHelper.Init, map {MapName} has wrong version ({MapHeader.Version} when expecting {BSPFileCommon.BSPVERSION})\n");
			return;
		}

		LoadName = new(loadName);

#if !SWDS
		InitDLightGlobals(MapHeader.Version);
#endif
		Map = ModelLoader.WorldBrushData;

		Assert(MapFileHandle != null);
	}

	public static void Shutdown() {
		if (MapFileHandle != null) {
			MapFileHandle.Close();
			MapFileHandle = null;
		}

		LoadName = null;
		MapName = null;
		memreset(ref MapHeader);
		Map = null;
	}

	private static void InitDLightGlobals(int version) {

	}

	public readonly WorldBrushData GetMap() => Map!;

	public readonly LumpIndex LumpID;
	public readonly int LumpSize;
	public readonly int LumpOffset;
	public readonly int LumpVersion;

	public MapLoadHelper(LumpIndex lumpToLoad) {
		LumpID = lumpToLoad;
		ref BSPLump lump = ref MapHeader.Lumps[(int)lumpToLoad];
		LumpSize = lump.FileLength;
		LumpOffset = lump.FileOffset;
		LumpVersion = lump.Version;
	}

	public byte[] LoadLumpData() {
		ref BSPLump lump = ref MapHeader.Lumps[(int)LumpID];
		return lump.ReadBytes(MapFileHandle!);
	}

	public readonly T[] LoadLumpData<T>(bool throwIfNoElements = false, int maxElements = 0, bool sysErrorIfOOB = false) where T : unmanaged {
		ref BSPLump lump = ref MapHeader.Lumps[(int)LumpID];
		string? error;

		T[]? data = lump.ReadBytes<T>(MapFileHandle!);
		if (data == null) {
			error = $"ModelLoader: funny {LumpID} lump size in {LoadName}";
			goto doError;
		}

		if (throwIfNoElements && data.Length < 1) {
			error = $"ModelLoader: lump {LumpID} has no elements in map {LoadName}";
			goto doError;
		}

		if (maxElements > 0 && data.Length > maxElements) {
			error = $"ModelLoader: lump {LumpID} has too many elements ({data.Length} > {maxElements}) in map {LoadName}";
			goto doError;
		}

		return data;
	doError:
		if (sysErrorIfOOB)
			Singleton<Sys>().Error(error);
		else
			Host!.Error(error);
		return [];
	}
}

public class ModelLoader(Sys Sys, IFileSystem fileSystem, Host Host, IEngineVGuiInternal EngineVGui, MatSysInterface materials, CollisionModelSubsystem CM, CommonHostState host_state) : IModelLoader
{
	public int GetCount() {
		throw new NotImplementedException();
	}

	public void GetExtraData(Model model) {
		throw new NotImplementedException();
	}

	public int GetModelFileSize(ReadOnlySpan<char> name) {
		throw new NotImplementedException();
	}

	public Model GetModelForIndex(int i) {
		throw new NotImplementedException();
	}

	public Model? GetModelForName(ReadOnlySpan<char> name, ModelLoaderFlags referenceType) {
		Model? model = FindModel(name);
		Model? retval = LoadModel(model, ref referenceType);

		return retval;
	}

	InlineArray64<char> ActiveMapName;
	InlineArray64<char> LoadName;

	Model? WorldModel;

	private Model? LoadModel(Model? mod, ref ModelLoaderFlags referenceType) {
		mod!.LoadFlags |= referenceType;

		bool touchAllData = false;
		int serverCount = Host.GetServerCount();
		if (mod.ServerCount != serverCount) {
			mod.ServerCount = serverCount;
			touchAllData = true;
		}

		if (mod.Type == ModelType.Studio && 0 == (mod.LoadFlags & ModelLoaderFlags.LoadedByPreload)) {
			throw new Exception("Studiomodels still need work");
		}

		if ((mod.LoadFlags & ModelLoaderFlags.Loaded) != 0)
			return mod;

		double st = Platform.Time;
		mod.StrName.String()!.FileBase(LoadName);
		DevMsg($"Loading: {mod.StrName.String()}\n");

		mod.Type = GetTypeFromName(mod.StrName);
		if (mod.Type == ModelType.Invalid)
			mod.Type = ModelType.Studio;

		switch (mod.Type) {
			case ModelType.Sprite: throw new NotImplementedException("ModelType.Sprite unsupported for now");
			case ModelType.Studio: throw new NotImplementedException("ModelType.Studio unsupported for now");
			case ModelType.Brush: {
					fileSystem.AddSearchPath(mod.StrName, "GAME", SearchPathAdd.ToHead);

					// exclude textures later

					strcpy(ActiveMapName, mod.StrName);

					fileSystem.BeginMapAccess();
					Map_LoadModel(mod);
					fileSystem.EndMapAccess();
				}
				break;
		}
		return mod;
	}

	int MapLoadCount;
	public readonly WorldBrushData WorldBrushData = new();

	private void Map_LoadModel(Model mod) {
		MapLoadCount++;
		double startTime = Platform.Time;

#if !SWDS
		EngineVGui.UpdateProgressBar(LevelLoadingProgress.LoadWorldModel);
#endif

		SetWorldModel(mod);
		mod.Brush.Shared = WorldBrushData;
		mod.Brush.RenderHandle = 0;

		Common.TimestampedLog("Loading map");
		CM.LoadMap(mod.StrName, false, out uint checksum);

		mod.Type = ModelType.Brush;
		mod.LoadFlags |= ModelLoaderFlags.Loaded;
		MapLoadHelper.Init(mod, ((Span<char>)(ActiveMapName)).SliceNullTerminatedString());

		Mod_LoadVertices();
		BSPEdge[] edges = Mod_LoadEdges();
		Mod_LoadSurfedges(edges);
		Mod_LoadPlanes();
		// Mod_LoadOcclusion();
		Mod_LoadTexdata();
		Mod_LoadTexinfo();

#if !SWDS
		EngineVGui.UpdateProgressBar(LevelLoadingProgress.LoadWorldModel);
#endif

		Mod_LoadPrimitives();
		Mod_LoadPrimVerts();
		Mod_LoadPrimIndices();

#if !SWDS
		EngineVGui.UpdateProgressBar(LevelLoadingProgress.LoadWorldModel);
#endif

		Mod_LoadFaces();
		Mod_LoadVertNormals();
		Mod_LoadVertNormalIndices();

		MapLoadHelper.Shutdown();
		double elapsed = Platform.Time - startTime;
		Common.TimestampedLog($"Map_LoadModel: Finish - loading took {elapsed:F4} seconds");
	}

	private void Mod_LoadPrimitives() { }
	private void Mod_LoadPrimVerts() { }
	private void Mod_LoadPrimIndices() { }
	private ref BSPMSurface2 SurfaceHandleFromIndex(int surfaceIndex, WorldBrushData? data = null) {
		return ref (data ?? host_state.WorldBrush)!.Surfaces2[surfaceIndex];
	}
	public static ref CollisionPlane MSurf_Plane(ref BSPMSurface2 surfID) => ref surfID.Plane.GetReference();
	public static ref int MSurf_FirstVertIndex(ref BSPMSurface2 surfID) => ref surfID.FirstVertIndex;
	public static ref SurfDraw MSurf_Flags(ref BSPMSurface2 surfID) => ref surfID.Flags;
	public static int MSurf_VertCount(ref BSPMSurface2 surfID) => (int)(((uint)surfID.Flags >> (int)SurfDraw.VertCountShift) & (uint)SurfDraw.VertCountMask);
	public static void MSurf_SetVertCount(ref BSPMSurface2 surfID, uint vertCount) {
		uint flags = (vertCount << (int)SurfDraw.VertCountShift) & (uint)SurfDraw.VertCountMask;
		surfID.Flags |= (SurfDraw)flags;
	}

	private void Mod_LoadFaces() {
		MapLoadHelper lh = new MapLoadHelper(LumpIndex.TexInfo);
		BSPFace[] inFaces = lh.LoadLumpData<BSPFace>();

		int count = inFaces.Length;
		BSPMSurface1[] out1 = new BSPMSurface1[count];
		BSPMSurface2[] out2 = new BSPMSurface2[count];

		WorldBrushData brushData = lh.GetMap();
		brushData.Surfaces1 = out1;
		brushData.Surfaces2 = out2;
		brushData.NumSurfaces = count;

		int ti, di;

		for (int surfnum = 0; surfnum < count; surfnum++) {
			ref readonly BSPFace _in = ref inFaces[surfnum];
			ref BSPMSurface1 _out1 = ref out1[surfnum];
			ref BSPMSurface2 _out2 = ref out2[surfnum];

			ref BSPMSurface2 surfID = ref SurfaceHandleFromIndex(surfnum, brushData);

			MSurf_FirstVertIndex(ref surfID) = _in.FirstEdge;

			int vertCount = _in.NumEdges;
			MSurf_Flags(ref surfID) = 0;
			Assert(vertCount <= 255);
			MSurf_SetVertCount(ref surfID, (uint)vertCount);

			int planenum = _in.PlaneNum;
			if (_in.OnNode != 0)
				MSurf_Flags(ref surfID) |= SurfDraw.Node;

			if (_in.Side != 0)
				MSurf_Flags(ref surfID) |= SurfDraw.PlaneBack;

			_out2.Plane = lh.GetMap().Planes![planenum];

			ti = _in.TexInfo;
			if (ti < 0 || ti >= lh.GetMap().NumTexInfo) 
				Host.Error("Mod_LoadFaces: bad texinfo number");
			
			surfID.TexInfo = (ushort)ti;
			surfID.DynamicShadowsEnabled = _in.AreDynamicShadowsEnabled();
			ref ModelTexInfo tex = ref lh.GetMap().TexInfo![ti];

			if (tex.Material == null)
				tex.Material = materials.MaterialEmpty;

			// TODO
			// if (Mod_LoadSurfaceLightingV1(pLighting, _in, lh.GetMap().LightData)) 
			// MSurf_Flags(ref surfID) |= SurfDraw.HasLightStyles;

			// set the drawing flags flag
			if ((tex.Flags & Surf.NoLight) != 0)
				MSurf_Flags(ref surfID) |= SurfDraw.NoLight;

			if ((tex.Flags & Surf.NoShadows) != 0)
				MSurf_Flags(ref surfID) |= SurfDraw.NoShadows;

			if ((tex.Flags & Surf.Warp) != 0)
				MSurf_Flags(ref surfID) |= SurfDraw.WaterSurface;

			if ((tex.Flags & Surf.Sky) != 0)
				MSurf_Flags(ref surfID) |= SurfDraw.Sky;

			// todo: disp info
			// todo: primitives
		}
	}
	
	private void Mod_LoadVertNormals() { }
	private void Mod_LoadVertNormalIndices() { }
	private void Mod_LoadTexdata() {
		MapLoadHelper.Map!.NumTexData = GetCollisionBSPData().NumSurfaces;
		MapLoadHelper.Map!.TexData = GetCollisionBSPData().MapSurfaces.Base();
	}

	private void Mod_LoadTexinfo() {
		MapLoadHelper lh = new MapLoadHelper(LumpIndex.TexInfo);
		BSPTexInfo[] inTexInfo = lh.LoadLumpData<BSPTexInfo>();
		ModelTexInfo[] outTexInfo = new ModelTexInfo[inTexInfo.Length];

		lh.GetMap().TexInfo = outTexInfo;
		lh.GetMap().NumTexInfo = inTexInfo.Length;

		bool loadtextures = true; // << todo: convar
		for (int i = 0; i < outTexInfo.Length; ++i) {
			ref BSPTexInfo _in = ref inTexInfo[i];
			ref ModelTexInfo _out = ref outTexInfo[i];
			for (int j = 0; j < 2; ++j) {
				for (int k = 0; k < 4; ++k) {
					_out.TextureVecsTexelsPerWorldUnits[j][k] = _in.TextureVecsTexelsPerWorldUnits[j][k];
					_out.LightmapVecsLuxelsPerWorldUnits[j][k] = _in.LightmapVecsLuxelsPerWorldUnits[j][k];
				}
			}

			_out.LuxelsPerWorldUnit = _out.LightmapVecsLuxelsPerWorldUnits[0].AsVector3().Length();
			_out.WorldUnitsPerLuxel = 1.0f / _out.LuxelsPerWorldUnit;

			_out.Flags = (Surf)_in.Flags;
			_out.TexInfoFlags = 0;

			if (loadtextures) {
				if (_in.TexData >= 0)
					_out.Material = materials.GL_LoadMaterial(lh.GetMap().TexData![_in.TexData].Name, MaterialDefines.TEXTURE_GROUP_WORLD);
				else {
					DevMsg($"Mod_LoadTexinfo: texdata < 0 (index=={i}/{outTexInfo.Length})\n");
					_out.Material = null;
				}
				if (_out.Material == null)
					_out.Material = materials.MaterialEmpty;
			}
			else
				_out.Material = materials.MaterialEmpty;
		}
	}

	private void Mod_LoadPlanes() {
		MapLoadHelper.Map!.Planes = GetCollisionBSPData().MapPlanes.Base();
		MapLoadHelper.Map!.NumPlanes = GetCollisionBSPData().NumPlanes;
	}

	private void Mod_LoadVertices() {
		MapLoadHelper lh = new MapLoadHelper(LumpIndex.Vertexes);
		lh.GetMap().Vertexes = lh.LoadLumpData<BSPVertex>();
	}

	private BSPEdge[] Mod_LoadEdges() {
		MapLoadHelper lh = new MapLoadHelper(LumpIndex.Edges);
		BSPEdge[] outData = lh.LoadLumpData<BSPEdge>();
		return outData;
	}
	private void Mod_LoadSurfedges(BSPEdge[] edges) {
		MapLoadHelper lh = new MapLoadHelper(LumpIndex.SurfEdges);
		int[] inData = lh.LoadLumpData<int>(throwIfNoElements: true, maxElements: BSPFileCommon.MAX_MAP_SURFEDGES);
		ushort[] outData = new ushort[inData.Length]; ;
		lh.GetMap().VertIndices = outData;

		for (int i = 0; i < outData.Length; i++) {
			int edge = inData[i];
			int index = 0;
			if (edge < 0) {
				edge = -edge;
				index = 1;
			}
			outData[i] = edges[edge].V[index];
		}
	}
	public void SetWorldModel(Model mod) {
		WorldModel = mod;
	}

	public void ClearWorldModel() {
		WorldModel = null;
	}



	private ModelType GetTypeFromName(ReadOnlySpan<char> modelName) => modelName.GetFileExtension() switch {
		"spr" or "vmt" => ModelType.Sprite,
		"bsp" => ModelType.Brush,
		"mdl" => ModelType.Studio,
		_ => ModelType.Invalid
	};

	readonly List<Model> InlineModels = [];
	readonly Dictionary<FileNameHandle_t, Model> Models = [];

	private Model? FindModel(ReadOnlySpan<char> name) {
		if (name.IsEmpty)
			Sys.Error("ModelLoader.FindModel: NULL name");


		if (name[0] == '*') {
			int.TryParse(name[1..], out int modelNum);
			if (IsWorldModelSet())
				Sys.Error($"bad inline model number {modelNum}, worldmodel not yet setup");

			if (modelNum < 1 || modelNum >= GetNumWorldSubmodels())
				Sys.Error($"bad inline model number {modelNum}");

			return InlineModels[modelNum];
		}

		Model? model = null;

		FileNameHandle_t fnHandle = fileSystem.FindOrAddFileName(name);

		if (!Models.TryGetValue(fnHandle, out model)) {
			model = new() {
				FileNameHandle = fnHandle,
				LoadFlags = ModelLoaderFlags.NotLoadedOrReferenced,
				StrName = name
			};

			Models[fnHandle] = model;
		}

		Assert(model);

		return model;
	}

	private bool IsWorldModelSet() {
		throw new NotImplementedException();
	}

	private int GetNumWorldSubmodels() {
		throw new NotImplementedException();
	}

	public ReadOnlySpan<char> GetName(Model model) {
		throw new NotImplementedException();
	}

	public void Init() {
		throw new NotImplementedException();
	}

	public void PurgeUnusedModels() {
		throw new NotImplementedException();
	}

	public Model? ReferenceModel(ReadOnlySpan<char> name, ModelLoaderFlags referenceType) {
		throw new NotImplementedException();
	}

	public void ResetModelServerCounts() {

	}

	public void UnreferenceAllModels(ModelLoaderFlags referenceType) {
		throw new NotImplementedException();
	}

	public void UnreferenceModel(Model model, ModelLoaderFlags referenceType) {
		throw new NotImplementedException();
	}
}