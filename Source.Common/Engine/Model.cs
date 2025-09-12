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