using Source.Common.Bitmap;
using Source.Common.Formats.Keyvalues;
using Source.Common.ShaderAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.MaterialSystem;

public enum MaterialIndexFormat
{
	Unknown = -1,
	x16 = 0,
	x32,
}

public enum MaterialBufferTypes {
	Front,
	Back
}

public enum MaterialPrimitiveType {
	Points,
	Lines,
	Triangles,
	TriangleStrip,
	LineStrip,
	LineLoop,
	Polygon,
	Quads,
	InstancedQuads,
	Heterogenous
}

public enum MaterialFogMode {
	None,
	Linear,
	LinearBelowFogZ
}

public enum ShaderParamType {
	Texture,
	Integer,
	Color,
	Vec2,
	Vec3,
	Vec4,
	EnvMap,
	Float,
	Bool,
	FourCC,
	Matrix,
	Material,
	String,
	Matrix4x2
}

public enum MaterialMatrixMode {
	View,
	Projection,
	Texture0,
	Texture1,
	Texture2,
	Texture3,
	Texture4,
	Texture5,
	Texture6,
	Texture7,
	Model,
	Count
}

public enum MaterialFindContext {
	None,
	IsOnAModel
}

public record struct VertexShaderHandle
{
	public static readonly VertexShaderHandle INVALID = new(-1);
	public VertexShaderHandle(nint handle) {
		Handle = handle;
	}

	public nint Handle;
	public static implicit operator nint(VertexShaderHandle handle) => handle.Handle;
	public static implicit operator VertexShaderHandle(nint handle) => new(handle);
}

public record struct GeometryShaderHandle
{
	public static readonly GeometryShaderHandle INVALID = new(-1);
	public GeometryShaderHandle(nint handle) {
		Handle = handle;
	}

	public nint Handle;
	public static implicit operator nint(GeometryShaderHandle handle) => handle.Handle;
	public static implicit operator GeometryShaderHandle(nint handle) => new(handle);
}

public record struct PixelShaderHandle
{
	public static readonly PixelShaderHandle INVALID = new(-1);
	public PixelShaderHandle(nint handle) {
		Handle = handle;
	}

	public nint Handle;
	public static implicit operator nint(PixelShaderHandle handle) => handle.Handle;
	public static implicit operator PixelShaderHandle(nint handle) => new(handle);
}
public struct MaterialVideoMode
{
	public int Width;            // if width and height are 0 and you select 
	public int Height;           // windowed mode, it'll use the window size
	public ImageFormat Format;   // use ImageFormats (ignored for windowed mode)
	public int RefreshRate;      // 0 == default (ignored for windowed mode)
};

public interface IMaterialSystem
{
	IMatRenderContext GetRenderContext();
	unsafe bool InitializeGraphics(nint graphics, delegate* unmanaged[Cdecl]<byte*, void*> loadExts, int width, int height);
	void ModInit();
	void ModShutdown();
	void BeginFrame(double frameTime);
	void EndFrame();
	void SwapBuffers();
	bool SetMode(nint v, MaterialSystem_Config config);
	IMaterial CreateMaterial(string v, KeyValues keyValues);
	bool CanUseEditorMaterials();
}

public interface IMatRenderContext
{
	void BeginRender();
	void EndRender();
	void Flush(bool flushHardware);

	void ClearBuffers(bool clearColor, bool clearDepth, bool clearStencil = false);

	void Viewport(int x, int y, int width, int height);
	void GetViewport(out int x, out int y, out int width, out int height);

	void ClearColor3ub(byte r, byte g, byte b);
	void ClearColor4ub(byte r, byte g, byte b, byte a);
	void DepthRange(double near, double far);

	void MatrixMode(MaterialMatrixMode mode);
	void PushMatrix();
	void PopMatrix();
	void LoadIdentity();
	void Bind(IMaterial material, object? proxyData);
	IMaterial? GetCurrentMaterial();
	IShaderAPI GetShaderAPI();
	IMesh GetDynamicMesh(bool buffered, IMesh? vertexOverride = null, IMesh? indexOverride = null, IMaterial? autoBind = null);
}

public readonly struct MatRenderContextPtr : IDisposable, IMatRenderContext
{
	readonly IMatRenderContext ctx;
	readonly IShaderAPI shaderAPI;
	public readonly IMatRenderContext Context => ctx;

	public MatRenderContextPtr(IMatRenderContext init) {
		ctx = init;
		shaderAPI = init.GetShaderAPI();
		init.BeginRender();
	}
	public MatRenderContextPtr(IMaterialSystem from) {
		ctx = from.GetRenderContext();
		shaderAPI = ctx.GetShaderAPI();
		ctx.BeginRender();
	}

	public readonly void Dispose() {
		ctx.EndRender();
	}

	public void BeginRender() {
		ctx.BeginRender();
	}

	public void EndRender() {
		ctx.EndRender();
	}

	public void Flush(bool flushHardware) {
		ctx.Flush(flushHardware);
	}

	public void ClearBuffers(bool clearColor, bool clearDepth, bool clearStencil = false) {
		ctx.ClearBuffers(clearColor, clearDepth, clearStencil);
	}

	public void Viewport(int x, int y, int width, int height) {
		ctx.Viewport(x, y, width, height);
	}

	public void GetViewport(out int x, out int y, out int width, out int height) {
		ctx.GetViewport(out x, out y, out width, out height);
	}

	public void ClearColor3ub(byte r, byte g, byte b) {
		ctx.ClearColor3ub(r, g, b);
	}

	public void ClearColor4ub(byte r, byte g, byte b, byte a) {
		ctx.ClearColor4ub(r, g, b, a);
	}

	public void DepthRange(double near, double far) {
		ctx.DepthRange(near, far);
	}

	public void MatrixMode(MaterialMatrixMode mode) {
		ctx.MatrixMode(mode);
	}

	public void PushMatrix() {
		ctx.PushMatrix();
	}

	public void LoadIdentity() {
		ctx.LoadIdentity();
	}

	public void Bind(IMaterial material, object? proxyData) {
		ctx.Bind(material, proxyData);
	}

	public IMaterial? GetCurrentMaterial() {
		return ctx.GetCurrentMaterial();
	}

	public void PopMatrix() {
		ctx.PopMatrix();
	}

	public IShaderAPI GetShaderAPI() {
		return ctx.GetShaderAPI();
	}

	public IMesh GetDynamicMesh(bool buffered, IMesh? vertexOverride = null, IMesh? indexOverride = null, IMaterial? autoBind = null) {
		return ctx.GetDynamicMesh(buffered, vertexOverride, indexOverride, autoBind);
	}
}