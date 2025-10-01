using Source.Common.MaterialSystem;
using Source.Common.Mathematics;
using Source.Common.ShaderAPI;
using Source.Common.Utilities;

using System.Diagnostics;
using System.Numerics;
using System.Xml.Linq;

namespace Source.MaterialSystem;

public class MatRenderContext : IMatRenderContextInternal
{
	readonly MaterialSystem materials;
	readonly IShaderAPI shaderAPI;

	public MatRenderContext(MaterialSystem materials) {
		this.materials = materials;
		RenderTargetStack = new RefStack<RenderTargetStackElement>();
		MatrixStacks = new RefStack<MatrixStackItem>[(int)MaterialMatrixMode.Count];
		MatrixStacksDirtyStates = new bool[(int)MaterialMatrixMode.Count];
		for (int i = 0; i < MatrixStacks.Length; i++) {
			MatrixStacks[i] = new();
			ref MatrixStackItem item = ref MatrixStacks[i].Push();
			MatrixStacksDirtyStates[i] = true;
			item.Matrix = Matrix4x4.Identity;
		}
		RenderTargetStackElement initialElement = new() {
			DepthTexture = null,
			ViewX = 0,
			ViewY = 0,
			ViewW = -1,
			ViewH = -1
		};
		RenderTargetStack.Push(initialElement);
		shaderAPI = materials.ShaderAPI;
	}
	RefStack<RenderTargetStackElement> RenderTargetStack;
	RefStack<MatrixStackItem>[] MatrixStacks;
	bool[] MatrixStacksDirtyStates;
	MaterialMatrixMode matrixMode;
	ShaderViewport ActiveViewport = new(0, 0, 0, 0, 0, 1);
	public ref MatrixStackItem CurMatrixItem => ref MatrixStacks[(int)matrixMode].Top();
	public ref RenderTargetStackElement CurRenderTargetStack => ref RenderTargetStack.Top();

	public void BeginRender() {

	}

	public void ClearColor3ub(byte r, byte g, byte b) => shaderAPI.ClearColor3ub(r, g, b);
	public void ClearColor4ub(byte r, byte g, byte b, byte a) => shaderAPI.ClearColor4ub(r, g, b, a);

	public void ClearBuffers(bool clearColor, bool clearDepth, bool clearStencil = false) {
		int width, height;
		GetRenderTargetDimensions(out width, out height);
		shaderAPI.ClearBuffers(clearColor, clearDepth, clearStencil, width, height);
	}

	public void GetRenderTargetDimensions(out int width, out int height) {
		// todo
		shaderAPI.GetBackBufferDimensions(out width, out height);
	}

	public unsafe void DepthRange(double near, double far) {
		ActiveViewport.MinZ = (float)near;
		ActiveViewport.MaxZ = (float)far;
		fixed (ShaderViewport* pVp = &ActiveViewport)
			shaderAPI.SetViewports(new(pVp, 1));
	}

	public void EndRender() {

	}

	public void Flush(bool flushHardware = false) {
		shaderAPI.FlushBufferedPrimitives();
	}

	public void GetViewport(out int x, out int y, out int width, out int height) {
		Assert(RenderTargetStack.Count > 0);
		ref RenderTargetStackElement element = ref RenderTargetStack.Top();
		if (element.ViewW <= 0 || element.ViewH <= 0) {
			x = y = 0;
			ITexture? renderTarget = element.RenderTarget0;
			if (renderTarget == null) {
				shaderAPI.GetBackBufferDimensions(out width, out height);
			}
			else {
				width = renderTarget.GetActualWidth();
				height = renderTarget.GetActualHeight();
			}
		}
		else {
			x = element.ViewX;
			y = element.ViewY;
			width = element.ViewW;
			height = element.ViewH;
		}
	}

	public void LoadIdentity() {
		ref MatrixStackItem item = ref CurMatrixItem;
		item.Matrix = Matrix4x4.Identity;
		CurrentMatrixChanged();
	}

	private void MarkDirty() => MatrixStacksDirtyStates[(int)matrixMode] = true;

	public void MatrixMode(MaterialMatrixMode mode) {
		matrixMode = mode;
	}

	public void PushMatrix() {
		RefStack<MatrixStackItem> curStack = MatrixStacks[(int)matrixMode];
		curStack.Push(CurMatrixItem);
		CurrentMatrixChanged();
		shaderAPI.PushMatrix();
	}

	private void CurrentMatrixChanged() {
		MarkDirty();
	}

	public void Viewport(int x, int y, int width, int height) {
		Dbg.Assert(RenderTargetStack.Count > 0);

		RenderTargetStackElement newTOS = new();
		newTOS = CurRenderTargetStack; // copy
		newTOS.ViewX = x;
		newTOS.ViewY = y;
		newTOS.ViewW = width;
		newTOS.ViewH = height;
		RenderTargetStack.Pop();
		RenderTargetStack.Push(newTOS);
	}

