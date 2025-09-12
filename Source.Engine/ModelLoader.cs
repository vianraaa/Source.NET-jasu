using Source.Common.Engine;
using Source.Common;
using Source.Common.Filesystem;

namespace Source.Engine;


public class ModelLoader(Sys Sys, IFileSystem fileSystem) : IModelLoader
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

	public Model? GetModelForName(ReadOnlySpan<char> name, ModelReferenceType referenceType) {
		Model? model = FindModel(name);
		Model? retval = LoadModel(model, ref referenceType);

		return retval;
	}

	private Model? LoadModel(Model? model, ref ModelReferenceType referenceType) {
		throw new NotImplementedException();
	}

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
				LoadFlags = ModelReferenceType.NotLoadedOrReferenced,
				Name = new(name)
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

	public Model? ReferenceModel(ReadOnlySpan<char> name, ModelReferenceType referenceType) {
		throw new NotImplementedException();
	}

	public void ResetModelServerCounts() {

	}

	public void UnreferenceAllModels(ModelReferenceType referenceType) {
		throw new NotImplementedException();
	}

	public void UnreferenceModel(Model model, ModelReferenceType referenceType) {
		throw new NotImplementedException();
	}
}