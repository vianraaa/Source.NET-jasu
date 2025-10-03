using CommunityToolkit.HighPerformance;

using SharpCompress.Common;

using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;
using Source.Common.ShaderLib;
using Source.Common.Utilities;

using Steamworks;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Source.MaterialSystem;

public enum MaterialFlags : ushort
{
	NeedsWhiteLightmap = 0x1,
	IsPrecached = 0x2,
	VarsIsPrecached = 0x4,
	ValidRenderState = 0x8,
	IsManuallyCreated = 0x10,
	UsesUNCFilename = 0x20,
	IsPReloaded = 0x40,
	ArtificalRefCount = 0x80,
}

public class Material : IMaterialInternal
{
	public readonly MaterialSystem materials;
	public MaterialVarFlags GetMaterialVarFlags() {
		if (ShaderParams != null && VarCount > (int)ShaderMaterialVars.Flags) {
			IMaterialVar var = ShaderParams[(int)ShaderMaterialVars.Flags];
			return (MaterialVarFlags)var.GetIntValue();
		}

		return 0;
	}

	public bool IsErrorMaterialInternal() {
		return materials.errorMaterial == this;
	}

	public MaterialVarFlags2 GetMaterialVarFlags2() {
		if (ShaderParams != null && VarCount > (int)ShaderMaterialVars.Flags2) {
			IMaterialVar var = ShaderParams[(int)ShaderMaterialVars.Flags2];
			return (MaterialVarFlags2)var.GetIntValue();
		}

		return 0;
	}

	public bool IsUsingVertexID() {
		return (GetMaterialVarFlags2() & MaterialVarFlags2.UsesVertexID) != 0;
	}

	IShaderAPI ShaderAPI => materials.ShaderAPI;

	public Material(MaterialSystem materials, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName, KeyValues? keyValues) {
		Span<char> temp = stackalloc char[materialName.Length];
		materialName.ToLowerInvariant(temp);
		temp = temp.StripExtension(temp);

		this.materials = materials;
		name = new(temp);
		texGroupName = new(textureGroupName);
		this.keyValues = keyValues;
		if (keyValues != null) {
			flags |= MaterialFlags.IsManuallyCreated;
		}
		ShaderParams = null;
		MappingWidth = MappingHeight = 0;
		if (keyValues != null) {
			flags |= MaterialFlags.IsManuallyCreated;
		}

		ShaderRenderState = new(materials.ShaderAPI, materialName);
	}

	public int MappingWidth;
	public int MappingHeight;

	public ReadOnlySpan<char> GetName() {
		return name;
	}

	public bool IsManuallyCreated() {
		return (flags & MaterialFlags.IsManuallyCreated) != 0;
	}

	public bool IsPrecached() {
		return (flags & MaterialFlags.IsPrecached) != 0;
	}
	public bool IsPrecachedVars() {
		return (flags & MaterialFlags.VarsIsPrecached) != 0;
	}

	public void Precache() {
		if (IsPrecached())
			return;

		if (!PrecacheVars())
			return;

		flags |= MaterialFlags.IsPrecached;
		if (Shader != null)
			materials.ShaderSystem.InitShaderInstance(Shader, ShaderParams, GetName(), GetTextureGroupName());

		RecomputeStateSnapshots();
		FindRepresentativeTexture();
		PrecacheMappingDimensions();
	}

	public void RecomputeStateSnapshots() {
		bool ok = InitializeRenderState();
		if (!ok)
			SetupErrorShader();
	}

	private bool InitializeRenderState() {
		if (IsPrecached()) {
			if (materials.GetCurrentMaterial() == this) {
				ShaderAPI.FlushBufferedPrimitives();
			}

			if (Shader != null && !materials.ShaderSystem.InitRenderState(Shader, ShaderParams, ref ShaderRenderState, GetName())) {
				flags &= ~MaterialFlags.ValidRenderState;
				return false;
			}

			flags |= MaterialFlags.ValidRenderState;
		}

		return true;
	}

	private void SetupErrorShader() {
		throw new NotImplementedException();
	}

