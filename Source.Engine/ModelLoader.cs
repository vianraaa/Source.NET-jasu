using Source.Common.Engine;
using Source.Common;
using Source.Common.Filesystem;
using Source.Common.Utilities;
using Source.Common.Formats.BSP;
using Source.Common.MaterialSystem;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Source.Engine;

public ref struct MapLoadHelper
{
	static WorldBrushData? Map;
	static string? LoadName;
	static string? MapName;
	static Stream? MapFileHandle;
	static BSPHeader MapHeader;

	public static void Init(Model? model, ReadOnlySpan<char> loadName) {
		Host Host = Singleton<Host>();
		ModelLoader ModelLoader = (ModelLoader)Singleton<IModelLoader>();
		IFileSystem fileSystem = Singleton<IFileSystem>();

		Map = null;
		LoadName = null;
		MapFileHandle = null;

		if (model == null)
			MapName = new(loadName);
		else
			MapName = model.StrName.String();

		MapFileHandle = fileSystem.Open($"maps/{loadName}.bsp", FileOpenOptions.Read | FileOpenOptions.Binary)?.Stream;
		if(MapFileHandle == null) {
			Host.Error($"MapLoadHelper.Init, unable to open {MapName}");
			return;
		}

		if(!MapFileHandle.ReadToStruct(ref MapHeader) || MapHeader.Identifier != BSPFileCommon.IDBSPHEADER) {
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

	private static void InitDLightGlobals(int version) {

	}

	public readonly LumpIndex LumpID;
	public readonly int LumpSize;
	public readonly int LumpOffset;
	public readonly int LumpVersion;
	
	public MapLoadHelper(LumpIndex lumpToLoad) {
		LumpID = lumpToLoad;
		ref BSPLump lump = ref MapHeader.Lumps[(int)lumpToLoad];
		LumpSize = lump.FileLength;
		LumpSize = lump.FileOffset;
		LumpVersion = lump.Version;
	}

	public byte[] LoadLumpData() {
		ref BSPLump lump = ref MapHeader.Lumps[(int)LumpID];
		return lump.ReadBytes(MapFileHandle!);
	}

	public Span<T> LoadLumpData<T>() where T : unmanaged {
		ref BSPLump lump = ref MapHeader.Lumps[(int)LumpID];
		return MemoryMarshal.Cast<byte, T>(lump.ReadBytes(MapFileHandle!));
	}
}

public class ModelLoader(Sys Sys, IFileSystem fileSystem, Host Host, IEngineVGuiInternal EngineVGui) : IModelLoader
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
		mod.StrName.String().FileBase(LoadName);
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

		EngineVGui.UpdateProgressBar(LevelLoadingProgress.LoadWorldModel);

		SetWorldModel(mod);
		mod.Brush.Shared = WorldBrushData;
		mod.Brush.RenderHandle = 0;

		Common.TimestampedLog("Loading map");
		mod.Type = ModelType.Brush;
		mod.LoadFlags |= ModelLoaderFlags.Loaded;
		MapLoadHelper.Init(mod, ((Span<char>)(LoadName)).SliceNullTerminatedString());

		Mod_LoadVertices();
	}

	private void Mod_LoadVertices() {
		MapLoadHelper lh = new MapLoadHelper(LumpIndex.Vertexes);
		ReadOnlySpan<BSPVertex> inData = lh.LoadLumpData<BSPVertex>();
		BSPVertex[] outData = inData.ToArray();
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
		if (name == null || name.Length <= 0)
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