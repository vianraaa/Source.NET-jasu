using Source.Common.Bitmap;
using Source.Common.Launcher;
using Source.Common.MaterialSystem;

namespace Source.Common.ShaderAPI;
public enum CreateTextureFlags
{
	Cubemap = 0x0001,
	RenderTarget = 0x0002,
	Managed = 0x0004,
	DepthBuffer = 0x0008,
	Dynamic = 0x0010,
	AutoMipmap = 0x0020,
	VertexTexture = 0x0040,
	SysMem = 0x0200, 
	UnfilterableOK = 0x1000,
	SRGB = 0x4000, 
}
public interface IShaderAPI : IShaderDynamicAPI
{
	void SetViewports(ReadOnlySpan<ShaderViewport> viewports);
	void GetViewports(Span<ShaderViewport> viewports);

	void PreInit(IShaderUtil shaderUtil, IServiceProvider services);
	void DrawMesh(IMesh mesh);
	void Bind(IMaterial? material);
	void FlushBufferedPrimitives();
	bool OnDeviceInit();
	void InitRenderState();
	void ClearBuffers(bool bClearColor, bool bClearDepth, bool bClearStencil, int renderTargetWidth, int renderTargetHeight);
	void ClearColor3ub(byte r, byte g, byte b);
	void ClearColor4ub(byte r, byte g, byte b, byte a);
	void GetBackBufferDimensions(out int width, out int height);
	ImageFormat GetBackBufferFormat();
	void BeginFrame();
	void EndFrame();
	int GetCurrentDynamicVBSize();
	void TexSubImage2D(int mip, int face, int x, int y, int z, int width, int height, ImageFormat srcFormat, int srcStride, Span<byte> imageData);
	bool DoRenderTargetsNeedSeparateDepthBuffer();
	void EnableLinearColorSpaceFrameBuffer(bool v);
	void SetRenderTargetEx(int rt, ShaderAPITextureHandle_t colorTextureHandle = (ShaderAPITextureHandle_t)ShaderRenderTarget.Backbuffer, ShaderAPITextureHandle_t depthTextureHandle = (ShaderAPITextureHandle_t)ShaderRenderTarget.Depthbuffer);
	void InvalidateDelayedShaderConstraints();
	void SetSkinningMatrices();
	void ShadeMode(ShadeMode flat);
	void RenderPass();
	bool SetMode(IWindow window, in ShaderDeviceInfo info);
}
