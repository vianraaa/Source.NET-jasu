using Microsoft.Extensions.DependencyInjection;

using Source.Common;
using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.Common.ShaderLib;

using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace Source.MaterialSystem;

public class RenderPassList
{
	public const int MAX_RENDER_PASSES = 4;

	public int PassCount;
	public StateSnapshot_t[] Snapshot = new StateSnapshot_t[MAX_RENDER_PASSES];
	public BasePerMaterialContextData[] ContextData = new BasePerMaterialContextData[MAX_RENDER_PASSES];
}

public class ShaderRenderState
{
	public const int SHADER_OPACITY_ALPHATEST = 0x0010;
	public const int SHADER_OPACITY_OPAQUE = 0x0020;
	public const int SHADER_OPACITY_TRANSLUCENT = 0x0040;
	public const int SHADER_OPACITY_MASK = 0x0070;

	public int Flags;
	public VertexFormat VertexFormat;
	public VertexFormat VertexUsage;

	public List<RenderPassList> Snapshots = [];

	public bool IsTranslucent() => (Flags & SHADER_OPACITY_TRANSLUCENT) != 0;
	public bool IsAlphaTested() => (Flags & SHADER_OPACITY_ALPHATEST) != 0;
}

public interface IShaderSystemInternal : IShaderInit, IShaderSystem
{
	void LoadAllShaderDLLs();
	bool LoadShaderDLL<T>(T instance) where T : IShaderDLL;
	IShader? FindShader(ReadOnlySpan<char> shaderName);

	void InitShaderParameters(IShader shader, IMaterialVar[] vars, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName);
	bool InitRenderState(IShader shader, IMaterialVar[] shaderParams, ref ShaderRenderState shaderRenderState, ReadOnlySpan<char> materialName);
	// void CleanupRenderState(ref ShaderRenderState renderState);
	void DrawElements(IShader shader, IMaterialVar[] parms, in ShaderRenderState renderState, VertexCompressionType vertexCompression, uint materialVarTimeStamp);
	IEnumerable<IShader> GetShaders();
}

public class ShaderSystem : IShaderSystemInternal
{
	List<IShaderDLL> ShaderDLLs = [];
	ShaderRenderState? RenderState;
	byte Modulation;
	byte RenderPass;
	internal MaterialSystem MaterialSystem;
	internal ShaderAPIGl46 ShaderAPI;
	internal MaterialSystem_Config Config;

	public void BindTexture(Sampler sampler, ITexture texture) {
		throw new NotImplementedException();
	}

	public void DrawElements(IShader shader, IMaterialVar[] parms, in ShaderRenderState renderState, VertexCompressionType vertexCompression, uint materialVarTimeStamp) {
		ShaderAPI.InvalidateDelayedShaderConstraints();
		int mod = shader.ComputeModulationFlags(parms, ShaderAPI);
		if (renderState.Snapshots[mod].PassCount == 0)
			return;

		int materialVarFlags = parms[(int)ShaderMaterialVars.Flags].GetIntValue();
		if (((materialVarFlags & (int)MaterialVarFlags.Model) != 0) || (IsFlag2Set(parms, MaterialVarFlags2.SupportsHardwareSkinning) && (ShaderAPI.GetCurrentNumBones() > 0))) {
			ShaderAPI.SetSkinningMatrices();
		}

		if ((Config.ShowNormalMap || Config.ShowMipLevels == 2) && (IsFlag2Set(parms, MaterialVarFlags2.LightingBumpedLightmap) || IsFlag2Set(parms, MaterialVarFlags2.DiffuseBumpmappedModel))) {
			DrawNormalMap(shader, parms, vertexCompression);
		}
		else {
			ShaderAPI.SetDefaultState();

			if ((materialVarFlags & (uint)MaterialVarFlags.Flat) > 0)
				ShaderAPI.ShadeMode(ShadeMode.Flat);

			PrepForShaderDraw(shader, parms, renderState, mod);
			ShaderAPI.BeginPass(CurrentStateSnapshot());

			ref BasePerMaterialContextData contextDataPtr = ref renderState.Snapshots[Modulation].ContextData[RenderPass];
			if(contextDataPtr != null && (contextDataPtr.VarChangeID != materialVarTimeStamp)) {
				contextDataPtr.MaterialVarsChanged = true;
				contextDataPtr.VarChangeID = materialVarTimeStamp;
			}

			shader.DrawElements(parms, mod, null, ShaderAPI, vertexCompression, ref renderState.Snapshots[Modulation].ContextData[RenderPass]);
			DoneWithShaderDraw();
		}
	}

