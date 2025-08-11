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

public interface IMatRenderContext {
	void BeginRender();
	void EndRender();
	void Flush(bool flushHardware);
}

public interface IMaterialSystem
{
	unsafe bool InitializeGraphics(nint graphics, delegate* unmanaged[Cdecl]<byte*, void*> loadExts, int width, int height);
	void ModInit();
	void ModShutdown();
	void BeginFrame(double frameTime);
	void EndFrame();
}