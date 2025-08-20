using Source.Common.MaterialSystem;
using Source.Common.ShaderLib;

namespace Source;

public static class ShaderDefines
{
	public const int INVALID_SHADERAPI_TEXTURE_HANDLE = 0;


	public static bool IsFlagSet(IMaterialVar[] shaderParams, int flag) => (shaderParams[(int)ShaderMaterialVars.Flags].GetIntValue() & flag) != 0;
	public static bool IsFlagSet(IMaterialVar[] shaderParams, MaterialVarFlags flag) => IsFlagSet(shaderParams, (int)flag);
	public static bool IsFlag2Set(IMaterialVar[] shaderParams, int flag) => (shaderParams[(int)ShaderMaterialVars.Flags2].GetIntValue() & flag) != 0;
	public static bool IsFlag2Set(IMaterialVar[] shaderParams, MaterialVarFlags2 flag) => IsFlag2Set(shaderParams, (int)flag);

	public static void SetFlags(IMaterialVar[] shaderParams, int flag)
		=> shaderParams[(int)ShaderMaterialVars.Flags].SetIntValue(shaderParams[(int)ShaderMaterialVars.Flags].GetIntValue() | flag);
	public static void SetFlags(IMaterialVar[] shaderParams, MaterialVarFlags flag) => SetFlags(shaderParams, (int)flag);
	public static void SetFlags2(IMaterialVar[] shaderParams, int flag)
		=> shaderParams[(int)ShaderMaterialVars.Flags2].SetIntValue(shaderParams[(int)ShaderMaterialVars.Flags2].GetIntValue() | flag);
	public static void SetFlags2(IMaterialVar[] shaderParams, MaterialVarFlags2 flag) => SetFlags2(shaderParams, (int)flag);

	public static void ClearFlags(IMaterialVar[] shaderParams, int flag)
		=> shaderParams[(int)ShaderMaterialVars.Flags].SetIntValue(shaderParams[(int)ShaderMaterialVars.Flags].GetIntValue() & ~flag);
	public static void ClearFlags(IMaterialVar[] shaderParams, MaterialVarFlags flag) => SetFlags(shaderParams, (int)flag);
	public static void ClearFlags2(IMaterialVar[] shaderParams, int flag)
		=> shaderParams[(int)ShaderMaterialVars.Flags2].SetIntValue(shaderParams[(int)ShaderMaterialVars.Flags2].GetIntValue() & ~flag);
	public static void ClearFlags2(IMaterialVar[] shaderParams, MaterialVarFlags2 flag) => SetFlags2(shaderParams, (int)flag);
}