	static int complainCount = 0;
	public IMaterialVar FindVar(ReadOnlySpan<char> varName, out bool found, bool complain = true) {
		Span<char> lowercased = stackalloc char[varName.Length];
		varName.ToLowerInvariant(lowercased);
		ReadOnlySpan<char> lowercasedROS = lowercased; // Need to make a version of that that works on Span...
		foreach (var shaderParam in Vars) {
			if (shaderParam.GetName().Hash() == lowercasedROS.Hash()) {
				found = true;
				return shaderParam;
			}
		}
		found = false;
		if (complain) {
			if (complainCount < 100) {
				Warning($"No such variable \"{varName}\" for material \"{GetName()}\"\n");
				complainCount++;
			}
		}
		return GetDummyVariable();
	}

	static IMaterialVar? dummyVar;
	private IMaterialVar GetDummyVariable() {
		dummyVar ??= new MaterialVar(null, "$dummyVar", 0);
		return dummyVar;
	}

	// A more C#-style way of doing this
	public bool TryFindVar(ReadOnlySpan<char> varName, [NotNullWhen(true)] out IMaterialVar? found, bool complain = true) {
		bool hasFound;
		found = FindVar(varName, out hasFound, complain);
		return hasFound;
	}

	private void FindRepresentativeTexture() {
		Precache();
		bool found;
		IMaterialVar? textureVar = FindVar("$basetexture", out found, false);
		if (found && textureVar.GetVarType() == MaterialVarType.Texture) {
			ITextureInternal? texture = (ITextureInternal?)textureVar.GetTextureValue();
			if (representativeTexture != null)
				representativeTexture.Precache();
			else {
				representativeTexture = materials.TextureSystem.ErrorTexture();
				Assert(representativeTexture);
			}
		}
		if (!found || textureVar.GetVarType() != MaterialVarType.Texture) {
			textureVar = FindVar("$envmapmask", out found, false);
			if (!found || textureVar.GetVarType() != MaterialVarType.Texture) {
				textureVar = FindVar("$bumpmap", out found, false);
				if (!found || textureVar.GetVarType() != MaterialVarType.Texture) {
					textureVar = FindVar("$dudvmap", out found, false);
					if (!found || textureVar.GetVarType() != MaterialVarType.Texture) {
						textureVar = FindVar("$normalmap", out found, false);
						if (!found || textureVar.GetVarType() != MaterialVarType.Texture) {
							representativeTexture = materials.TextureSystem.ErrorTexture();
							return;
						}
					}
				}
			}
		}

		representativeTexture = (ITextureInternal?)textureVar.GetTextureValue();
		if (representativeTexture != null)
			representativeTexture.Precache();
		else {
			representativeTexture = materials.TextureSystem.ErrorTexture();
			Assert(representativeTexture);
		}
	}

	private void PrecacheMappingDimensions() {
		if (representativeTexture == null) {
			MappingWidth = 64;
			MappingHeight = 64;
		}
		else {
			MappingWidth = representativeTexture.GetMappingWidth();
			MappingHeight = representativeTexture.GetMappingHeight();
		}
	}

	readonly List<FileNameHandle_t> VMTIncludes = [];

	private string GetTextureGroupName() {
		return "";
	}

	public static bool LoadVMTFile(IFileSystem fileSystem, KeyValues keyValues, KeyValues patchKeyValues, ReadOnlySpan<char> materialName, bool absolutePath, List<FileNameHandle_t>? includes = null) {
		Span<char> fileName = stackalloc char[MAX_PATH];
		ReadOnlySpan<char> pathID = "GAME";
		if (!absolutePath) {
			sprintf(fileName, "materials/%s.vmt", new string(materialName));
		}
		else {
			sprintf(fileName, "%s.vmt", new string(materialName));
			if (materialName[0] == '/' && materialName[1] == '/' && materialName[2] != '/') {
				pathID = null;
			}
		}

		if (!keyValues.LoadFromFile(fileSystem, fileName[..fileName.IndexOf('\0')], pathID)) {
			return false;
		}
		// ExpandPatchFile(keyValues, patchKeyValues, pathID, includes);

		return true;
	}


