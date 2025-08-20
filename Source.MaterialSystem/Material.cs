using Raylib_cs;

using Source.Common.Formats.Keyvalues;
using Source.Common.MaterialSystem;
using Source.Common.ShaderLib;

using Steamworks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

public class EditorRenderStateList : List<RenderPassList>
{
	public EditorRenderStateList() : base() {
		for (int i = 0; i < ShaderManager.SNAPSHOT_COUNT_EDITOR; i++) {
			Add(new() {

			});
		}
	}
}

public class StandardRenderStateList : List<RenderPassList>
{
	public StandardRenderStateList() : base() {
		for (int i = 0; i < ShaderManager.SNAPSHOT_COUNT_NORMAL; i++) {
			Add(new() {

			});
		}
	}
}

public class Material : IMaterialInternal
{
	public readonly MaterialSystem materials;
	public Material(MaterialSystem materials, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName, KeyValues? keyValues) {
		this.materials = materials;
		name = new(materialName);
		texGroupName = new(textureGroupName);
		this.keyValues = keyValues;
		if (keyValues != null) {
			flags |= MaterialFlags.IsManuallyCreated;
		}
		ShaderParams = null;
		MappingWidth = MappingHeight = 0;
		if(keyValues != null) {
			flags |= MaterialFlags.IsManuallyCreated;
		}

		ShaderRenderState.Flags = 0;
		ShaderRenderState.VertexFormat = ShaderRenderState.VertexUsage = 0;
		ShaderRenderState.Snapshots = CreateRenderPassList();
	}

	private List<RenderPassList> CreateRenderPassList() {
		List<RenderPassList> renderPassList;
		if (!materials.CanUseEditorMaterials()) 
			renderPassList = new StandardRenderStateList();
		else 
			renderPassList = new EditorRenderStateList();
		
		return renderPassList;
	}

	public int MappingWidth;
	public int MappingHeight;

	public string GetName() {
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
		if(Shader != null) 
			materials.ShaderSystem.InitShaderInstance(Shader, ShaderParams, GetName(), GetTextureGroupName());

		RecomputeStateSnapshots();
		FindRepresentativeTexture();
		PrecacheMappingDimensions();
		Assert(IsValidRenderState());
	}

	private void RecomputeStateSnapshots() {
		bool ok = InitializeStateSnapshots();
		if (!ok)
			SetupErrorShader();
	}

	private bool InitializeStateSnapshots() {
		if (IsPrecached()) {
			if(materials.GetCurrentMaterial() == this) {
				Rlgl.DrawRenderBatchActive();
			}

			CleanUpStateSnapshots();

			if (Shader != null && !materials.ShaderSystem.InitRenderState(Shader, ShaderParams, ref ShaderRenderState, GetName())) {
				flags &= ~MaterialFlags.ValidRenderState;
				return false;
			}

			flags |= MaterialFlags.ValidRenderState;
		}

		return true;
	}

	private void CleanUpStateSnapshots() {
		if (IsValidRenderState()) {
			materials.ShaderSystem.CleanupRenderState(ref ShaderRenderState);
		}
	}

	private void SetupErrorShader() {
		throw new NotImplementedException();
	}

	private void FindRepresentativeTexture() {

	}

	private void PrecacheMappingDimensions() {

	}

	private bool IsValidRenderState() {
		return (flags & MaterialFlags.ValidRenderState) != 0;
	}

	private string GetTextureGroupName() {
		return "";
	}

	private bool PrecacheVars(KeyValues? inVmtKeyValues = null, KeyValues? inPatchKeyValues = null, int findContext = 0) {
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
			// includes
			error = true;
		}

