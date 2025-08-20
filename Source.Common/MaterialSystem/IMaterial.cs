using System.Runtime.CompilerServices;

namespace Source.Common.MaterialSystem;

public readonly struct VertexFormatFlags
{
	public const int VertexFormatPosition = 0x0001;
	public const int VertexFormatNormal = 0x0002;
	public const int VertexFormatColor = 0x0004;
	public const int VertexFormatSpecular = 0x0008;
	public const int VertexFormatTangentS = 0x0010;
	public const int VertexFormatTangentT = 0x0020;
	public const int VertexFormatTangentSpace = (VertexFormatTangentS | VertexFormatTangentT);
	// Indicates we're using wrinkle
	public const int VertexFormatWrinkle = 0x0040;
	// Indicates we're using bone indices
	public const int VertexFormatBoneIndex = 0x0080;
	// Indicates this is a vertex shader
	public const int VertexFormatVertexShader = 0x0100;
	// Indicates this format shouldn't be bloated to cache align it
	// (only used for VertexUsage)
	public const int VertexFormatUseExactFormat = 0x0200;
	// Indicates that compressed vertex elements are to be used (see also VertexCompressionType_t)
	public const int VertexFormatCompressed = 0x400;
	// Update this if you add or remove bits...
	public const int VertexLastBit = 10;
	public const int VertexBoneWeightBit = (VertexLastBit + 1);
	public const int UserDataSizeBit = (VertexLastBit + 4);
	public const int TexCoordSizeBit = (VertexLastBit + 7);
	public const int VertexBoneWeightMask = (0x7 << VertexBoneWeightBit);
	public const int UserDataSizeMask = (0x7 << UserDataSizeBit);
	public const int FieldMask = 0x0FF;
	// If everything is off, it's an unknown vertex format
	public const int Unknown = 0;
}

public enum VertexElement : int
{
	None = -1,
	Position = 0,
	Normal = 1,
	Color = 2,
	Specular = 3,
	TangentS = 4,
	TangentT = 5,
	Wrinkle = 6,
	BoneIndex = 7,
	BoneWeights1 = 8,
	BoneWeights2 = 9,
	BoneWeights3 = 10,
	BoneWeights4 = 11,
	UserData1 = 12,
	UserData2 = 13,
	UserData3 = 14,
	UserData4 = 15,
	TexCoord1D_0 = 16,
	TexCoord1D_1 = 17,
	TexCoord1D_2 = 18,
	TexCoord1D_3 = 19,
	TexCoord1D_4 = 20,
	TexCoord1D_5 = 21,
	TexCoord1D_6 = 22,
	TexCoord1D_7 = 23,
	TexCoord2D_0 = 24,
	TexCoord2D_1 = 25,
	TexCoord2D_2 = 26,
	TexCoord2D_3 = 27,
	TexCoord2D_4 = 28,
	TexCoord2D_5 = 29,
	TexCoord2D_6 = 30,
	TexCoord2D_7 = 31,
	TexCoord3D_0 = 32,
	TexCoord3D_1 = 33,
	TexCoord3D_2 = 34,
	TexCoord3D_3 = 35,
	TexCoord3D_4 = 36,
	TexCoord3D_5 = 37,
	TexCoord3D_6 = 38,
	TexCoord3D_7 = 39,
	TexCoord4D_0 = 40,
	TexCoord4D_1 = 41,
	TexCoord4D_2 = 42,
	TexCoord4D_3 = 43,
	TexCoord4D_4 = 44,
	TexCoord4D_5 = 45,
	TexCoord4D_6 = 46,
	TexCoord4D_7 = 47,

	Count = 48
}

[Flags]
public enum VertexFormat : ulong
{
	Position = 1 << VertexElement.Position,
	Normal = 1 << VertexElement.Normal,
	Color = 1 << VertexElement.Color,
	Specular = 1 << VertexElement.Specular,
	TangentS = 1 << VertexElement.TangentS,
	TangentT = 1 << VertexElement.TangentT,
	TangentSpace = TangentS | TangentT,
	UseExactFormat = VertexFormatFlags.VertexFormatUseExactFormat,
	Compressed = VertexFormatFlags.VertexFormatCompressed,
}