	public bool PrecacheVars(KeyValues? inVmtKeyValues = null, KeyValues? inPatchKeyValues = null, List<FileNameHandle_t>? includes = null, MaterialFindContext findContext = 0) {
		if (IsPrecachedVars())
			return true;

		// How should we load VMT includes?
		// How should we allow async?

		bool ok = false;
		bool error = false;
		KeyValues? vmtKeyValues = null;
		KeyValues? patchKeyValues = null;

		if (keyValues != null) {
			vmtKeyValues = keyValues;
			patchKeyValues = new("vmt_patches");
		}
		else if (inVmtKeyValues != null) {
			vmtKeyValues = inVmtKeyValues;
			patchKeyValues = inPatchKeyValues;
		}
		else {
			VMTIncludes.Clear();

			vmtKeyValues = new KeyValues("vmt");
			patchKeyValues = new KeyValues("vmt_patches");
			if (!LoadVMTFile(materials.FileSystem, vmtKeyValues, patchKeyValues, GetName(), false, VMTIncludes)) {
				Warning($"CMaterial::PrecacheVars: error loading vmt file for {GetName()}\n");
				error = true;
			}
		}

		if (!error) {
			flags |= MaterialFlags.VarsIsPrecached;
			KeyValues? fallbackKeyValues = InitializeShader(vmtKeyValues!, patchKeyValues, findContext);
			if (fallbackKeyValues != null) {
				InitializeMaterialProxy(fallbackKeyValues);
				ok = true;
			}
		}

		return ok;
	}

	private void InitializeMaterialProxy(KeyValues fallbackKeyValues) {

	}

	private KeyValues? InitializeShader(KeyValues keyValues, KeyValues? patchKeyValues, MaterialFindContext findContext) {
		KeyValues currentFallback = keyValues;
		KeyValues? fallbackSection = null;

		string? shaderName = currentFallback.Name;
		if (shaderName == null) {
			Warning($"Shader not specified in material {GetName()}\nUsing wireframe instead...\n");
			Assert(false);
			shaderName = MissingShaderName();
		}

		IShader? shader;
		IMaterialVar[] vars = new IMaterialVar[256];
		string fallbackShaderName = "";
		string fallbackMaterialName = "";
		bool modelDefault = false;
		int varCount;

		while (true) {
			shader = materials.ShaderSystem.FindShader(shaderName);
			if (shader == null) {
				Dbg.Warning($"Error: Material \"{GetName()}\" uses unknown shader \"{shaderName}\"\n");

				shaderName = MissingShaderName();
				shader = materials.ShaderSystem.FindShader(shaderName);
				if (shader == null)
					return null;
			}

			varCount = ParseMaterialVars(shader, keyValues, fallbackSection, null, modelDefault, vars, findContext);
			if (shader == null)
				break;

			materials.ShaderSystem.InitShaderParameters(shader, vars, GetName(), null);

			shaderName = shader.GetFallbackShader(vars);
			if (shaderName == null)
				break;

			if (true) { // Yeah, we support vertex and pixel shaders... do we even really need a flag for that....
				modelDefault = (vars[(int)ShaderMaterialVars.Flags].GetIntValue() & (int)MaterialVarFlags.Model) != 0;
			}

			for (int i = 0; i < varCount; i++) {
				vars[i] = null;
			}
		}

		this.Shader = shader;
		this.ShaderParams = vars;
		this.VarCount = varCount;

		return currentFallback;
	}

