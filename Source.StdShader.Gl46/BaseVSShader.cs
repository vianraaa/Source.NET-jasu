using Source.Common.MaterialSystem;

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

	public void InitUnlitGeneric_DX8(int baseTextureVar, int detailVar, int envmapVar, int envmapMaskVar) {
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
