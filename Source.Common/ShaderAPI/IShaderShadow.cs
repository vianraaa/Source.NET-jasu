using Source.Common.MaterialSystem;

namespace Source.Common.ShaderAPI;

public enum ShaderDepthFunc
{
	Never,
	Nearer,
	Equal,
	NearerOrEqual,
	Farther,
	NotEqual,
	FartherOrEqual,
	Always
}

public enum ShaderBlendFactor
{
	Zero,
	One,
	SrcColor,
	InvSrcColor,
	SrcAlpha,
	InvSrcAlpha,
	DstAlpha,
	InvDstAlpha,
	DstColor,
	InvDstColor,
	SrcAlphaSat,
	BothSrcAlpha,
	BothInvSrcAlpha
}

public enum ShaderBlendOp
{
	Add,
	Subtract,
	RevSubtract,
	Min,
	Max
}

public enum ShaderAlphaFunc
{
	Never,
	Less,
	Equal,
	LessEqual,
	Greater,
	NotEqual,
	GreaterEqual,
	Always
}

public enum ShaderStencilFunc
{
	Never = 0,
	Less,
	Equal,
	LessEqual,
	Greater,
	NotEqual,
	GreaterEqual,
	Always
}

public enum ShaderStencilOp
{
	Keep = 0,
	Zero,
	SetToReference,
	IncrementClamp,
	DecrementClamp,
	invert,
	IncrementWrap,
	DecrementWrap,
}

public enum ShaderTexChannel
{
	Color = 0,
	Alpha
}

public enum ShaderPolyModeFace
{
	Front,
	Back,
	FrontAndBack,
}

public enum ShaderPolyMode
{
	Point,
	Line,
	Fill
}

public enum ShaderTexArg
{
	Texture = 0,
	VertexColor,
	SpecularColor,
	ConstantColor,
	PreviousStage,
	None,
	Zero,
	TextureAlpha,
	InvTextureAlpha,
	One,
}

public enum ShaderTexOp
{
	// DX5 shaders support these
	Modulate = 0,
	Modulate2x,
	Modulate4x,
	SelectArg1,
	SelectArg2,
	Disable,

	// DX6 shaders support these
	Add,
	Subtract,
	AddSigned2x,
	BlendConstantAlpha,
	BlendTextureAlpha,
	BlendPreviousStageAlpha,
	ModulateColorAddAlpha,
	ModulateInvColorAddAlpha,

	// DX7
	DotProduct3
}

public enum ShaderTexGenParam
{
	ObjectLinear,
	EyeLinear,
	SphereMap,
	CameraSpaceReflectionVector,
	CameraSpaceNormal
}

public enum ShaderDrawBitField
{
	Position = 0x0001,
	Normal = 0x0002,
	Color = 0x0004,
	Specular = 0x0008,

	TexCoord0 = 0x0010,
	TexCoord1 = 0x0020,
	TexCoord2 = 0x0040,
	TexCoord3 = 0x0080,

	LightmapTexCoord0 = 0x0100,
	LightmapTexCoord1 = 0x0200,
	LightmapTexCoord2 = 0x0400,
	LightmapTexCoord3 = 0x0800,

	SecondaryTexCoord0 = 0x1000,
	SecondaryTexCoord1 = 0x2000,
	SecondaryTexCoord2 = 0x4000,
	SecondaryTexCoord3 = 0x8000,

	TexCoordMask = TexCoord0 | TexCoord1 | TexCoord2 | TexCoord3,
	LightmapTexCoordMask = LightmapTexCoord0 | LightmapTexCoord1 | LightmapTexCoord2 | LightmapTexCoord3,
	SecondaryTexCoordMask = SecondaryTexCoord0 | SecondaryTexCoord1 | SecondaryTexCoord2 | SecondaryTexCoord3
}
public enum ShaderFogMode
{
	Disabled = 0,
	Overbright,
	Black,
	Grey,
	FogColor,
	White,
	Num
}
public enum ShaderMaterialSource
{
	Material,
	Color1,
	Color2
}
public enum PolygonOffsetMode
{
	Disable = 0x0,
	Decal = 0x1,
	ShadowBias = 0x2,
	Reserved = 0x3
}
public interface IShaderShadow
{
	// Sets the default *shadow* state
	void SetDefaultState();

	// Methods related to depth buffering
	void DepthFunc(ShaderDepthFunc depthFunc);
	void EnableDepthWrites(bool enable);
	void EnableDepthTest(bool enable);
	void EnablePolyOffset(PolygonOffsetMode offsetMode);