	private int ParseMaterialVars(IShader shader, KeyValues keyValues, KeyValues? fallbackSection, KeyValues? overrideKeyValues, bool modelDefault, IMaterialVar[] vars, MaterialFindContext findContext) {
		IMaterialVar? newVar;
		Span<bool> overrides = stackalloc bool[256];
		Span<bool> conditional = stackalloc bool[256];
		int overrideMask = 0;
		int flagMask = 0;

		int modelFlag = modelDefault ? (int)MaterialVarFlags.Model : 0;
		vars[(int)ShaderMaterialVars.Flags] = new MaterialVar(this, "$flags", modelFlag);
		vars[(int)ShaderMaterialVars.FlagsDefined] = new MaterialVar(this, "$flags_defined", modelFlag);
		vars[(int)ShaderMaterialVars.Flags2] = new MaterialVar(this, "flags2", 0);
		vars[(int)ShaderMaterialVars.FlagsDefined2] = new MaterialVar(this, "$flags_defined2", 0);

		int numParams = shader == null ? 0 : shader.GetNumParams();
		int varCount = numParams;

		bool parsingOverrides = overrideKeyValues != null;
		var var = overrideKeyValues != null ? overrideKeyValues.GetFirstSubKey() : keyValues.GetFirstSubKey();

		ReadOnlySpan<char> matName = var != null ? var.GetString() : "Unknown";
		while (var != null) {
			bool processThisOne = true;
			bool isConditionalVar;
			ReadOnlySpan<char> varName = GetVarName(var);

			if (findContext == MaterialFindContext.IsOnAModel && varName != null && varName.Length > 0) {
				if (varName.Contains("$ignorez", StringComparison.OrdinalIgnoreCase)) {
					Warning($"Ignoring material flag '{varName}' on material '{matName}'.\n");
					goto nextVar;
				}
			}

			if (
				ShouldSkipVar(var, out isConditionalVar) ||
				(var.Name[0] == '%') ||
				ParseMaterialFlag(var, vars[(int)ShaderMaterialVars.Flags], vars[(int)ShaderMaterialVars.FlagsDefined], parsingOverrides, ref flagMask, ref overrideMask) ||
				ParseMaterialFlag(var, vars[(int)ShaderMaterialVars.Flags2], vars[(int)ShaderMaterialVars.FlagsDefined2], parsingOverrides, ref flagMask, ref overrideMask)
				)
				processThisOne = false;

			if (processThisOne) {
				int varIdx = FindMaterialVar(shader, varName);
				if (varIdx >= 0) {
					if (vars[varIdx] != null && (!isConditionalVar)) {
						if (!overrides[varIdx] || parsingOverrides) {
							Warning($"Error! Variable \"{var.Name}\" is multiply defined in material \"{GetName()}\"!\n");
						}
						goto nextVar;
					}
				}
				else {
					int i;
					for (i = numParams; i < varCount; ++i) {
						Assert(vars[i] != null);
						if (vars[i].GetName().Equals(var.Name, StringComparison.OrdinalIgnoreCase))
							break;
					}

					if (i != varCount) {
						if (!overrides[varIdx] || parsingOverrides) {
							Warning($"Error! Variable \"{var.Name}\" is multiply defined in material \"{GetName()}\"!\n");
						}
						goto nextVar;
					}
				}

				newVar = CreateMaterialVarFromKeyValue(this, var);
				if (newVar == null)
					goto nextVar;

				if (varIdx < 0)
					varIdx = varCount++;

				vars[varIdx] = newVar;
				if (parsingOverrides)
					overrides[varIdx] = true;

				conditional[varIdx] = isConditionalVar;
			}

		nextVar:
			var = var.GetNextKey();
			if (var != null && parsingOverrides) {
				var = keyValues.GetFirstSubKey();
				parsingOverrides = false;
			}
		}

		for (int i = 0; i < numParams; i++) {
			if (vars[i] == null)
				vars[i] = new MaterialVar(this, shader!.GetParamName(i));
		}

		return varCount;
	}

	private static IMaterialVar? CreateMaterialVarFromKeyValue(Material material, KeyValues keyValue) {
		ReadOnlySpan<char> name = GetVarName(keyValue);
		switch (keyValue.Type) {
			case KeyValues.Types.Int: return new MaterialVar(material, name, keyValue.GetInt());
			case KeyValues.Types.Double: return new MaterialVar(material, name, keyValue.GetFloat());
			case KeyValues.Types.String:
				ReadOnlySpan<char> str = keyValue.GetString();
				if (str == null || str.Length == 0)
					return null;

				IMaterialVar? matrixVar = CreateMatrixVarFromKeyValue(material, keyValue);
				if (matrixVar != null) return matrixVar;

				if (!IsVector(str))
					return new MaterialVar(material, name, str);

				return CreateVectorMaterialVarFromKeyValue(material, keyValue);
		}

		return null;
	}

