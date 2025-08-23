using Source.Common.ShaderAPI;
using Source.Common.ShaderLib;

using System.Numerics;

namespace Source.Common.MaterialSystem;


public struct ShaderParamInfo
{
	public string Name;
	public string Help;
	public ShaderParamType Type;
	public string? DefaultValue;
	public ShaderParamFlags Flags;
}


public interface IShader
{
	string? GetName();
	int GetFlags();
	int GetNumParams();
	ReadOnlySpan<char> GetParamName(int paramIndex);
	ReadOnlySpan<char> GetParamHelp(int paramIndex);
	ShaderParamType GetParamType(int paramIndex);
    ReadOnlySpan<char> GetParamDefault(int paramIndex);
	string? GetFallbackShader(IMaterialVar[] vars);
	void InitShaderParams(IMaterialVar[] vars, IShaderAPI shaderAPI, ReadOnlySpan<char> materialName);
	void InitShaderInstance(IMaterialVar[] shaderParams, IShaderInit shaderManager, ReadOnlySpan<char> materialName, ReadOnlySpan<char> textureGroupName);
	void DrawElements(IMaterialVar[] shaderParams, IShaderDynamicAPI shaderAPI, VertexCompressionType none);
}

public interface IShaderInit {
	public void LoadTexture(IMaterialVar textureVar, ReadOnlySpan<char> textureGroupName, int additionalCreationFlags = 0);
}


public enum VertexCompressionType : uint
{
	Invalid = 0xFFFFFFFF,
	None = 0,
	On = 1
}

public struct ShaderViewport {
	public int TopLeftX;
	public int TopLeftY;
	public int Width;
	public int Height;
	public float MinZ;
	public float MaxZ;

	public ShaderViewport() {

	}

	public ShaderViewport(int x, int y, int width, int height, float minZ = 0.0f, float maxZ = 1.0f) {
		TopLeftX = x;
		TopLeftY = y;
		Width = width;
		Height = height;
		MinZ = minZ;
		MaxZ = maxZ;
	}
}

public interface IShaderDynamicAPI
{
	MaterialFogMode GetSceneFogMode();
	bool InFlashlightMode();
	void PushMatrix();
	void PopMatrix();
	IMesh GetDynamicMesh(IMaterial material, int nCurrentBoneCount, bool buffered, IMesh? vertexOverride, IMesh? indexOverride);
	bool InEditorMode();


	void BindVertexShader(in VertexShaderHandle vertexShader);
	void BindGeometryShader(in GeometryShaderHandle geometryShader);
	void BindPixelShader(in PixelShaderHandle pixelShader);

	void SetVertexShaderConstant(int constant, int integer);
	void SetVertexShaderConstant(int constant, float fl);
	void SetVertexShaderConstant(int constant, ReadOnlySpan<float> flConsts);
	void SetPixelShaderConstant(int constant, int integer);
	void SetPixelShaderConstant(int constant, float fl);
	void SetPixelShaderConstant(int constant, ReadOnlySpan<float> flConsts);

	void MatrixMode(MaterialMatrixMode i);
	void LoadMatrix(in Matrix4x4 transposeTop);
	void LoadIdentity();
	int GetCurrentNumBones();
	GraphicsDriver GetDriver();
}