using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.ShaderAPI;
public interface IShaderAPI : IShaderDynamicAPI
{
	void SetViewports(ReadOnlySpan<ShaderViewport> viewports);
	void GetViewports(Span<ShaderViewport> viewports);

	bool IsAlphaTested(StateSnapshot_t snapshot);
	bool IsTranslucent(StateSnapshot_t snapshot);
	bool IsDepthWriteEnabled(StateSnapshot_t id);
	bool UsesVertexAndPixelShaders(StateSnapshot_t id);
	StateSnapshot_t TakeSnapshot();
	void DrawMesh(IMesh mesh);
	void Bind(IMaterial? material);
	void FlushBufferedPrimitives();
	bool OnDeviceInit();
	void InitRenderState();
	void ClearBuffers(bool bClearColor, bool bClearDepth, bool bClearStencil, int renderTargetWidth, int renderTargetHeight);
	void ClearColor3ub(byte r, byte g, byte b);
	void ClearColor4ub(byte r, byte g, byte b, byte a);
	void BindVertexShader(in VertexShaderHandle vertexShader);
	void BindGeometryShader(in GeometryShaderHandle geometryShader);
	void BindPixelShader(in PixelShaderHandle pixelShader);
	void GetBackBufferDimensions(out int width, out int height);
	void BeginFrame();
	void EndFrame();
}