	private static IMaterialVar? CreateVectorMaterialVarFromKeyValue(Material material, KeyValues keyValue) {
		throw new NotImplementedException();
	}

	private static IMaterialVar? CreateMatrixVarFromKeyValue(Material material, KeyValues keyValue) {
		ReadOnlySpan<char> scan = keyValue.GetString();
		ReadOnlySpan<char> name = GetVarName(keyValue);
		return null; // TODO: implement
	}

	private static bool IsVector(ReadOnlySpan<char> str) {
		while (char.IsWhiteSpace(str[0])) {
			str = str[1..];
			if (str.Length == 0 || str[0] == '\n')
				return false;
		}

		return str[0] == '[' || str[0] == '{';
	}

	private int FindMaterialVar(IShader? shader, ReadOnlySpan<char> varName) {
		if (shader == null)
			return -1;

		varName = varName.Trim();
		for (int i = shader.GetNumParams(); --i >= 0;) {
			ReadOnlySpan<char> paramName = shader.GetParamName(i);
			int foundIndex = varName.IndexOf(paramName, StringComparison.OrdinalIgnoreCase);
			if (foundIndex != 0)
				continue;

			int len = paramName.Length;
			int pFound = len;

			while (true) {
				if (pFound >= varName.Length)
					return i;

				if (!char.IsWhiteSpace(varName[pFound]))
					break;

				pFound++;
			}
		}

		return -1;
	}

	private bool ParseMaterialFlag(KeyValues parseValue, IMaterialVar flagVar, IMaterialVar flagDefinedVar, bool parsingOverrides, ref int flagMask, ref int overrideMask) {
		int flagbit = (int)FindMaterialVarFlag(GetVarName(parseValue));
		if (flagbit == 0)
			return false;

		MaterialVarFlags dbg = (MaterialVarFlags)flagbit;
		int testMask = parsingOverrides ? overrideMask : flagMask;
		if ((testMask & flagbit) != 0) {
			Warning($"Error! Flag \"{parseValue.Name}\" is multiply defined in material \"{GetName()}\"!\n");
			return true;
		}

		if ((overrideMask & flagbit) != 0)
			return true;

		if (parsingOverrides)
			overrideMask |= flagbit;
		else
			flagMask |= flagbit;

		if (parseValue.GetInt() != 0)
			flagVar.SetIntValue(flagVar.GetIntValue() | flagbit);
		else
			flagVar.SetIntValue(flagVar.GetIntValue() & (~flagbit));

		flagDefinedVar.SetIntValue(flagDefinedVar.GetIntValue() | flagbit);
		return true;
	}

	private MaterialVarFlags FindMaterialVarFlag(ReadOnlySpan<char> flagName) {
		flagName = flagName.Trim();

		for (int i = 0; materials.ShaderSystem.ShaderStateString(i) != null; ++i) {
			string stateString = materials.ShaderSystem.ShaderStateString(i);
			int foundIndex = flagName.IndexOf(stateString, StringComparison.OrdinalIgnoreCase);

			if (foundIndex != 0)
				continue;

			int nLen = stateString.Length;
			int pFound = nLen;

			while (true) {
				if (pFound >= flagName.Length)
					return (MaterialVarFlags)(1 << i);

				if (!char.IsWhiteSpace(flagName[pFound]))
					break;

				pFound++;
			}
		}

		return 0;
	}

	private bool ShouldSkipVar(KeyValues var, out bool isConditionalVar) {
		isConditionalVar = false; // TODO
		return false;
	}

