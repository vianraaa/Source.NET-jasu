using Source.Common.MaterialSystem;
using Source.Common.ShaderLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.StdShader.Gl46;

public class UnlitGeneric : BaseVSShader
{
	public static string HelpString = "Help for UnlitGeneric";
	public static int Flags = 0;
	public static List<ShaderParam> ShaderParams = [];
	public static ShaderParam[] ShaderParamOverrides = new ShaderParam[(int)ShaderMaterialVars.Count];

	public class ShaderParam
	{
		public readonly ShaderParamInfo Info;
		public readonly int Index;
		public ShaderParam(ShaderMaterialVars var, ShaderParamType type, ReadOnlySpan<char> defaultParam, ReadOnlySpan<char> help, int flags) {
			Info.Name = "override";
			Info.Type = type;
			Info.DefaultValue = new(defaultParam);
			Info.Help = new(help);
			Info.Flags = (ShaderParamFlags)flags;

			if (ShaderParamOverrides[(int)var] == null) {

			}
			else {
				Dbg.AssertMsg(false, ":(");
			}

			ShaderParamOverrides[(int)var] = this;
			Index = (int)var;
		}
		public ShaderParam(string name, ShaderParamType type, ReadOnlySpan<char> defaultParam, ReadOnlySpan<char> help, int flags) {
			Info.Name = name;
			Info.Type = type;
			Info.DefaultValue = new(defaultParam);
			Info.Help = new(help);
			Info.Flags = (ShaderParamFlags)flags;
			Index = (int)ShaderMaterialVars.Count + ShaderParams.Count;
			ShaderParams.Add(this);
		}
		public static implicit operator int(ShaderParam param) => param.Index;
		public ReadOnlySpan<char> GetName() => Info.Name;
		public ShaderParamType GetType() => Info.Type;
		public ReadOnlySpan<char> GetDefaultValue() => Info.DefaultValue;
		public int GetFlags() => (int)Info.Flags;
		public ReadOnlySpan<char> GetHelp() => Info.Help;
	}

	public static readonly ShaderParam ALBEDO = new("$ALBEDO", ShaderParamType.Texture, "shadertest/BaseTexture", "albedo (Base texture with no baked lighting", 0);

	public static void SetupVars(ref VertexLitGeneric_Gl46_Vars info) {
		info.BaseTexture = ShaderMaterialVars.BaseTexture;
		info.BaseTextureFrame = ShaderMaterialVars.Frame;
		info.BaseTextureTransform = ShaderMaterialVars.BaseTextureTransform;
		info.Albedo = ALBEDO;
	}
	protected override void OnInitShaderParams(IMaterialVar[] vars, ReadOnlySpan<char> materialName) {
		VertexLitGeneric_Gl46_Vars invars = new();
		SetupVars(ref invars);
		VertexLitGeneric_Gl46_Helper.InitParams(this, vars, materialName, false, in invars);
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
			return ShaderParams[paramIndex - baseClassParamCount].GetName();
	}
	public override ReadOnlySpan<char> GetParamHelp(int paramIndex) {
		int baseClassParamCount = base.GetNumParams();
		if (paramIndex < baseClassParamCount)
			return base.GetParamHelp(paramIndex);
		else
			return ShaderParams[paramIndex - baseClassParamCount].GetHelp();
	}
	public override ShaderParamType GetParamType(int paramIndex) {
		int baseClassParamCount = base.GetNumParams();
		if (paramIndex < baseClassParamCount)
			return base.GetParamType(paramIndex);
		else
			return ShaderParams[paramIndex - baseClassParamCount].GetType();
	}
	public override ReadOnlySpan<char> GetParamDefault(int paramIndex) {
		int baseClassParamCount = base.GetNumParams();
		if (paramIndex < baseClassParamCount)
			return base.GetParamDefault(paramIndex);
		else
			return ShaderParams[paramIndex - baseClassParamCount].GetDefaultValue();
	}
	protected override void OnInitShaderInstance(IMaterialVar[] vars, IShaderInit shaderInit, ReadOnlySpan<char> materialName) {
		VertexLitGeneric_Gl46_Vars invars = new();
		SetupVars(ref invars);
		VertexLitGeneric_Gl46_Helper.Init(this, vars, false, in invars);
	}
	protected override void OnDrawElements(IMaterialVar[] vars, IShaderShadow shaderShadow, IShaderDynamicAPI shaderAPI, VertexCompressionType vertexCompression) {
		VertexLitGeneric_Gl46_Vars invars = new();
		SetupVars(ref invars);
		VertexLitGeneric_Gl46_Helper.Draw(this, vars, false, in invars, vertexCompression);
	}
}
