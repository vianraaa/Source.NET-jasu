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

	// Standard vertex shader constants
	public const int VERTEX_SHADER_MATH_CONSTANTS0 = 0;
	public const int VERTEX_SHADER_MATH_CONSTANTS1 = 1;
	public const int VERTEX_SHADER_CAMERA_POS = 2;
	public const int VERTEX_SHADER_FLEXSCALE = 3;   // DX9 only
	public const int VERTEX_SHADER_LIGHT_INDEX = 3; // DX8 only
	public const int VERTEX_SHADER_MODELVIEWPROJ = 4;
	public const int VERTEX_SHADER_VIEWPROJ = 8;
	public const int VERTEX_SHADER_MODELVIEWPROJ_THIRD_ROW = 12;
	public const int VERTEX_SHADER_VIEWPROJ_THIRD_ROW = 13;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_10 = 14;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_11 = 15;
	public const int VERTEX_SHADER_FOG_PARAMS = 16;
	public const int VERTEX_SHADER_VIEWMODEL = 17;
	public const int VERTEX_SHADER_AMBIENT_LIGHT = 21;
	public const int VERTEX_SHADER_LIGHTS = 27;
	public const int VERTEX_SHADER_LIGHT0_POSITION = 29;
	public const int VERTEX_SHADER_MODULATION_COLOR = 47;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_0 = 48;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_1 = 49;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_2 = 50;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_3 = 51;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_4 = 52;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_5 = 53;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_6 = 54;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_7 = 55;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_8 = 56;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_9 = 57;
	public const int VERTEX_SHADER_MODEL = 58;
	//
	// We reserve up through 216 for the 53 bones supported on DX9
	//
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_13 = 217;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_14 = 218;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_15 = 219;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_16 = 220;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_17 = 221;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_18 = 222;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_19 = 223;
	public const int VERTEX_SHADER_SHADER_SPECIFIC_CONST_12 = 224;

	// 226		ClipPlane0				|------ OpenGL will jam clip planes into these two
	// 227		ClipPlane1				|

	public const int VERTEX_SHADER_FLEX_WEIGHTS = 1024;
	public const int VERTEX_SHADER_MAX_FLEX_WEIGHT_COUNT = 512;
}
