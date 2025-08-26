using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;
using Source.Common.ShaderLib;

using static Source.StdShader.Gl46.UnlitGeneric;

namespace Source.StdShader.Gl46;

public abstract class BaseVSShader : BaseShader
{
	public static bool IsTextureSet(int index, Span<IMaterialVar> parms) {
		return index != -1 && parms[index].GetTextureValue() != null;
	}
	public static bool IsBoolSet(int index, Span<IMaterialVar> parms) {
		return index != -1 && parms[index].GetIntValue() != 0;
	}
	public static int GetIntParam(int index, Span<IMaterialVar> parms, int defaultValue = 0) {
		return index != -1 ? parms[index].GetIntValue() : defaultValue;
	}
	public static float GetFloatParam(int index, Span<IMaterialVar> parms, float defaultValue = 0) {
		return index != -1 ? parms[index].GetFloatValue() : defaultValue;
	}
	public static void InitFloatParam(int index, Span<IMaterialVar> parms, float value) {
		if (index != -1 && !parms[index].IsDefined())
			parms[index].SetFloatValue(value);
	}
	public static void InitIntParam(int index, Span<IMaterialVar> parms, int value) {
		if (index != -1 && !parms[index].IsDefined())
			parms[index].SetIntValue(value);
	}

	internal void InitParamsUnlitGeneric(int baseTextureVar, int detailScaleVar, int envmapOptionalVar,
		int envmapVar, int envmapTintVar, int envmapMaskScaleVar, int detailBlendMode) {
		IMaterialVar[] shaderParams = Params!;

		SetFlags2(shaderParams, MaterialVarFlags2.SupportsHardwareSkinning);

		if (envmapTintVar >= 0 && !shaderParams[envmapTintVar].IsDefined()) {
			shaderParams[envmapTintVar].SetVecValue(1.0f, 1.0f, 1.0f);
		}

		if (envmapMaskScaleVar >= 0 && !shaderParams[envmapMaskScaleVar].IsDefined()) {
			shaderParams[envmapMaskScaleVar].SetFloatValue(1.0f);
		}

		if (detailScaleVar >= 0 && !shaderParams[detailScaleVar].IsDefined()) {
			shaderParams[detailScaleVar].SetFloatValue(4.0f);
		}

		// No texture means no self-illum or env mask in base alpha
		if (baseTextureVar >= 0 && !shaderParams[baseTextureVar].IsDefined()) {
			ClearFlags(shaderParams, MaterialVarFlags.BaseAlphaEnvMapMask);
		}

		// If in decal mode, no debug override...
		if (IsFlagSet(shaderParams, MaterialVarFlags.Decal)) {
			SetFlags(shaderParams, MaterialVarFlags.NoDebugOverride);
		}

		// Get rid of the envmap if it's optional for this dx level.
		if (envmapOptionalVar >= 0 && shaderParams[envmapOptionalVar].IsDefined() && shaderParams[envmapOptionalVar].GetIntValue() != 0) {
			if (envmapVar >= 0) {
				shaderParams[envmapVar].SetUndefined();
			}
		}

		// If mat_specular 0, then get rid of envmap
		// TODO: what to do about the materialsystem_config_t type, which is what the "true" is right now as a placeholder
		if (envmapVar >= 0 && baseTextureVar >= 0 && true && shaderParams[envmapVar].IsDefined() && shaderParams[baseTextureVar].IsDefined()) {
			shaderParams[envmapVar].SetUndefined();
		}
	}

	VertexShaderHandle vsh;
	PixelShaderHandle psh;

	public void VertexShaderUnlitGenericPass(int baseTextureVar, int frameVar,
												  int baseTextureTransformVar,
												  int detailVar, int detailTransform,
												  bool bDetailTransformIsScale,
												  int envmapVar, int envMapFrameVar,
												  int envmapMaskVar, int envmapMaskFrameVar,
												  int envmapMaskScaleVar, int envmapTintVar,
												  int alphaTestReferenceVar,
												  int nDetailBlendModeVar,
												  int nOutlineVar,
												  int nOutlineColorVar,
												  int nOutlineStartVar,
												  int nOutlineEndVar,
												  int nSeparateDetailUVsVar) {
		IMaterialVar[] shaderParams = Params!;

		bool bBaseTexture = (baseTextureVar >= 0) && shaderParams[baseTextureVar].IsTexture();


		ShaderAPI!.BindVertexShader(in vsh);
		ShaderAPI!.BindPixelShader(in psh);

		if (ShaderAPI!.InFlashlightMode()) {
			Draw(false);
			return;
		}

		if (bBaseTexture) {
			BindTexture(in shaderParams[baseTextureVar].GPU, baseTextureVar, frameVar);
			//SetVertexShaderTextureTransform(baseTextureTransformVar);
		}

		Draw();
	}

	private void BindTexture(in MaterialVarGPU hardwareTarget, int textureVarIdx, int frameVarIdx) {
		IMaterialVar textureVar = Params![textureVarIdx];
		IMaterialVar? frameVar = frameVarIdx != -1 ? Params[frameVarIdx] : null;
		ShaderSystem.BindTexture(hardwareTarget, textureVar.GetTextureValue()!, frameVar?.GetIntValue() ?? 0);
	}

	private void Draw(bool makeActualDrawCall = true) {
		ShaderSystem.Draw();
	}

	public void InitUnlitGeneric(int baseTextureVar, int detailVar, int envmapVar, int envmapMaskVar) {
		IMaterialVar[] shaderParams = Params!;

		vsh = ShaderInit!.LoadVertexShader($"unlitgeneric_{ShaderAPI!.GetDriver().Extension(ShaderType.Vertex)}");
		psh = ShaderInit!.LoadPixelShader($"unlitgeneric_{ShaderAPI!.GetDriver().Extension(ShaderType.Pixel)}");

		if (!vsh.IsValid() || !psh.IsValid()) {
			Warning("Invalid shaders, skipping InitUnlitGeneric as it would be pointless to continue.\n");
			Warning($"   Vertex: {(vsh.IsValid() ? "valid" : "invalid")}, pixel: {(vsh.IsValid() ? "valid" : "invalid")}\n");
			return;
		}

		RecomputeShaderUniforms(in vsh, in psh);

		if (baseTextureVar >= 0 && shaderParams[baseTextureVar].IsDefined()) {
			LoadTexture(baseTextureVar);

			if (!shaderParams[baseTextureVar].GetTextureValue()!.IsTranslucent()) {
				if (IsFlagSet(shaderParams, MaterialVarFlags.BaseAlphaEnvMapMask))
					ClearFlags(shaderParams, MaterialVarFlags.BaseAlphaEnvMapMask);
			}
		}

		DevMsg("Managed to init an UnlitGeneric shader instance.\n");
	}
}
