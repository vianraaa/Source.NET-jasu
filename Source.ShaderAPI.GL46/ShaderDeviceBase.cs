using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

namespace Source.ShaderAPI.Gl46;

public abstract class ShaderDeviceBase : IShaderDevice
{
	public bool AddView(nint window) {
		return true;
	}

	public abstract GeometryShaderHandle CreateGeometryShader(ReadOnlySpan<char> program, ReadOnlySpan<char> shaderVersion);
	public abstract GeometryShaderHandle CreateGeometryShader(Stream program, ReadOnlySpan<char> shaderVersion);
	public abstract IIndexBuffer CreateIndexBuffer(ShaderBufferType type, MaterialIndexFormat format, int count);
	public abstract PixelShaderHandle CreatePixelShader(ReadOnlySpan<char> program, ReadOnlySpan<char> shaderVersion);
	public abstract PixelShaderHandle CreatePixelShader(Stream program, ReadOnlySpan<char> shaderVersion);
	public abstract IVertexBuffer CreateVertexBuffer(ShaderBufferType type, VertexFormat format, int count);
	public abstract VertexShaderHandle CreateVertexShader(ReadOnlySpan<char> program, ReadOnlySpan<char> shaderVersion);
	public abstract VertexShaderHandle CreateVertexShader(Stream program, ReadOnlySpan<char> shaderVersion);

	public abstract void DestroyGeometryShader(GeometryShaderHandle shaderHandle);
	public abstract void DestroyIndexBuffer(IIndexBuffer vertexBuffer);
	public abstract void DestroyPixelShader(PixelShaderHandle shaderHandle);
	public abstract void DestroyVertexBuffer(IVertexBuffer vertexBuffer);
	public abstract void DestroyVertexShader(VertexShaderHandle shaderHandle);

	public void GetBackBufferDimensions(out int width, out int height) {
		throw new NotImplementedException();
	}

	public ImageFormat GetBackBufferFormat() {
		throw new NotImplementedException();
	}

	public abstract IIndexBuffer GetDynamicIndexBuffer(int streamID, VertexFormat format, bool buffered = true);

	public abstract IVertexBuffer GetDynamicVertexBuffer(int streamID, VertexFormat format, bool buffered = true);

	public void GetWindowSize(out int width, out int height) {
		throw new NotImplementedException();
	}

	public bool IsAAEnabled() {
		throw new NotImplementedException();
	}

	public bool IsUsingGraphics() {
		throw new NotImplementedException();
	}

	public void Present() {
		throw new NotImplementedException();
	}

	public bool RemoveView(nint window) {
		return true;
	}

	public void SetHardwareGammaRamp(float gamma, float tvRangeMin, float tvRangeMax, float tvExponent, bool tvEnabled) {
		throw new NotImplementedException();
	}

	public bool SetView(nint window) {
		return true;
	}

	public void SpewDriverInfo() {
		throw new NotImplementedException();
	}

	public int StencilBufferBits() {
		throw new NotImplementedException();
	}
}