public enum StencilOperation
{
	Keep = 1,
	Zero = 2,
	Replace = 3,
	IncrSat = 4,
	DecrSat = 5,
	Invert = 6,
	Incr = 7,
	Decr = 8,
}

public enum StencilComparisonFunction
{
	Never = 1,
	Less = 2,
	Equal = 3,
	LessEqual = 4,
	Greater = 5,
	NotEqual = 6,
	GreaterEqual = 7,
	Always = 8,
}



public enum MaterialVarFlags
{
	Debug = (1 << 0),
	NoDebugOverride = (1 << 1),
	NoDraw = (1 << 2),
	UseInFillrateMode = (1 << 3),

	VertexColor = (1 << 4),
	VertexAlpha = (1 << 5),
	SelfIllum = (1 << 6),
	Additive = (1 << 7),
	AlphaTest = (1 << 8),
	Multipass = (1 << 9),
	ZNearer = (1 << 10),
	Model = (1 << 11),
	Flat = (1 << 12),
	NoCull = (1 << 13),
	NoFog = (1 << 14),
	IgnoreZ = (1 << 15),
	Decal = (1 << 16),
	EnvMapSphere = (1 << 17),
	NoAlphaMod = (1 << 18),
	EnvMapCameraSpace = (1 << 19),
	BaseAlphaEnvMapMask = (1 << 20),
	Translucent = (1 << 21),
	NormalMapAlphaEnvMapMask = (1 << 22),
	NeedsSoftwareSkinning = (1 << 23),
	OpaqueTexture = (1 << 24),
	EnvMapMode = (1 << 25),
	SuppressDecals = (1 << 26),
	HalfLambert = (1 << 27),
	Wireframe = (1 << 28),
	AllowAlphaToCoverage = (1 << 29),
	IgnoreAlphaModulation = (1 << 30)
}

public enum MaterialVarFlags2
{
	// NOTE: These are for $flags2!!!!!
	//	UNUSED											= (1 << 0),

	LightingUnlit = 0,
	LightingVertexLit = (1 << 1),
	LightingLightmap = (1 << 2),
	LightingBumpedLightmap = (1 << 3),
	LightingMask =
		(LightingVertexLit |
		  LightingLightmap |
		  LightingBumpedLightmap),

	// FIXME: Should this be a part of the above lighting enums?
	DiffuseBumpmappedModel = (1 << 4),
	UsesEnvCubemap = (1 << 5),
	NeedsTangentSpaces = (1 << 6),
	NeedsSoftwareLighting = (1 << 7),
	// GR - HDR path puts lightmap alpha in separate texture...
	BlendWithLightmapAlpha = (1 << 8),
	NeedsBakedLightingSnapshots = (1 << 9),
	UseFlashlight = (1 << 10),
	UseFixedFunctionBakedLighting = (1 << 11),
	NeedsFixedFunctionFlashlight = (1 << 12),
	UseEditor = (1 << 13),
	NeedsPowerOfTwoFrameBufferTexture = (1 << 14),
	NeedsFullFrameBufferTexture = (1 << 15),
	IsSpritecard = (1 << 16),
	UsesVertexID = (1 << 17),
	SupportsHardwareSkinning = (1 << 18),
	SupportsFlashlight = (1 << 19),
}

public interface IMaterial
{
	
}
public static class IMaterialExts {
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int VertexFlags(this VertexFormat format) => (int)format & ((1 << (VertexFormatFlags.VertexLastBit + 1)) - 1);
	public static int NumBoneWeights(this VertexFormat format) => ((int)format >> VertexFormatFlags.VertexBoneWeightBit) & 0x7;
	public static int UserDataSize(this VertexFormat format) => ((int)format >> VertexFormatFlags.UserDataSizeBit) & 0x7;
	public static int TexCoordSize(this VertexFormat format, int texCoordIndex) => ((int)format >> (VertexFormatFlags.TexCoordSizeBit + 3 * texCoordIndex)) & 0x7;
	public static bool UsesVertexShader(this VertexFormat format) => ((int)format & VertexFormatFlags.VertexFormatVertexShader) != 0;
	public static VertexCompressionType CompressionType(this VertexFormat format) => ((int)format & VertexFormatFlags.VertexFormatCompressed) != 0 ? VertexCompressionType.On : VertexCompressionType.None;
}