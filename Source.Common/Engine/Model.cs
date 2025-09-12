using Source.Common.Formats.BSP;
using Source.Common.MaterialSystem;
using Source.Common.Mathematics;
using Source.Common.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.Engine;

public struct WorldBrushData
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
	// TODO: Displacement info
	public BSPWorldLight[]? WorldLights;
	public BSPPrimVert[]? PrimVerts;
	public ushort[]? PrimIndices;
	public BSPArea[]? Areas;
	public BSPAreaPortal[]? AreaPortals;
	public Vector3[]? ClipPortalVerts;
	public BSPCubeMapSample[]? CubemapSamples;
}

public enum ModelType
{
	Invalid,
	Brush,
	Sprite,
	Studio
}

/// <summary>
/// Analog of model_t
/// </summary>
public class Model
{
	public FileNameHandle_t FileNameHandle;
	public UtlSymbol StrName;
	public ModelReferenceType LoadFlags;
	public int ServerCount;
	public IMaterial[]? Materials;

	public ModelType Type;
	public int Flags;

	public Vector3 Mins, Maxs;
	public float Radius;

	public object? Data;
}