	IMaterialInternal? currentMaterial;

	public void Bind(IMaterial iMaterial, object? proxyData) {
		if (iMaterial == null) {
			if (materials.errorMaterial == null)
				return;
			Warning("Programming error: MatRenderContext.Bind: NULL material\n");
			iMaterial = materials.errorMaterial;
		}
		else {
			// Proxy replacements?
		}

		IMaterialInternal material = (IMaterialInternal)iMaterial;
		material = material.GetRealTimeVersion(); // TODO: figure out how to do this.

		SyncMatrices();

		if (material == null) {
			Warning("Programming error: MatRenderContext.Bind NULL material\n");
			material = ((MaterialSystem)materials).errorMaterial;
		}

		if (GetCurrentMaterialInternal() != material) {
			if (!material.IsPrecached()) {
				material.Precache();
			}
			SetCurrentMaterialInternal(material);
		}

		shaderAPI.Bind(GetCurrentMaterialInternal());
	}

	private IMaterialInternal? GetCurrentMaterialInternal() {
		return currentMaterial;
	}
	private void SetCurrentMaterialInternal(IMaterialInternal? mat) {
		currentMaterial = mat;
	}

	public IMaterial? GetCurrentMaterial() {
		return currentMaterial;
	}

	public void PopMatrix() {
		shaderAPI.PopMatrix(); // We need to tell ShaderAPI *NOW* so it can flush primitives trigger matrix sync etc
							   // ^^ is NOT source behavior. But I think, for all intents and purposes, it will act as such (we'll see if I eat my words on that)
		RefStack<MatrixStackItem> curStack = MatrixStacks[(int)matrixMode];
		curStack.Pop();
		CurrentMatrixChanged();
	}

	public IShaderAPI GetShaderAPI() {
		return materials.ShaderAPI;
	}

	public bool OnDrawMesh(IMesh mesh, int firstIndex, int indexCount) {
		SyncMatrices();
		return true;
	}

	public IMesh GetDynamicMesh(bool buffered, IMesh? vertexOverride = null, IMesh? indexOverride = null, IMaterial? autoBind = null) {
		if (autoBind != null) {
			Bind(autoBind, null);
		}

		// For anything more than 1 bone, imply the last weight from the 1 - the sum of the others.
		int nCurrentBoneCount = shaderAPI.GetCurrentNumBones();
		Assert(nCurrentBoneCount <= 4);
		if (nCurrentBoneCount > 1) {
			--nCurrentBoneCount;
		}

		return shaderAPI.GetDynamicMesh(GetCurrentMaterialInternal()!, nCurrentBoneCount, buffered, vertexOverride, indexOverride);
	}

	bool FlashlightEnable;
	bool DirtyViewState;
	bool DirtyViewProjState;
	bool EnableClipping;

	public bool InFlashlightMode() {
		return FlashlightEnable;
	}

	public void BeginFrame() => shaderAPI.BeginFrame();
	public void EndFrame() => shaderAPI.EndFrame();

	public void MarkRenderDataUnused(bool v) {

	}

	public void SetFrameTime(double frameTime) {

	}

	public void SwapBuffers() {
		materials.ShaderDevice.Present();
	}

	public bool OnSetPrimitiveType(IMesh mesh, MaterialPrimitiveType type) {
		return true;
	}

	public static bool ShouldValidateMatrices() => false;
	public static bool AllowLazyMatrixSync() => true;

	public void ForceSyncMatrix(MaterialMatrixMode mode) {
		ref MatrixStackItem top = ref MatrixStacks[(int)mode].Top();
		if (MatrixStacksDirtyStates[(int)matrixMode]) {
			bool setMode = matrixMode != mode;
			if (setMode)
				shaderAPI.MatrixMode(mode);

			if (!top.Matrix.IsIdentity) {
				shaderAPI.LoadMatrix(in top.Matrix);
			}
			else {
				shaderAPI.LoadIdentity();
			}

			if (setMode)
				shaderAPI.MatrixMode(mode);

			MatrixStacksDirtyStates[(int)matrixMode] = false;
		}
	}

	public void SyncMatrices() {
		if (!ShouldValidateMatrices() && AllowLazyMatrixSync()) {
			for (int i = 0; i < (int)MaterialMatrixMode.Count; i++) {
				ref MatrixStackItem top = ref MatrixStacks[i].Top();
				if (MatrixStacksDirtyStates[i]) {
					shaderAPI.MatrixMode((MaterialMatrixMode)i);
					if (!top.Matrix.IsIdentity) {
						shaderAPI.LoadMatrix(in top.Matrix);
					}
					else {
						shaderAPI.LoadIdentity();
					}

					MatrixStacksDirtyStates[i] = false;
				}
			}
		}
	}

	public void SyncMatrix(MaterialMatrixMode mode) {
		if (!ShouldValidateMatrices() && AllowLazyMatrixSync())
			ForceSyncMatrix(mode);
	}

