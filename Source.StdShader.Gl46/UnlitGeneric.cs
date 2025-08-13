using Source.Common.MaterialSystem;
using Source.Common.ShaderLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.StdShader.Gl46;

public class UnlitGeneric : BaseShaderGl46
{
	public static string HelpString = "Help for UnlitGeneric";
	public static int Flags = 0;
	public static List<ShaderParam> ShaderParams = [];
	public static ShaderParam[] ShaderParamOverrides = new ShaderParam[(int)ShaderMaterialVars.Count];

	public class ShaderParam
	{
		public readonly ShaderParamType Type;
		public readonly string Name;
		public readonly string DefaultValue;
		public readonly string? Help;
		public readonly int Flags;
		public readonly int Index;
		public ShaderParam(ShaderMaterialVars var, ShaderParamType type, ReadOnlySpan<char> defaultParam, ReadOnlySpan<char> help, int flags) {
			Name = "override";
			Type = type;
			DefaultValue = new(defaultParam);
			Help = new(help);
			Flags = flags;

			if (ShaderParamOverrides[(int)var] == null) {

			}
			else {
				Dbg.AssertMsg(false, ":(");
			}

			ShaderParamOverrides[(int)var] = this;
			Index = (int)var;
		}
		public ShaderParam(string name, ShaderParamType type, ReadOnlySpan<char> defaultParam, ReadOnlySpan<char> help, int flags) {
			Name = name;
			Type = type;
			DefaultValue = new(defaultParam);
			Help = new(help);
			Flags = flags;
			Index = (int)ShaderMaterialVars.Count + ShaderParams.Count;
			ShaderParams.Add(this);
		}
	}

	public static readonly ShaderParam ALBEDO = new("$ALBEDO", ShaderParamType.Texture, "shadertest/BaseTexture", "albedo (Base texture with no baked lighting", 0);

	public static void SetupVars(ref VertexLitGeneric_Gl46_Vars info) {
		info.BaseTexture = ShaderMaterialVars.BaseTexture;
	}
	protected override void OnInitShaderParams(IMaterialVar[] vars, string materialName) {
		VertexLitGeneric_Gl46_Vars invars = new();
		SetupVars(ref invars);
		//InitParamsVertexLitGeneric_Gl46(this, vars, materialName, false, ref invars);
	}
	public override string? GetFallbackShader(IMaterialVar[] vars) {
		return null;
	}
	public override int GetFlags() => Flags;
	public override int GetNumParams() => base.GetNumParams() + ShaderParams.Count;
	public override ReadOnlySpan<char> GetParamName(int paramIndex) {
		int baseClassParamCount = base.GetNumParams();
		if (paramIndex < baseClassParamCount)
			return base.GetParamName(paramIndex);
		else
			return ShaderParams[paramIndex - baseClassParamCount].Name;
	}
	public override ReadOnlySpan<char> GetParamHelp(int paramIndex) {
		int baseClassParamCount = base.GetNumParams();
		if (paramIndex < baseClassParamCount)
			return base.GetParamHelp(paramIndex);
		else
			return ShaderParams[paramIndex - baseClassParamCount].Help;
	}
	public override ShaderParamType GetParamType(int paramIndex) {
		int baseClassParamCount = base.GetNumParams();
		if (paramIndex < baseClassParamCount)
			return base.GetParamType(paramIndex);
		else
			return ShaderParams[paramIndex - baseClassParamCount].Type;
	}
	public override ReadOnlySpan<char> GetParamDefault(int paramIndex) {
		int baseClassParamCount = base.GetNumParams();
		if (paramIndex < baseClassParamCount)
			return base.GetParamDefault(paramIndex);
		else
			return ShaderParams[paramIndex - baseClassParamCount].DefaultValue;
	}
	protected override void OnInitShaderInstance(IMaterialVar[] vars, IShaderInit shaderInit, ReadOnlySpan<char> materialName) {
		VertexLitGeneric_Gl46_Vars invars = new();
		SetupVars(ref invars);
		//InitVertexLitGeneric_Gl46(this, vars, materialName, false, ref invars);
	}
	protected override void OnDrawElements(IMaterialVar[] vars, IShaderShadow shaderShadow, IShaderDynamicAPI shaderAPI, VertexCompressionType vertexCompression) {
		VertexLitGeneric_Gl46_Vars invars = new();
		SetupVars(ref invars);
		//DrawVertexLitGeneric_DX9(this, GetNumParams, shaderAPI, shaderShadow, false, vars, vertexCompression);
	}
}
