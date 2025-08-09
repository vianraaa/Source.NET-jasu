namespace Source.Common.ShaderAPI;

using Source.Common.Filesystem;
using Source.Common.MaterialSystem;
using System.Reflection.Metadata;

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

public struct ShaderDisplayMode {
	public int Version;
	public int Width;
	public int Height;
	public ImageFormat Format;
	int RefreshRateNumerator;
	int RefreshRateDenominator;
}

[Flags]
public enum GraphicsAPI : ulong {
	// 8 bits available for the overall API
	DirectX = 1 << 62,
	OpenGL = 1 << 61,
	Vulkan = 1 << 60,
	Metal = 1 << 59,
	// 16 bits available for versions
	v460 = 460,

	// Merged enums, only do this where actually applicable
	OpenGL_v460 = OpenGL | v460
}
public static class GraphicsAPIExts {
	public static void GetGLInfo(this GraphicsAPI graphicsAPI, out int major, out int minor) {
		if (!graphicsAPI.HasFlag(GraphicsAPI.OpenGL))
			throw new NotSupportedException();
		int version = (int)graphicsAPI & 0xFFFF;
		major = version / 100;
		minor = (version / 10) % 10;
	}
}

public struct ShaderDeviceInfo {
	public int Version;
	public ShaderDisplayMode DisplayMode;
	public int BackBufferCount;
	public int AASamples;
	public int AAQuality;
	public GraphicsAPI APILevel;
	public int WindowedSizeLimitWidth;
	public int WindowedSizeLimitHeight;
	
	public bool Windowed;
	public bool Resizing;
	public bool UseStencil;
	public bool LimitWindowedSize;
	public bool WaitForVSync;
	public bool ScaleToOutputResolution;
	public bool Progressive;
	public bool UsingMultipleWindows;
}

public enum ShaderBufferType {
	Static = 0,
	Dynamic
}

public interface IShaderDevice {
	GraphicsAPI GetGraphicsAPI();
	ImageFormat GetBackBufferFormat();
	void GetBackBufferDimensions(out int width, out int height);
	bool IsUsingGraphics();
	void SpewDriverInfo();
	int StencilBufferBits();
	bool IsAAEnabled();
	void Present();
	void GetWindowSize(out int width, out int height);
	void SetHardwareGammaRamp(float gamma, float tvRangeMin, float tvRangeMax, float tvExponent, bool tvEnabled);

	void DestroyVertexShader(VertexShaderHandle shaderHandle);
	void DestroyGeometryShader(GeometryShaderHandle shaderHandle);
	void DestroyPixelShader(PixelShaderHandle shaderHandle);

	// Utility methods to make shader creation simpler
	VertexShaderHandle CreateVertexShader(ReadOnlySpan<char> program, ReadOnlySpan<char> shaderVersion);
	GeometryShaderHandle CreateGeometryShader(ReadOnlySpan<char> program, ReadOnlySpan<char> shaderVersion);
	PixelShaderHandle CreatePixelShader(ReadOnlySpan<char> program, ReadOnlySpan<char> shaderVersion);

	VertexShaderHandle CreateVertexShader(Stream program, ReadOnlySpan<char> shaderVersion);
	GeometryShaderHandle CreateGeometryShader(Stream program, ReadOnlySpan<char> shaderVersion);
	PixelShaderHandle CreatePixelShader(Stream program, ReadOnlySpan<char> shaderVersion);

	VertexShaderHandle CreateVertexShader(IFileHandle program, ReadOnlySpan<char> shaderVersion) 
		=> program == null ? default : CreateVertexShader(program.Stream, shaderVersion);
	GeometryShaderHandle CreateGeometryShader(IFileHandle program, ReadOnlySpan<char> shaderVersion) 
		=> program == null ? default : CreateGeometryShader(program.Stream, shaderVersion);
	PixelShaderHandle CreatePixelShader(IFileHandle? program, ReadOnlySpan<char> shaderVersion) 
		=> program == null ? default : CreatePixelShader(program.Stream, shaderVersion);

	IVertexBuffer CreateVertexBuffer(ShaderBufferType type, VertexFormat format, int count);
	void DestroyVertexBuffer(IVertexBuffer vertexBuffer);

	IIndexBuffer CreateIndexBuffer(ShaderBufferType type, MaterialIndexFormat format, int count);
	void DestroyIndexBuffer(IIndexBuffer vertexBuffer);

	IVertexBuffer GetDynamicVertexBuffer(int streamID, VertexFormat format, bool buffered = true);
	IIndexBuffer GetDynamicIndexBuffer(int streamID, VertexFormat format, bool buffered = true);
}