using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Source;

public static class MaterialDefines
{
	public static bool IsPlatformOpenGL() => true;
	public const int MATERIAL_MAX_PATH = 256;

	// These are const strings rather than an enum since they refer to strings
	// Just have MaterialDefines in your projects global defines if you want them
	// or global-use MaterialDefines.cs
	public const string TEXTURE_GROUP_LIGHTMAP = "Lightmaps";
	public const string TEXTURE_GROUP_WORLD = "World textures";
	public const string TEXTURE_GROUP_MODEL = "Model textures";
	public const string TEXTURE_GROUP_VGUI = "VGUI textures";
	public const string TEXTURE_GROUP_PARTICLE = "Particle textures";
	public const string TEXTURE_GROUP_DECAL = "Decal textures";
	public const string TEXTURE_GROUP_SKYBOX = "SkyBox textures";
	public const string TEXTURE_GROUP_CLIENT_EFFECTS = "ClientEffect textures";
	public const string TEXTURE_GROUP_OTHER = "Other textures";
	public const string TEXTURE_GROUP_PRECACHED = "Precached";
	public const string TEXTURE_GROUP_CUBE_MAP = "CubeMap textures";
	public const string TEXTURE_GROUP_RENDER_TARGET = "RenderTargets";
	public const string TEXTURE_GROUP_RUNTIME_COMPOSITE = "Runtime Composite";
	public const string TEXTURE_GROUP_UNACCOUNTED = "Unaccounted textures";
	public const string TEXTURE_GROUP_STATIC_INDEX_BUFFER = "Static Indices";
	public const string TEXTURE_GROUP_STATIC_VERTEX_BUFFER_DISP = "Displacement Verts";
	public const string TEXTURE_GROUP_STATIC_VERTEX_BUFFER_COLOR = "Lighting Verts";
	public const string TEXTURE_GROUP_STATIC_VERTEX_BUFFER_WORLD = "World Verts";
	public const string TEXTURE_GROUP_STATIC_VERTEX_BUFFER_MODELS = "Model Verts";
	public const string TEXTURE_GROUP_STATIC_VERTEX_BUFFER_OTHER = "Other Verts";
	public const string TEXTURE_GROUP_DYNAMIC_INDEX_BUFFER = "Dynamic Indices";
	public const string TEXTURE_GROUP_DYNAMIC_VERTEX_BUFFER = "Dynamic Verts";
	public const string TEXTURE_GROUP_DEPTH_BUFFER = "DepthBuffer";
	public const string TEXTURE_GROUP_VIEW_MODEL = "ViewModel";
	public const string TEXTURE_GROUP_PIXEL_SHADERS = "Pixel Shaders";
	public const string TEXTURE_GROUP_VERTEX_SHADERS = "Vertex Shaders";
	public const string TEXTURE_GROUP_RENDER_TARGET_SURFACE = "RenderTarget Surfaces";
	public const string TEXTURE_GROUP_MORPH_TARGETS = "Morph Targets";
}