	private void DrawNormalMap(IShader shader, Span<IMaterialVar> parms, VertexCompressionType vertexCompression) {
		throw new NotImplementedException();
	}

	public IShader? FindShader(ReadOnlySpan<char> shaderName) {
		foreach (var shaderDLL in ShaderDLLs) {
			foreach (var shader in shaderDLL.GetShaders()) {
				if (shaderName.Equals(shader.GetName(), StringComparison.OrdinalIgnoreCase))
					return shader;
			}
		}
		return null;
	}

	public IEnumerable<IShader> GetShaders() {
		foreach (var shaderDLL in ShaderDLLs) {
			foreach (var shader in shaderDLL.GetShaders())
				yield return shader;
		}
	}

	public bool LoadShaderDLL<T>(T shaderAPI) where T : IShaderDLL {
		ShaderDLLs.Add(shaderAPI);
		return true;
	}

	public ShaderSystem() {

	}
	public IServiceProvider Services;
	public void LoadAllShaderDLLs() {
		foreach (var dll in Services.GetServices<IShaderDLL>()) {
			LoadShaderDLL(dll);
		}
	}

	static string[] shaderStateStrings = [
		"$debug",
		"$no_fullbright",
		"$no_draw",
		"$use_in_fillrate_mode",

		"$vertexcolor",
		"$vertexalpha",
		"$selfillum",
		"$additive",
		"$alphatest",
		"$multipass",
		"$znearer",
		"$model",
		"$flat",
		"$nocull",
		"$nofog",
		"$ignorez",
		"$decal",
		"$envmapsphere",
		"$noalphamod",
		"$envmapcameraspace",
		"$basealphaenvmapmask",
		"$translucent",
		"$normalmapalphaenvmapmask",
		"$softwareskin",
		"$opaquetexture",
		"$envmapmode",
		"$nodecal",
		"$halflambert",
		"$wireframe",
		"$allowalphatocoverage",
		null
	];

	internal string ShaderStateString(int i) {
		return shaderStateStrings[i];
	}

	public void InitShaderParameters(IShader shader, IMaterialVar[] vars, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName) {
		PrepForShaderDraw(shader, vars, null, 0);
		shader.InitShaderParams(vars, materialName);
		DoneWithShaderDraw();

		if (!vars[(int)ShaderMaterialVars.Color].IsDefined())
			vars[(int)ShaderMaterialVars.Color].SetVecValue(1, 1, 1);

		if (!vars[(int)ShaderMaterialVars.Alpha].IsDefined())
			vars[(int)ShaderMaterialVars.Alpha].SetFloatValue(1);

		int i;
		for (i = shader.GetNumParams(); --i >= 0;) {
			if (vars[i].IsDefined())
				continue;
			ShaderParamType type = shader.GetParamType(i);
			switch (type) {
				case ShaderParamType.Texture:
					// Do nothing; we'll be loading in a string later
					break;
				case ShaderParamType.String:
					// Do nothing; we'll be loading in a string later
					break;
				case ShaderParamType.Material:
					vars[i].SetMaterialValue(null);
					break;
				case ShaderParamType.Bool:
				case ShaderParamType.Integer:
					vars[i].SetIntValue(0);
					break;
				case ShaderParamType.Color:
					vars[i].SetVecValue(1.0f, 1.0f, 1.0f);
					break;
				case ShaderParamType.Vec2:
					vars[i].SetVecValue(0.0f, 0.0f);
					break;
				case ShaderParamType.Vec3:
					vars[i].SetVecValue(0.0f, 0.0f, 0.0f);
					break;
				case ShaderParamType.Vec4:
					vars[i].SetVecValue(0.0f, 0.0f, 0.0f, 0.0f);
					break;
				case ShaderParamType.Float:
					vars[i].SetFloatValue(0);
					break;
				case ShaderParamType.FourCC:
					vars[i].SetFourCCValue(0, 0);
					break;
				case ShaderParamType.Matrix: {
						Matrix4x4 identity = Matrix4x4.Identity;
						vars[i].SetMatrixValue(identity);
					}
					break;
				case ShaderParamType.Matrix4x2: {
						Matrix4x4 identity = Matrix4x4.Identity;
						vars[i].SetMatrixValue(identity);
					}
					break;
				default:
					Dbg.Assert(false);
					break;
			}
		}
	}

