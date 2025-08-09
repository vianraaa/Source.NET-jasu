using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.ShaderAPI.Gl46;

public class ShaderDeviceGl46 : ShaderDeviceBase
{
	public override IShaderBuffer CompileShader(ReadOnlySpan<char> program, ReadOnlySpan<char> shaderVersion) {
		throw new NotImplementedException();
	}

	public override GeometryShaderHandle CreateGeometryShader(IShaderBuffer shaderBuffer) {
		throw new NotImplementedException();
	}

	public override GeometryShaderHandle CreateGeometryShader(ReadOnlySpan<char> program, ReadOnlySpan<char> shaderVersion) {
		throw new NotImplementedException();
	}

	public override GeometryShaderHandle CreateGeometryShader(Stream program, ReadOnlySpan<char> shaderVersion) {
		throw new NotImplementedException();
	}

	public override IIndexBuffer CreateIndexBuffer(ShaderBufferType type, MaterialIndexFormat format, int count) {
		throw new NotImplementedException();
	}

	public override PixelShaderHandle CreatePixelShader(IShaderBuffer shaderBuffer) {
		throw new NotImplementedException();
	}

	public override PixelShaderHandle CreatePixelShader(ReadOnlySpan<char> program, ReadOnlySpan<char> shaderVersion) {
		throw new NotImplementedException();
	}

	public override PixelShaderHandle CreatePixelShader(Stream program, ReadOnlySpan<char> shaderVersion) {
		throw new NotImplementedException();
	}

	public override IVertexBuffer CreateVertexBuffer(ShaderBufferType type, VertexFormat format, int count) {
		throw new NotImplementedException();
	}

	public override VertexShaderHandle CreateVertexShader(IShaderBuffer shaderBuffer) {
		throw new NotImplementedException();
	}

	public override VertexShaderHandle CreateVertexShader(ReadOnlySpan<char> program, ReadOnlySpan<char> shaderVersion) {
		throw new NotImplementedException();
	}

	public override VertexShaderHandle CreateVertexShader(Stream program, ReadOnlySpan<char> shaderVersion) {
		throw new NotImplementedException();
	}

	public override void DestroyGeometryShader(GeometryShaderHandle shaderHandle) {
		throw new NotImplementedException();
	}

	public override void DestroyIndexBuffer(IIndexBuffer vertexBuffer) {
		throw new NotImplementedException();
	}

	public override void DestroyPixelShader(PixelShaderHandle shaderHandle) {
		throw new NotImplementedException();
	}

	public override void DestroyVertexBuffer(IVertexBuffer vertexBuffer) {
		throw new NotImplementedException();
	}

	public override void DestroyVertexShader(VertexShaderHandle shaderHandle) {
		throw new NotImplementedException();
	}

	public override IIndexBuffer GetDynamicIndexBuffer(int streamID, VertexFormat format, bool buffered = true) {
		throw new NotImplementedException();
	}

	public override IVertexBuffer GetDynamicVertexBuffer(int streamID, VertexFormat format, bool buffered = true) {
		throw new NotImplementedException();
	}
}