		if (!error) {
			flags |= MaterialFlags.IsPrecached;
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

	private KeyValues? InitializeShader(KeyValues keyValues, KeyValues? patchKeyValues, int findContext) {
		KeyValues currentFallback = keyValues;
		KeyValues? fallbackSection = null;

		string? shaderName = currentFallback.Name;
		if (shaderName == null) {
			Dbg.Warning($"Shader not specified in material {GetName()}\nUsing wireframe instead...\n");
			Dbg.Assert(false);
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

		return currentFallback;
	}

	private int ParseMaterialVars(IShader shader, KeyValues keyValues, KeyValues? fallbackSection, KeyValues? overrideKeyValues, bool modelDefault, IMaterialVar[] vars, int findContext) {
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

		ReadOnlySpan<char> matName = var != null ? var.Value.GetString() : "Unknown";
		while (var != null) {
			bool processThisOne = true;
			bool isConditionalVar;
			ReadOnlySpan<char> varName = GetVarName(var.Value);

			if (findContext == (int)MaterialFindContext.IsOnAModel && varName != null && varName.Length > 0) {
				if (varName.Contains("$ignorez", StringComparison.OrdinalIgnoreCase)) {
					Dbg.Warning($"Ignoring material flag '{varName}' on material '{matName}'.\n");
					goto nextVar;
				}
			}

			if (
				ShouldSkipVar(var, out isConditionalVar) ||
				(var.Value.Name[0] == '%') ||
				ParseMaterialFlag(var.Value, vars[(int)ShaderMaterialVars.Flags], vars[(int)ShaderMaterialVars.FlagsDefined], parsingOverrides, ref flagMask, ref overrideMask) ||
				ParseMaterialFlag(var.Value, vars[(int)ShaderMaterialVars.Flags2], vars[(int)ShaderMaterialVars.FlagsDefined2], parsingOverrides, ref flagMask, ref overrideMask)
				)
				processThisOne = false;

			if (processThisOne) {
				int varIdx = FindMaterialVar(shader, varName);
				if (varIdx >= 0) {
					if (vars[varIdx] != null && (!isConditionalVar)) {
						if (!overrides[varIdx] || parsingOverrides) {
							Dbg.Warning($"Error! Variable \"{var.Value.Name}\" is multiply defined in material \"{GetName()}\"!\n");
						}
						goto nextVar;
					}
				}
				else {
					int i;
					for (i = numParams; i < varCount; ++i) {
						Dbg.Assert(vars[i] != null);
						if (vars[i].GetName().Equals(var.Value.Name, StringComparison.OrdinalIgnoreCase))
							break;
					}

					if (i != varCount) {
						if (!overrides[varIdx] || parsingOverrides) {
							Dbg.Warning($"Error! Variable \"{var.Value.Name}\" is multiply defined in material \"{GetName()}\"!\n");
						}
						goto nextVar;
					}
				}

				newVar = CreateMaterialVarFromKeyValue(this, var.Value);
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
			var = var.Next;
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
		int flagbit = FindMaterialVarFlag(GetVarName(parseValue));
		if (flagbit == 0)
			return false;

		int testMask = parsingOverrides ? overrideMask : flagMask;
		if ((testMask & flagbit) != 0) {
			Dbg.Warning($"Error! Flag \"{parseValue.Name}\" is multiply defined in material \"{GetName()}\"!\n");
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

	private int FindMaterialVarFlag(ReadOnlySpan<char> flagName) {
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
					return (1 << i);

				if (!char.IsWhiteSpace(flagName[pFound]))
					break;

				pFound++;
			}
		}

		return 0;
	}

	private bool ShouldSkipVar(LinkedListNode<KeyValues> var, out bool isConditionalVar) {
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
	string name;
	string texGroupName;
	IShader? Shader;
	KeyValues? keyValues;
	IMaterialVar[]? ShaderParams;
	// IMaterialProxy
	ShaderRenderState ShaderRenderState = new();

	public void DrawMesh(VertexCompressionType vertexCompression) { }
	public IShader? GetShader() => Shader;
	public string? GetShaderName() => Shader?.GetName();
	public void SetShader(ReadOnlySpan<char> shaderName) {

	}
}