	public static ReadOnlySpan<char> GetVarName(KeyValues value) {
		ReadOnlySpan<char> name = value.Name;
		int qIndex = name.IndexOf('?');
		if (qIndex == -1)
			return name;

		return name[(qIndex + 1)..];
	}

	private string MissingShaderName() {
		return "Wireframe_GL46";
	}

	MaterialFlags flags;
	UtlSymbol name;
	string texGroupName;
	IShader? Shader;
	KeyValues? keyValues;
	IMaterialVar[]? ShaderParams;
	private int VarCount;
	ITextureInternal? representativeTexture;
	Vector3 Reflectivity;
	uint ChangeID;

	public Span<IMaterialVar> Vars => new Span<IMaterialVar>(ShaderParams)[..VarCount];

	// IMaterialProxy
	ShadowState ShaderRenderState;
	static uint DebugVarsSignature = 0;

	public void DrawMesh(VertexCompressionType vertexCompression) {
		if (Shader != null) {
			if ((GetMaterialVarFlags() & MaterialVarFlags.Debug) == 0) {
#pragma warning disable CS0168
				int x; // Debugging breakpoint.
#pragma warning restore CS0168
			}

			if ((GetMaterialVarFlags() & MaterialVarFlags.NoDraw) == 0) {
				ReadOnlySpan<char> name = Shader.GetName();
				materials.ShaderSystem.DrawElements(Shader, ShaderParams, in ShaderRenderState, vertexCompression, ChangeID ^ DebugVarsSignature);
			}
		}
		else {
			Warning("Material.DrawMesh: No bound shader\n");
		}
	}
	public IShader? GetShader() => Shader;
	public string? GetShaderName() => Shader?.GetName();
	public void SetShader(ReadOnlySpan<char> shaderName) {

	}

	public bool IsRealTimeVersion() {
		return true;
	}

	public VertexFormat GetVertexFormat() {
		Precache();
		return ShaderRenderState.VertexFormat;
	}

	// This is not well understood. It relates to multithreading, and that's all I know right now.
	public IMaterialInternal GetRealTimeVersion() {
		return this;
	}

	public bool InMaterialPage() => false;
	public IMaterial GetMaterialPage() => null;

	public float GetMappingWidth() {
		Precache();
		return MappingWidth;
	}

	public float GetMappingHeight() {
		Precache();
		return MappingHeight;
	}

	public void Refresh() {
		if (materials.ShaderDevice.IsUsingGraphics()) {
			Uncache();
			Precache();
		}
	}

	private void Uncache(bool preserveVars = false) {
		if (IsPrecached()) {
			CleanUpStateSnapshots();
			flags &= ~MaterialFlags.ValidRenderState;
			flags &= ~MaterialFlags.IsPrecached;
		}

		if (!preserveVars) {
			if (IsPrecachedVars()) {
				CleanUpShaderParams();
				Shader = null;

				CleanUpMaterialProxy();

				flags &= ~MaterialFlags.VarsIsPrecached;
			}
		}
	}

	private void CleanUpMaterialProxy() {

	}

	private void CleanUpShaderParams() {

	}

	private void CleanUpStateSnapshots() {

	}

	int EnumerationID;
	public int GetEnumerationID() => EnumerationID;
	public void SetEnumerationID(int id) => EnumerationID = id;
	public bool GetPropertyFlag(MaterialPropertyTypes types) => false; // todo

	int MinLightmapPageID;
	int MaxLightmapPageID;

	public int GetMinLightmapPageID() => MinLightmapPageID;
	public int GetMaxLightmapPageID() => MaxLightmapPageID;
	public void SetMinLightmapPageID(int value) => MinLightmapPageID = value;
	public void SetMaxLightmapPageID(int value) => MaxLightmapPageID = value;
	public bool GetNeedsWhiteLightmap() => (flags & MaterialFlags.NeedsWhiteLightmap) != 0;
	public void SetNeedsWhiteLightmap(bool value) {
		if (value)
			flags |= MaterialFlags.NeedsWhiteLightmap;
		else
			flags &= ~MaterialFlags.NeedsWhiteLightmap;
	}
}