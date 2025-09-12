using Source.Common.Engine;

namespace Source.Common;
public enum ModelReferenceType
{
	NotLoadedOrReferenced = 0,
	Loaded = 1 << 0,
	Server = 1 << 1,
	Client = 1 << 2,
	ClientDLL = 1 << 3,
	StaticProp = 1 << 4,
	DetailProp = 1 << 5,
	DynServer = 1 << 6,
	DynClient = 1 << 7,
	Dynamic = DynServer | DynClient,
	ReferenceMask = Server | Client | ClientDLL | StaticProp | DetailProp | Dynamic,
	TouchedByPreload = 1 << 15,
	LoadedByPreload = 1 << 16,
	TouchedMaterials = 1 << 17,
};
public interface IModelLoader {
	void Init();
	int GetCount();
	Model GetModelForIndex(int i);
	ReadOnlySpan<char> GetName(Model model);
	void GetExtraData(Model model);
	int GetModelFileSize(ReadOnlySpan<char> name);
	Model? GetModelForName(ReadOnlySpan<char> name, ModelReferenceType referenceType);
	Model? ReferenceModel(ReadOnlySpan<char> name, ModelReferenceType referenceType);
	void UnreferenceModel(Model model, ModelReferenceType referenceType);
	void UnreferenceAllModels(ModelReferenceType referenceType);
	void ResetModelServerCounts();
	void PurgeUnusedModels();
}

public class ModelLoader : IModelLoader
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
		throw new NotImplementedException();
	}

	public void UnreferenceAllModels(ModelReferenceType referenceType) {
		throw new NotImplementedException();
	}

	public void UnreferenceModel(Model model, ModelReferenceType referenceType) {
		throw new NotImplementedException();
	}
}