	// These methods for controlling stencil are obsolete and stubbed to do nothing.  Stencil
	// control is via the shaderapi/material system now, not part of the shadow state.
	// Methods related to stencil
	void EnableStencil(bool enable);
	void StencilFunc(ShaderStencilFunc stencilFunc);
	void StencilPassOp(ShaderStencilOp stencilOp);
	void StencilFailOp(ShaderStencilOp stencilOp);
	void StencilDepthFailOp(ShaderStencilOp stencilOp);
	void StencilReference(int nReference);
	void StencilMask(int nMask);
	void StencilWriteMask(int nMask);

	// Suppresses/activates color writing 
	void EnableColorWrites(bool enable);
	void EnableAlphaWrites(bool enable);

	// Methods related to alpha blending
	void EnableBlending(bool enable);
	void BlendFunc(ShaderBlendFactor srcFactor, ShaderBlendFactor dstFactor);
	// More below...

	// Alpha testing
	void EnableAlphaTest(bool enable);
	void AlphaFunc(ShaderAlphaFunc alphaFunc, float alphaRef /* [0-1] */ );

	// Wireframe/filled polygons
	void PolyMode(ShaderPolyModeFace face, ShaderPolyMode polyMode);

	// Back face culling
	void EnableCulling(bool enable);

	// constant color + transparency
	void EnableConstantColor(bool enable);

	// Indicates the vertex format for use with a vertex shader
	// The flags to pass in here come from the VertexFormatFlags_t enum
	// If pTexCoordDimensions is *not* specified, we assume all coordinates
	// are 2-dimensional
	void VertexShaderVertexFormat(uint flags,
			int texCoordCount, Span<int> texCoordDimensions, int userDataSize);

	// Pixel and vertex shader methods
	void SetVertexShader(ReadOnlySpan<char> fileName, int nStaticVshIndex);
	void SetPixelShader(ReadOnlySpan<char> fileName, int nStaticPshIndex = 0);

	// Indicates we're going to light the model
	void EnableLighting(bool enable);

	// Enables specular lighting (lighting has also got to be enabled)
	void EnableSpecular(bool enable);

	// Convert from linear to gamma color space on writes to frame buffer.
	void EnableSRGBWrite(bool enable);

	// Convert from gamma to linear on texture fetch.
	void EnableSRGBRead(Sampler sampler, bool enable);

	// Activate/deactivate skinning. Indexed blending is automatically
	// enabled if it's available for this hardware. When blending is enabled,
	// we allocate enough room for 3 weights (max allowed)
	void EnableVertexBlend(bool enable);

	// per texture unit stuff
	void OverbrightValue(TextureStage stage, float value);
	void EnableTexture(Sampler sampler, bool enable);
	void EnableTexGen(TextureStage stage, bool enable);
	void TexGen(TextureStage stage, ShaderTexGenParam param);

	// alternate method of specifying per-texture unit stuff, more flexible and more complicated
	// Can be used to specify different operation per channel (alpha/color)...
	void EnableCustomPixelPipe(bool enable);
	void CustomTextureStages(int stageCount);
	void CustomTextureOperation(TextureStage stage, ShaderTexChannel channel, ShaderTexOp op, ShaderTexArg arg1, ShaderTexArg arg2);

	// indicates what per-vertex data we're providing
	void DrawFlags(ShaderDrawBitField drawFlags);

	// A simpler method of dealing with alpha modulation
	void EnableAlphaPipe(bool enable);
	void EnableConstantAlpha(bool enable);
	void EnableVertexAlpha(bool enable);
	void EnableTextureAlpha(TextureStage stage, bool enable);

	// GR - Separate alpha blending
	void EnableBlendingSeparateAlpha(bool enable);
	void BlendFuncSeparateAlpha(ShaderBlendFactor srcFactor, ShaderBlendFactor dstFactor);
	void FogMode(ShaderFogMode fogMode);

	void SetDiffuseMaterialSource(ShaderMaterialSource materialSource);

	// Indicates the morph format for use with a vertex shader
	// The flags to pass in here come from the MorphFormatFlags_t enum
	// void SetMorphFormat(MorphFormat flags);

	void DisableFogGammaCorrection(bool bDisable); //some blending modes won't work properly with corrected fog

	// Alpha to coverage
	void EnableAlphaToCoverage(bool enable);

	// Shadow map filtering
	void SetShadowDepthFiltering(Sampler stage);

	// More alpha blending state
	void BlendOp(ShaderBlendOp blendOp);
	void BlendOpSeparateAlpha(ShaderBlendOp blendOp);
}
