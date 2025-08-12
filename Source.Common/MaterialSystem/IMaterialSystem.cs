using Source.Common.Formats.Keyvalues;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.MaterialSystem;

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

public enum MaterialCullMode
{
	CounterClockwise,
	Clockwise
}

public enum MaterialIndexFormat {
	Unknown = -1,
	x16Bits,
	x32Bits
}
public record struct VertexShaderHandle
{
	public VertexShaderHandle(nint handle) {
		Handle = handle;
	}

	public nint Handle;
	public static implicit operator nint(VertexShaderHandle handle) => handle.Handle;
	public static implicit operator VertexShaderHandle(nint handle) => new(handle);
}

public record struct GeometryShaderHandle
{
	public GeometryShaderHandle(nint handle) {
		Handle = handle;
	}

	public nint Handle;
	public static implicit operator nint(GeometryShaderHandle handle) => handle.Handle;
	public static implicit operator GeometryShaderHandle(nint handle) => new(handle);
}

public record struct PixelShaderHandle
{
	public PixelShaderHandle(nint handle) {
		Handle = handle;
	}

	public nint Handle;
	public static implicit operator nint(PixelShaderHandle handle) => handle.Handle;
	public static implicit operator PixelShaderHandle(nint handle) => new(handle);
}
public class MaterialSystemConfig {
	public int Width;
	public int Height;
	public int RefreshRate;
}

public interface IMaterialSystem
{
	IMatRenderContext GetRenderContext();
	unsafe bool InitializeGraphics(nint graphics, delegate* unmanaged[Cdecl]<byte*, void*> loadExts, int width, int height);
	void ModInit();
	void ModShutdown();
	void BeginFrame(double frameTime);
	void EndFrame();
	void SwapBuffers();
	bool SetMode(nint v, MaterialSystemConfig config);
	IMaterial CreateMaterial(string v, KeyValues keyValues);
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
}

public readonly struct MatRenderContextPtr : IDisposable, IMatRenderContext
{
	readonly IMatRenderContext ctx;
	public readonly IMatRenderContext Context => ctx;

	public MatRenderContextPtr(IMatRenderContext init) {
		ctx = init;
		init.BeginRender();
	}
	public MatRenderContextPtr(IMaterialSystem from) {
		ctx = from.GetRenderContext();
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
}