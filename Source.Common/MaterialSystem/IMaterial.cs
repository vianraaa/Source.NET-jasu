using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Source.Common.MaterialSystem;

public enum VertexElement : int
{
	None = -1,

	Position = 0,
	Normal = 1,
	Color = 2,
	Specular = 3,
	BoneIndex = 4,
	BoneWeights = 5,
	TexCoord = 6,
	Count
}

public enum VertexAttributeType
{
	Byte = 0x1400,
	UnsignedByte = 0x1401,
	Short = 0x1402,
	UnsignedShort = 0x1403,
	Int = 0x1404,
	UnsignedInt = 0x1405,
	Float = 0x1406
}

public static class VertexExts
{
	public static nint SizeOf(this VertexAttributeType type) => type switch {
		VertexAttributeType.Byte => sizeof(sbyte),
		VertexAttributeType.UnsignedByte => sizeof(byte),
		VertexAttributeType.Short => sizeof(short),
		VertexAttributeType.UnsignedShort => sizeof(ushort),
		VertexAttributeType.Int => sizeof(int),
		VertexAttributeType.UnsignedInt => sizeof(uint),
		VertexAttributeType.Float => sizeof(float),
		_ => throw new NotSupportedException()
	};

	public static nint GetSize(this VertexElement element, VertexCompressionType compression = VertexCompressionType.None) {
		element.GetInformation(out int count, out VertexAttributeType type);
		return count * type.SizeOf();
	}

	public static void GetInformation(this VertexElement element, out int count, out VertexAttributeType type, VertexCompressionType compression = VertexCompressionType.None) {
		switch (element) {
			case VertexElement.Position:
				count = 3;
				type = VertexAttributeType.Float;
				return;
			case VertexElement.Normal:
				count = 3;
				type = VertexAttributeType.Float;
				return;
			case VertexElement.Color:
				count = 4;
				type = VertexAttributeType.UnsignedByte;
				return;
			case VertexElement.Specular:
				count = 4;
				type = VertexAttributeType.UnsignedByte;
				return;
			case VertexElement.BoneIndex:
				count = 4;
				type = VertexAttributeType.UnsignedByte;
				return;
			case VertexElement.BoneWeights:
				count = 4;
				type = VertexAttributeType.Float;
				return;
			case VertexElement.TexCoord:
				count = 2;
				type = VertexAttributeType.Float;
				return;
		}
		AssertMsg(false, "No size definition");
		count = 0;
		type = VertexAttributeType.Byte;
	}
}

[Flags]
public enum VertexFormat : ulong
{
	Position = 1 << VertexElement.Position,
	Normal = 1 << VertexElement.Normal,
	Color = 1 << VertexElement.Color,
	Specular = 1 << VertexElement.Specular,
	BoneIndex = 1 << VertexElement.BoneIndex,
	BoneWeights = 1 << VertexElement.BoneWeights,
	TexCoord = 1 << VertexElement.TexCoord,

	Invalid = 0xFFFFFFFFFFFFFFFFul
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


[Flags]
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
[Flags]
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

public static class IMaterialExts {
	public static bool IsErrorMaterial([NotNullWhen(false)] this IMaterial? mat) {
		return mat == null || mat.IsErrorMaterialInternal();
	}
}
public interface IMaterial
{
	bool IsRealTimeVersion();
	bool InMaterialPage();
	IMaterial GetMaterialPage();
	float GetMappingWidth();
	float GetMappingHeight();
	bool TryFindVar(ReadOnlySpan<char> varName, [NotNullWhen(true)] out IMaterialVar? found, bool complain = true);
	IMaterialVar FindVar(ReadOnlySpan<char> varName, out bool found, bool complain = true);
	void Refresh();
	bool IsErrorMaterialInternal();
}