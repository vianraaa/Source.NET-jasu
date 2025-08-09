using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static OpenGL.Gl46;

namespace Source.ShaderAPI.Gl46;

public unsafe class ShaderDeviceGl46 : ShaderDeviceBase
{

	public override GeometryShaderHandle CreateGeometryShader(ReadOnlySpan<char> program, ReadOnlySpan<char> shaderVersion) {
		var shader = glCreateShader(GL_GEOMETRY_SHADER);
		glShaderSource(shader, program);

		uint prog = glCreateProgram();
		glProgramParameteri(prog, GL_PROGRAM_SEPARABLE, GL_TRUE);
		glAttachShader(prog, shader);
		glLinkProgram(prog);

		glDeleteShader(shader);

		return new((nint)prog);
	}

	public override GeometryShaderHandle CreateGeometryShader(Stream program, ReadOnlySpan<char> shaderVersion) {
		using StreamReader reader = new(program);
		return CreateGeometryShader(reader.ReadToEnd(), shaderVersion);
	}


	public override IIndexBuffer CreateIndexBuffer(ShaderBufferType type, MaterialIndexFormat format, int count) {
		IndexBufferGl46 newIndexBuffer = new();
		return newIndexBuffer;
	}

	public override PixelShaderHandle CreatePixelShader(ReadOnlySpan<char> program, ReadOnlySpan<char> shaderVersion) {
		var shader = glCreateShader(GL_FRAGMENT_SHADER);
		glShaderSource(shader, program);

		uint prog = glCreateProgram();
		glProgramParameteri(prog, GL_PROGRAM_SEPARABLE, GL_TRUE);
		glAttachShader(prog, shader);
		glLinkProgram(prog);

		glDeleteShader(shader);

		return new((nint)prog);
	}

	public override PixelShaderHandle CreatePixelShader(Stream program, ReadOnlySpan<char> shaderVersion) {
		using StreamReader reader = new(program);
		return CreatePixelShader(reader.ReadToEnd(), shaderVersion);
	}


	public override IVertexBuffer CreateVertexBuffer(ShaderBufferType type, VertexFormat format, int count) {
		VertexBufferGl46 newVertexBuffer = new();
		return newVertexBuffer;
	}

	public override VertexShaderHandle CreateVertexShader(ReadOnlySpan<char> program, ReadOnlySpan<char> shaderVersion) {
		var shader = glCreateShader(GL_VERTEX_SHADER);
		glShaderSource(shader, program);

		uint prog = glCreateProgram();
		glProgramParameteri(prog, GL_PROGRAM_SEPARABLE, GL_TRUE);
		glAttachShader(prog, shader);
		glLinkProgram(prog);

		glDeleteShader(shader);

		return new((nint)prog);
	}

	public override VertexShaderHandle CreateVertexShader(Stream program, ReadOnlySpan<char> shaderVersion) {
		using StreamReader reader = new(program);
		return CreateVertexShader(reader.ReadToEnd(), shaderVersion);
	}

	public override void DestroyGeometryShader(GeometryShaderHandle shaderHandle) {
		glDeleteProgram((uint)shaderHandle.Handle);
	}

	public override void DestroyIndexBuffer(IIndexBuffer indexBuffer) {

	}

	public override void DestroyPixelShader(PixelShaderHandle shaderHandle) {
		glDeleteProgram((uint)shaderHandle.Handle);
	}

	public override void DestroyVertexBuffer(IVertexBuffer vertexBuffer) {

	}

	public override void DestroyVertexShader(VertexShaderHandle shaderHandle) {
		glDeleteProgram((uint)shaderHandle.Handle);
	}

	public override IIndexBuffer GetDynamicIndexBuffer(int streamID, VertexFormat format, bool buffered = true) {
		throw new NotImplementedException();
	}

	public override IVertexBuffer GetDynamicVertexBuffer(int streamID, VertexFormat format, bool buffered = true) {
		throw new NotImplementedException();
	}
}
