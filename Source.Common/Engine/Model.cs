using Source.Common.Formats.BSP;
using Source.Common.MaterialSystem;
using Source.Common.Mathematics;
using Source.Common.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.Engine;

public struct ModelTexInfo {
	public InlineArray2<Vector4> TextureVecsTexelsPerWorldUnits;
	public InlineArray2<Vector4> LightmapVecsTexelsPerWorldUnits;
	public float LuxelsPerWorldUnit;
	public float WorldUnitsPerLuxel;
	public SurfF;
}

public class WorldBrushData
{
	public int NumSubModels;
	public CPlane[]? Planes;
	public BSPLeaf[]? Leafs;
	public BSPLeafWaterData[]? LeafWaterData;
	public BSPVertex[]? Vertexes;
	public BSPOccluderData[]? Occluders;
	public BSPOccluderPolyData[]? OccluderPolys;
	public int[]? OccluderVertIndices;
	public ushort[]? VertNormalIndices;
	public Vector3[]? VertNormals;
	public BSPNode[]? Nodes;
	public ushort[]? LeafMinDistToWater;
	public BSPTexInfo[]? TexInfo;
	public BSPTexData[]? TexData;
	public ushort[]? VertIndices;
	// TODO: Displacement info
	public BSPWorldLight[]? WorldLights;
	public BSPPrimitive[]? Primitives;
	public BSPPrimVert[]? PrimVerts;
	public ushort[]? PrimIndices;
	public BSPArea[]? Areas;
	public BSPAreaPortal[]? AreaPortals;
	public Vector3[]? ClipPortalVerts;
	public BSPCubeMapSample[]? CubemapSamples;
}

public struct BrushData
{
	public WorldBrushData? Shared;
	public int FirstModelSurface;
	public int NumModelSurfaces;
	public ushort RenderHandle;
	public ushort FirstNode;
}

public struct SpriteData
{
	public int NumFrames;
	public int Width;
	public int Height;
	// >> todo when EngineSprite exists: public EngineSprite? Sprite;
}

public enum ModelType
{
	Invalid,
	Brush,
	Sprite,
	Studio
}

public enum ModelFlag
{
	MaterialProxy = 0x0001,
	Translucent = 0x0002,
	VertexLit = 0x0004,
	TranslucentTwoPass = 0x0008,
	FramebufferTexture = 0x0010,
	HasDLight = 0x0020,
	UsesFBTexture = 0x0100,
	UsesBumpMapping = 0x0200,
	UsesEnvCubemap = 0x0400,
	AmbientBoost = 0x0800,
	DoNotCastShadows = 0x1000
}

/// <summary>
/// Analog of model_t
/// </summary>
public class Model
{
	public FileNameHandle_t FileNameHandle;
	public UtlSymbol StrName;
	public ModelLoaderFlags LoadFlags;
	public int ServerCount;
	public IMaterial[]? Materials;

	public ModelType Type;
	public ModelFlag Flags;

	public Vector3 Mins, Maxs;
	public float Radius;

	public BrushData Brush;
	public MDLHandle_t Studio;
	public SpriteData Sprite;
}