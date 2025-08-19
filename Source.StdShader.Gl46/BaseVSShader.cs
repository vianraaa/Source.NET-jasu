using Source.Common.MaterialSystem;

namespace Source.StdShader.Gl46;

public abstract class BaseVSShader : BaseShader {
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
}
