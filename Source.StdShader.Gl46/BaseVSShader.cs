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

		// Load unlit generic shader
		vsh = ShaderSystem.GetOrCreateVertexShader($"unlitgeneric_{ShaderAPI!.GetDriver().Extension(ShaderType.Vertex)}");
		psh = ShaderSystem.GetOrCreatePixelShader($"unlitgeneric_{ShaderAPI!.GetDriver().Extension(ShaderType.Pixel)}");
		RecomputeShaderUniforms();

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

		bool bBaseAlphaEnvmapMask = IsFlagSet(shaderParams, MaterialVarFlags.BaseAlphaEnvMapMask);
		bool bEnvmap = (envmapVar >= 0) && shaderParams[envmapVar].IsTexture();
		bool bMask = false;
		if (bEnvmap && (envmapMaskVar >= 0)) {
			bMask = shaderParams[envmapMaskVar].IsTexture();
		}
		bool bDetail = (detailVar >= 0) && shaderParams[detailVar].IsTexture();
		bool bBaseTexture = (baseTextureVar >= 0) && shaderParams[baseTextureVar].IsTexture();
		bool bVertexColor = IsFlagSet(shaderParams, MaterialVarFlags.VertexColor);
		bool bEnvmapCameraSpace = IsFlagSet(shaderParams, MaterialVarFlags.EnvMapCameraSpace);
		bool bEnvmapSphere = IsFlagSet(shaderParams, MaterialVarFlags.EnvMapSphere);

		bool bDetailMultiply = (nDetailBlendModeVar >= 0) && (shaderParams[nDetailBlendModeVar].GetIntValue() == 8);
		bool bMaskBaseByDetailAlpha = (nDetailBlendModeVar >= 0) && (shaderParams[nDetailBlendModeVar].GetIntValue() == 9);
		bool bSeparateDetailUVs = (nSeparateDetailUVsVar >= 0) && (shaderParams[nSeparateDetailUVsVar].GetIntValue() != 0);

		ShaderAPI!.BindVertexShader(in vsh);
		ShaderAPI!.BindPixelShader(in psh);

		if (ShaderAPI!.InFlashlightMode()) {
			Draw(false);
			return;
		}

		if (bBaseTexture) {
			BindTexture(Sampler.Sampler0, baseTextureVar, frameVar);
			//SetVertexShaderTextureTransform(baseTextureTransformVar);
		}

		if (bDetail) {
			BindTexture(Sampler.Sampler3, detailVar, frameVar);

			if (bDetailTransformIsScale) {
				//SetVertexShaderTextureScaledTransform(VERTEX_SHADER_SHADER_SPECIFIC_CONST_4, baseTextureTransformVar, detailTransform);
			}
			else {
				//SetVertexShaderTextureTransform(VERTEX_SHADER_SHADER_SPECIFIC_CONST_4, detailTransform);
			}
		}

		if (bEnvmap) {
			BindTexture(Sampler.Sampler1, envmapVar, envMapFrameVar);

			if (bMask || bBaseAlphaEnvmapMask) {
				if (bMask)
					BindTexture(Sampler.Sampler2, envmapMaskVar, envmapMaskFrameVar);
				else
					BindTexture(Sampler.Sampler2, baseTextureVar, frameVar);

				//SetVertexShaderTextureScaledTransform(VERTEX_SHADER_SHADER_SPECIFIC_CONST_2, baseTextureTransformVar, envmapMaskScaleVar);
			}

			//SetEnvMapTintPixelShaderDynamicState(2, envmapTintVar, -1);

			if (bEnvmapSphere || IsFlagSet(shaderParams, MaterialVarFlags.EnvMapCameraSpace)) {
				//LoadViewMatrixIntoVertexShaderConstant(VERTEX_SHADER_VIEWMODEL);
			}
		}

		//SetModulationVertexShaderDynamicState();

		Span<float> flConsts = [ 0, 0, 0, 1, 				// color
			0, 0, 0, 0,					// max
			0, 0, 0, .5f,				// min
		];

		// set up outline pixel shader constants
		if (bDetailMultiply && (nOutlineVar != -1) && (shaderParams[nOutlineVar].GetIntValue() != 0)) {
			if (nOutlineColorVar != -1)
				shaderParams[nOutlineColorVar].GetVecValue(flConsts[..3]);
			if (nOutlineEndVar != -1)
				flConsts[7] = shaderParams[nOutlineEndVar].GetFloatValue();
			if (nOutlineStartVar != -1)
				flConsts[11] = shaderParams[nOutlineStartVar].GetFloatValue();
		}

		// ShaderAPI.SetPixelShaderConstant("", flConsts[..3]);

		Draw();
	}

	private void BindTexture(Sampler sampler, int baseTextureVar, int frameVar) {
	}

	private void Draw(bool makeActualDrawCall = true) {
		ShaderSystem.Draw();
	}

	public void InitUnlitGeneric(int baseTextureVar, int detailVar, int envmapVar, int envmapMaskVar) {
		IMaterialVar[] shaderParams = Params!;
		if (baseTextureVar >= 0 && shaderParams[baseTextureVar].IsDefined()) {
			LoadTexture(baseTextureVar);

			if (!shaderParams[baseTextureVar].GetTextureValue()!.IsTranslucent()) {
				if (IsFlagSet(shaderParams, MaterialVarFlags.BaseAlphaEnvMapMask))
					ClearFlags(shaderParams, MaterialVarFlags.BaseAlphaEnvMapMask);
			}
		}

		// Don't alpha test if the alpha channel is used for other purposes
		if (IsFlagSet(shaderParams, MaterialVarFlags.BaseAlphaEnvMapMask))
			ClearFlags(shaderParams, MaterialVarFlags.AlphaTest);

		// the second texture (if there is one)
		if (detailVar >= 0 && shaderParams[detailVar].IsDefined()) {
			LoadTexture(detailVar);
		}

		if (envmapVar >= 0 && shaderParams[envmapVar].IsDefined()) {
			if (!IsFlagSet(shaderParams, MaterialVarFlags.EnvMapSphere))
				LoadCubeMap(envmapVar);
			else
				LoadTexture(envmapVar);

			if (!HardwareConfig.SupportsCubeMaps())
				SetFlags(shaderParams, MaterialVarFlags.EnvMapSphere);

			if (envmapMaskVar >= 0 && shaderParams[envmapMaskVar].IsDefined())
				LoadTexture(envmapMaskVar);
		}
		if (baseTextureVar >= 0 && shaderParams[baseTextureVar].IsDefined()) {
			LoadTexture(baseTextureVar);

			if (!shaderParams[baseTextureVar].GetTextureValue()!.IsTranslucent()) {
				if (IsFlagSet(shaderParams, MaterialVarFlags.BaseAlphaEnvMapMask))
					ClearFlags(shaderParams, MaterialVarFlags.BaseAlphaEnvMapMask);
			}
		}

		// Don't alpha test if the alpha channel is used for other purposes
		if (IsFlagSet(shaderParams, MaterialVarFlags.BaseAlphaEnvMapMask))
			ClearFlags(shaderParams, MaterialVarFlags.AlphaTest);

		// the second texture (if there is one)
		if (detailVar >= 0 && shaderParams[detailVar].IsDefined()) {
			LoadTexture(detailVar);
		}

		if (envmapVar >= 0 && shaderParams[envmapVar].IsDefined()) {
			if (!IsFlagSet(shaderParams, MaterialVarFlags.EnvMapSphere))
				LoadCubeMap(envmapVar);
			else
				LoadTexture(envmapVar);

			if (!HardwareConfig.SupportsCubeMaps())
				SetFlags(shaderParams, MaterialVarFlags.EnvMapSphere);

			if (envmapMaskVar >= 0 && shaderParams[envmapMaskVar].IsDefined())
				LoadTexture(envmapMaskVar);
		}
	}
}