	private void DoneWithShaderDraw() {
		RenderState = null;
	}

	private void PrepForShaderDraw(IShader shader, Span<IMaterialVar> vars, ShaderRenderState? renderState, int modulation) {
		Assert(RenderState == null);
		// LATER; plug into spew?
		RenderState = renderState;
		Modulation = (byte)modulation;
		RenderPass = 0;
	}

	public void InitShaderInstance(IShader shader, IMaterialVar[] shaderParams, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName) {
		PrepForShaderDraw(shader, shaderParams, null, 0);
		shader.InitShaderInstance(shaderParams, this, materialName, textureGroupName);
		DoneWithShaderDraw();
	}

	public void LoadTexture(IMaterialVar textureVar, ReadOnlySpan<char> textureGroupName, int additionalCreationFlags = 0) {
		throw new NotImplementedException();
	}

	public bool InitRenderState(IShader shader, IMaterialVar[] shaderParams, ref ShaderRenderState renderState, ReadOnlySpan<char> materialName) {
		Assert(RenderState == null);
		InitRenderStateFlags(ref renderState, shaderParams);
		InitStateSnapshots(shader, shaderParams, ref renderState);

		// todo

		ComputeRenderStateFlagsFromSnapshot(ref renderState);

		if (!ComputeVertexFormatFromSnapshot(shaderParams, ref renderState)) {
			Warning("Material \"%s\":\n   Shader \"%s\" can't be used with models!\n", new string(materialName), shader.GetName());
			CleanupRenderState(ref renderState);
			return false;
		}

		return true;
	}

	private void ComputeRenderStateFlagsFromSnapshot(ref ShaderRenderState renderState) {
		StateSnapshot_t snapshot = renderState.Snapshots[0].Snapshot[0];
		if (ShaderAPI.IsTranslucent(snapshot)) {

		}
		else {
			if (ShaderAPI.IsAlphaTested(snapshot)) {

			}
			else {

			}
		}
	}

