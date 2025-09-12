
using Source.Common;
using Source.Common.Client;
using Source.Common.Commands;
using Source.Common.MaterialSystem;
using Source.Common.Mathematics;
using Source.Common.Utilities;

using System.Numerics;

namespace Source.Engine;

public struct ViewStack
{
	public ViewSetup View;
	public Matrix4x4 MatrixView;
	public Matrix4x4 MatrixProjection;
	public Matrix4x4 MatrixWorldToScreen;
	public bool Is2DView;
	public bool NoDraw;
}

public class Render(
	CommonHostState host_state,
	IMaterialSystem materials,
	IModelLoader modelloader,
	MatSysInterface MaterialSystem
	)
{
	int framecount = 1;
	RefStack<ViewStack> ViewStack = [];
	Matrix4x4 MatrixView;
	Matrix4x4 MatrixProjection;
	Matrix4x4 MatrixWorldToScreen;

	float FOV;
	float Framerate;
	float ZNear;
	float ZFar;

	Vector3 CurrentViewOrigin = new(0, 0, 0);
	Vector3 CurrentViewForward = new(1, 0, 0);
	Vector3 CurrentViewRight = new(0, -1, 0);
	Vector3 CurrentViewUp = new(0, 0, 1);

	Vector3 MainViewOrigin = new(0, 0, 0);
	Vector3 MainViewForward = new(1, 0, 0);
	Vector3 MainViewRight = new(0, -1, 0);
	Vector3 MainViewUp = new(0, 0, 1);

	bool CanAccessCurrentView;

	internal void FrameBegin() {

		framecount++;
	}

	internal void FrameEnd() {

	}

	internal void PopView(Frustum frustumPlanes) {
		if (!ViewStack.Top().NoDraw) {
			using MatRenderContextPtr renderContext = new(materials);

			renderContext.MatrixMode(MaterialMatrixMode.Projection);
			renderContext.PopMatrix();

			renderContext.MatrixMode(MaterialMatrixMode.View);
			renderContext.PopMatrix();

			renderContext.MatrixMode(MaterialMatrixMode.Model);
			renderContext.PopMatrix();

			renderContext.PopRenderTargetAndViewport();
		}

		bool reset = (ViewStack.Count() > 1) ? true : false;
		ViewStack.Pop();

		if (reset) {
			if (!ViewStack.Top().Is2DView) {
				ExtractMatrices();
				OnViewActive(frustumPlanes);
			}
		}
	}

	private void ExtractMatrices() {
		MatrixView = ViewStack.Top().MatrixView;
		MatrixProjection = ViewStack.Top().MatrixProjection;
		MatrixWorldToScreen = ViewStack.Top().MatrixWorldToScreen;
	}

	private ref ViewSetup CurrentView() => ref ViewStack.Top().View;

	private void OnViewActive(Frustum frustumPlanes) {
		ref ViewSetup view = ref CurrentView();

		FOV = MathLib.CalcFovY(view.FOV, view.AspectRatio);

		CurrentViewOrigin = view.Origin;
		view.Angles.Vectors(out CurrentViewForward, out CurrentViewRight, out CurrentViewUp);
		CanAccessCurrentView = true;

		/*if (view.Ortho) {
			OrthoExtractFrustumPlanes(frustumPlanes);
		}
		else {
			ExtractFrustumPlanes(frustumPlanes);
		}*/

		// OcclusionSystem.SetView(view.Origin, view.FOV, MatrixView, MatrixProjection, frustumPlanes[FrustumPlane.NearZ]);

		if (!ViewStack.Top().NoDraw) {
			// R_SceneBegin();
		}
	}

	internal void Push2DView(in ViewSetup view, ClearFlags flags, ITexture? renderTarget, Frustum frustumPlanes) {
		ref ViewStack viewStack = ref ViewStack.Push();
		viewStack.View = view;
		viewStack.Is2DView = true;
		viewStack.NoDraw = (flags & ClearFlags.NoDraw) != 0;
		viewStack.MatrixView = MatrixView;
		viewStack.MatrixProjection = MatrixProjection;
		viewStack.MatrixWorldToScreen = MatrixWorldToScreen;

		ref ViewSetup topView = ref viewStack.View;
		using MatRenderContextPtr renderContext = new(materials);
		renderContext.PushRenderTargetAndViewport(renderTarget, topView.X, topView.Y, topView.Width, topView.Height);
		ClearView(topView, flags, renderTarget);

		renderContext.MatrixMode(MaterialMatrixMode.Projection);
		renderContext.PushMatrix();
		renderContext.LoadIdentity();
		renderContext.Scale(1, -1, 1);
		renderContext.Ortho(0, 0, topView.Width, topView.Height, -99999, 99999);

		renderContext.MatrixMode(MaterialMatrixMode.View);
		renderContext.PushMatrix();
		renderContext.LoadIdentity();

		renderContext.MatrixMode(MaterialMatrixMode.Model);
		renderContext.PushMatrix();
		renderContext.LoadIdentity();
	}

	private void ClearView(ViewSetup topView, ClearFlags flags, ITexture? renderTarget) {

	}

	public int FrameCount = 1;

	public void LevelInit() {
		ConDMsg("Initializing renderer...\n");

		FrameCount = 1;
		ResetLightStyles();
		DecalInit();
		LoadSkys();
		InitStudio();

		LoadWorldGeometry();

		Surface_LevelInit();
		Areaportal_LevelInit();
	}

	private void ResetLightStyles() {}
	private void DecalInit() { }
	private void LoadSkys() { }
	private void InitStudio() { }
	private void LoadWorldGeometry() {
		MaterialSystem.WorldStaticMeshCreate();
	}
	private void Surface_LevelInit() { }
	private void Areaportal_LevelInit() { }
}