	public void Scale(float x, float y, float z) {
		Matrix4x4 mat = Matrix4x4.CreateScale(x, y, z);
		MultMatrixLocal(in mat);
	}


	private void MultMatrixLocal(in Matrix4x4 mat) {
		Matrix4x4 result = Matrix4x4.Multiply(CurMatrixItem.Matrix, mat);
		ref MatrixStackItem item = ref CurMatrixItem;
		item.Matrix = result;
		CurrentMatrixChanged();
	}

	public void Ortho(double left, double top, double right, double bottom, double near, double far) {
		ref Matrix4x4 item = ref CurMatrixItem.Matrix;
		Matrix4x4 matrix = MathLib.CreateOpenGLOrthoOffCenter((float)left, (float)right, (float)bottom, (float)top, (float)near, (float)far);
		item = Matrix4x4.Multiply(item, matrix);
		CurrentMatrixChanged();
	}

	private void CommitRenderTargetAndViewport() {
		Assert(RenderTargetStack.Count > 0);

		ref RenderTargetStackElement element = ref RenderTargetStack.Top();

		for (int rt = 0, size = element.Size; rt < size; rt++) {
			if (element[rt] == null) {
				shaderAPI.SetRenderTargetEx(rt);

				if (rt == 0) {
					if ((element.ViewW < 0) || (element.ViewH < 0)) {
						ActiveViewport.TopLeftX = 0;
						ActiveViewport.TopLeftY = 0;
						shaderAPI.GetBackBufferDimensions(out ActiveViewport.Width, out ActiveViewport.Height);
						shaderAPI.SetViewports(new Span<ShaderViewport>(ref ActiveViewport));
					}
					else {
						ActiveViewport.TopLeftX = element.ViewX;
						ActiveViewport.TopLeftY = element.ViewY;
						ActiveViewport.Width = element.ViewW;
						ActiveViewport.Height = element.ViewH;
						shaderAPI.SetViewports(new Span<ShaderViewport>(ref ActiveViewport));
					}
				}
			}
			else {
				ITextureInternal texInt = (ITextureInternal)element[rt]!;
				texInt.SetRenderTarget(rt, element.DepthTexture);

				if (rt == 0) {
					if (element[rt]!.GetImageFormat() == Common.Bitmap.ImageFormat.RGBA16161616F)
						shaderAPI.EnableLinearColorSpaceFrameBuffer(true);
					else
						shaderAPI.EnableLinearColorSpaceFrameBuffer(false);

					if ((element.ViewW < 0) || (element.ViewH < 0)) {
						ActiveViewport.TopLeftX = 0;
						ActiveViewport.TopLeftY = 0;
						ActiveViewport.Width = element[rt]!.GetActualWidth();
						ActiveViewport.Height = element[rt]!.GetActualHeight();
						shaderAPI.SetViewports(new Span<ShaderViewport>(ref ActiveViewport));
					}
					else {
						ActiveViewport.TopLeftX = element.ViewX;
						ActiveViewport.TopLeftY = element.ViewY;
						ActiveViewport.Width = element.ViewW;
						ActiveViewport.Height = element.ViewH;
						shaderAPI.SetViewports(new Span<ShaderViewport>(ref ActiveViewport));
					}
				}
			}
		}
	}

	public void PushRenderTargetAndViewport(ITexture? thisTexture, int x, int y, int w, int h) {
		RenderTargetStackElement element = new(thisTexture, x, y, w, h);
		RenderTargetStack.Push(element);
		CommitRenderTargetAndViewport();
	}
	public void PushRenderTargetAndViewport(ITexture? thisTexture) {
		RenderTargetStackElement element = new(thisTexture, 0, 0, -1, -1);
		RenderTargetStack.Push(element);
		CommitRenderTargetAndViewport();
	}

	public void PopRenderTargetAndViewport() {
		if (RenderTargetStack.Count == 0) {
			AssertMsg(false, "MatRenderContext.PopRenderTargetAndViewport: Stack is empty!\n");
			return;
		}

		Flush();

		RenderTargetStack.Pop();
		CommitRenderTargetAndViewport();
	}

	public void GetWindowSize(out int w, out int h) {
		shaderAPI.GetBackBufferDimensions(out w, out h);
	}

	public ITexture? GetRenderTarget() {
		if (RenderTargetStack.Count > 0)
			return RenderTargetStack.Top().RenderTarget0;
		else
			return null;
	}

	public IMesh CreateStaticMesh(VertexFormat format, ReadOnlySpan<char> textureGroup, IMaterial material) {
		return materials.ShaderDevice.CreateStaticMesh(format, textureGroup, material);
	}

	public int GetMaxVerticesToRender(IMaterial material) => materials.ShaderAPI.GetMaxVerticesToRender(material);
	public int GetMaxIndicesToRender(IMaterial material) => materials.ShaderAPI.GetMaxIndicesToRender(material);
}