	private void InitStateSnapshots(IShader shader, IMaterialVar[] shaderParams, ref ShaderRenderState renderState) {
		renderState ??= new();
		if (IsFlagSet(shaderParams, MaterialVarFlags.Debug)) {
#pragma warning disable CS0168
			int x; // Debugging breakpoint.
#pragma warning restore CS0168
		}

		float alpha;
		Span<float> color = stackalloc float[3];
		shaderParams[(int)ShaderMaterialVars.Color].GetVecValue(color);
		alpha = shaderParams[(int)ShaderMaterialVars.Alpha].GetFloatValue();
		bool bakedLighting = IsFlag2Set(shaderParams, MaterialVarFlags2.UseFixedFunctionBakedLighting);
		bool flashlight = IsFlag2Set(shaderParams, MaterialVarFlags2.UseFlashlight);
		bool editor = IsFlag2Set(shaderParams, MaterialVarFlags2.UseEditor);

		Span<float> white = [1, 1, 1];
		Span<float> grey = [.5f, .5f, .5f];

		int snapshotCount = GetModulationSnapshotCount(shaderParams);
		bool modUsesFlashlight = mat_supportflashlight.GetInt() != 0;

		for (int i = 0; i < snapshotCount; i++) {
			if ((i & (int)ShaderUsing.Flashlight) != 0 && !modUsesFlashlight) {
				renderState.Snapshots[i].PassCount = 0;
				continue;
			}

			if ((i & (int)ShaderUsing.ColorModulation) != 0)
				shaderParams[(int)ShaderMaterialVars.Color].SetVecValue(grey);
			else
				shaderParams[(int)ShaderMaterialVars.Color].SetVecValue(white);

			if ((i & (int)ShaderUsing.AlphaModulation) != 0)
				shaderParams[(int)ShaderMaterialVars.Alpha].SetFloatValue(grey[0]);
			else
				shaderParams[(int)ShaderMaterialVars.Alpha].SetFloatValue(white[0]);

			if ((i & (int)ShaderUsing.Flashlight) != 0)
				SetFlags2(shaderParams, MaterialVarFlags2.UseFlashlight);
			else
				ClearFlags2(shaderParams, MaterialVarFlags2.UseFlashlight);

			if ((i & (int)ShaderUsing.Editor) != 0)
				SetFlags2(shaderParams, MaterialVarFlags2.UseEditor);
			else
				ClearFlags2(shaderParams, MaterialVarFlags2.UseEditor);

			if ((i & (int)ShaderUsing.FixedFunctionBakedLighting) != 0)
				SetFlags2(shaderParams, MaterialVarFlags2.UseFixedFunctionBakedLighting);
			else
				ClearFlags2(shaderParams, MaterialVarFlags2.UseFixedFunctionBakedLighting);

			PrepForShaderDraw(shader, shaderParams, renderState, i);
			renderState.Snapshots[i].PassCount = 0;
			shader.DrawElements(shaderParams, i, MaterialSystem.ShaderShadow, null, VertexCompressionType.None, ref renderState.Snapshots[i].ContextData[0]);
			DoneWithShaderDraw();
		}

		shaderParams[(int)ShaderMaterialVars.Color].SetVecValue(color);
		shaderParams[(int)ShaderMaterialVars.Alpha].SetFloatValue(alpha);

		if (bakedLighting) SetFlags2(shaderParams, MaterialVarFlags2.UseFixedFunctionBakedLighting);
		else ClearFlags2(shaderParams, MaterialVarFlags2.UseFixedFunctionBakedLighting);

		if (editor) SetFlags2(shaderParams, MaterialVarFlags2.UseEditor);
		else ClearFlags2(shaderParams, MaterialVarFlags2.UseEditor);

		if (flashlight) SetFlags2(shaderParams, MaterialVarFlags2.UseFlashlight);
		else ClearFlags2(shaderParams, MaterialVarFlags2.UseFlashlight);
	}

	public const int SNAPSHOT_COUNT_NORMAL = 16;
	public const int SNAPSHOT_COUNT_EDITOR = 32;
	public int SnapshotTypeCount() => MaterialSystem.CanUseEditorMaterials() ? SNAPSHOT_COUNT_EDITOR : SNAPSHOT_COUNT_NORMAL;

	static void AddSnapshotsToList(RenderPassList passList, ref int snapshotID, Span<StateSnapshot_t> snapshots) {
		int numPassSnapshots = passList.PassCount;
		for (int i = 0; i < numPassSnapshots; i++) {
			snapshots[snapshotID] = passList.Snapshot[i];
			snapshotID++;
		}
	}

	public bool ComputeVertexFormatFromSnapshot(IMaterialVar[] shaderParams, ref ShaderRenderState renderState) {
		int modulationSnapshotCount = GetModulationSnapshotCount(shaderParams);
		int numSnapshots = renderState.Snapshots[0].PassCount;
		if (modulationSnapshotCount >= (int)ShaderUsing.Flashlight)
			numSnapshots += renderState.Snapshots[(int)ShaderUsing.Flashlight].PassCount;
		if (MaterialSystem.CanUseEditorMaterials())
			numSnapshots += renderState.Snapshots[(int)ShaderUsing.Editor].PassCount;

		Span<StateSnapshot_t> snapshots = stackalloc StateSnapshot_t[numSnapshots];
		int snapshotID = 0;
		AddSnapshotsToList(renderState.Snapshots[0], ref snapshotID, snapshots);
		if (modulationSnapshotCount >= (int)ShaderUsing.Flashlight)
			AddSnapshotsToList(renderState.Snapshots[(int)ShaderUsing.Flashlight], ref snapshotID, snapshots);
		if (MaterialSystem.CanUseEditorMaterials())
			AddSnapshotsToList(renderState.Snapshots[(int)ShaderUsing.Editor], ref snapshotID, snapshots);

		Assert(snapshotID == numSnapshots);

		for (int mod = 0; mod < modulationSnapshotCount; mod++) {
			int numSnapshotsTest = renderState.Snapshots[mod].PassCount;
			Span<StateSnapshot_t> snapshotsTest = stackalloc StateSnapshot_t[numSnapshotsTest];
			for (int i = 0; i < numSnapshotsTest; i++) {
				snapshotsTest[i] = renderState.Snapshots[mod].Snapshot[i];
			}
			VertexFormat usageTest = ShaderAPI.ComputeVertexUsage(snapshotsTest);
		}

		if (IsPC()) {
			renderState.VertexUsage = ShaderAPI.ComputeVertexUsage(snapshots);
		}
		else {
			renderState.VertexFormat = renderState.VertexUsage;
		}

		return true;
	}

	private int GetModulationSnapshotCount(IMaterialVar[] shaderParams) {
		int snapshotCount = SnapshotTypeCount();
		if (!MaterialSystem.CanUseEditorMaterials()) {
			if (!IsFlag2Set(shaderParams, MaterialVarFlags2.NeedsBakedLightingSnapshots))
				snapshotCount /= 2;
		}
		return snapshotCount;
	}

	private void InitRenderStateFlags(ref ShaderRenderState renderState, IMaterialVar[] shaderParams) {
		renderState.Flags = 0;
		renderState.Flags &= ~ShaderRenderState.SHADER_OPACITY_MASK;
	}

	internal void CleanupRenderState(ref ShaderRenderState renderState) {
		if (renderState != null) {
			int snapshotCount = SnapshotTypeCount();
			for (int i = 0; i < snapshotCount; i++) {
				for (int j = 0; j < renderState.Snapshots[i].PassCount; j++)
					renderState.Snapshots[i].ContextData[j] = null;
				renderState.Snapshots[i].PassCount = 0;
			}
		}
	}


	public void TakeSnapshot() {
		Assert(RenderState);
		Assert(Modulation < SnapshotTypeCount());
		if (MaterialSystem.HardwareConfig.SupportsPixelShaders_2_b()) {
			MaterialSystem.ShaderShadow.EnableTexture(Sampler.Sampler15, true);
			MaterialSystem.ShaderShadow.EnableSRGBRead(Sampler.Sampler15, true);
		}

		RenderPassList snapshotList = RenderState!.Snapshots[Modulation];
		snapshotList.Snapshot[snapshotList.PassCount] = ShaderAPI.TakeSnapshot();
		++snapshotList.PassCount;
	}

	public StateSnapshot_t CurrentStateSnapshot() {
		Assert(RenderState);
		Assert(RenderPass < RenderPassList.MAX_RENDER_PASSES);
		Assert(RenderPass < RenderState!.Snapshots[Modulation].PassCount);
		return RenderState.Snapshots[Modulation].Snapshot[RenderPass];
	}

	public void DrawSnapshot(bool makeActualDrawCall = true) {
		Assert(RenderState);
		RenderPassList snapshotList = RenderState!.Snapshots[Modulation];

		int passCount = snapshotList.PassCount;
		Assert(RenderPass < passCount);

		if (makeActualDrawCall)
			ShaderAPI.RenderPass(RenderPass, passCount);

		ShaderAPI.InvalidateDelayedShaderConstraints();
		if (++RenderPass < passCount)
			ShaderAPI.BeginPass(CurrentStateSnapshot());
	}
}