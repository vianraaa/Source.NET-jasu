
using CommunityToolkit.HighPerformance;

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
	Host Host,
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

	public Vector3 CurrentViewOrigin = new(0, 0, 0);
	public Vector3 CurrentViewForward = new(1, 0, 0);
	public Vector3 CurrentViewRight = new(0, -1, 0);
	public Vector3 CurrentViewUp = new(0, 0, 1);

	public Vector3 MainViewOrigin = new(0, 0, 0);
	public Vector3 MainViewForward = new(1, 0, 0);
	public Vector3 MainViewRight = new(0, -1, 0);
	public Vector3 MainViewUp = new(0, 0, 1);

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

	public void SetMainView(in Vector3 origin, in QAngle angles) {
		MainViewOrigin = origin;
		angles.Vectors(out MainViewForward, out MainViewRight, out MainViewUp);
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

	private void ClearView(ViewSetup topView, ClearFlags flags, ITexture? color, ITexture? depth = null) {

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

	private void ResetLightStyles() { }
	private void DecalInit() { }
	private void LoadSkys() { }
	private void InitStudio() { }
	private void LoadWorldGeometry() {
		MaterialSystem.DestroySortInfo();
		MaterialSystem.RegisterLightmapSurfaces();
		MaterialSystem.CreateSortInfo();
	}
	private void Surface_LevelInit() { }
	private void Areaportal_LevelInit() { }


	internal void DrawSceneBegin() {

	}

	internal void DrawSceneEnd() {

	}

	internal void ViewSetupVisEx(bool novis, ReadOnlySpan<Vector3> origins, out uint returnFlags) {
		ModelLoader.Map_VisSetup(host_state.WorldModel, origins, novis, out returnFlags);
	}

	internal void DrawWorld(DrawWorldListFlags flags, float waterZAdjust) {
		using MatRenderContextPtr renderContext = new(materials);
		Span<MatSysInterface.MeshList> meshLists = MaterialSystem.Meshes.AsSpan();
		for (int i = 0; i < meshLists.Length; i++) {
			ref MatSysInterface.MeshList meshList = ref meshLists[i];
			meshList.Mesh.Draw();
		}
	}

	float Near;
	float Far;

	internal void Push3DView(in ViewSetup view, ClearFlags clearFlags, ITexture? rtColor, Frustum frustum, ITexture? rtDepth) {
		ref ViewStack writeStack = ref ViewStack.Push();
		writeStack.View = view;
		writeStack.Is2DView = false;
		writeStack.NoDraw = (clearFlags & ClearFlags.NoDraw) != 0;

		ref ViewSetup topView = ref writeStack.View;

		if (topView.AspectRatio == 0.0f)
			topView.AspectRatio = (topView.Height != 0) ? ((float)topView.Width / (float)topView.Height) : 1.0f;

		ref ViewStack viewStack = ref ViewStack.Top();
		topView.AspectRatio = ComputeViewMatrices(ref viewStack.MatrixView, ref viewStack.MatrixProjection, ref viewStack.MatrixWorldToScreen, in topView);

		Near = topView.ZNear;
		Far = topView.ZFar;

		ExtractMatrices();

		if (!writeStack.NoDraw) {
			using MatRenderContextPtr renderContext = new(materials);

			if (rtColor == null)
				rtColor = renderContext.GetRenderTarget();

			renderContext.PushRenderTargetAndViewport(rtColor, rtDepth, topView.X, topView.Y, topView.Width, topView.Height);

			ClearView(topView, clearFlags, rtColor, rtDepth);

			renderContext.DepthRange(0, 1);
			renderContext.MatrixMode(MaterialMatrixMode.Projection);
			renderContext.PushMatrix();
			renderContext.LoadMatrix(MatrixProjection);
			renderContext.MatrixMode(MaterialMatrixMode.View);
			renderContext.PushMatrix();
			renderContext.LoadMatrix(MatrixView);
			renderContext.MatrixMode(MaterialMatrixMode.Model);
			renderContext.PushMatrix();

			OnViewActive(frustum);
		}
	}

	private float ComputeViewMatrices(ref Matrix4x4 worldToView, ref Matrix4x4 viewToProjection, ref Matrix4x4 worldToProjection, in ViewSetup viewSetup) {
		float aspectRatio = viewSetup.AspectRatio;
		if (aspectRatio == 0.0f)
			aspectRatio = (viewSetup.Height != 0) ? ((float)viewSetup.Height / (float)viewSetup.Width) : 1.0f;

		ComputeViewMatrix(ref worldToView, viewSetup.Origin, viewSetup.Angles + new QAngle(0, 0, -45));

		float fovX = MathLib.DEG2RAD(viewSetup.FOV);

		if (viewSetup.Ortho) {
			throw new NotImplementedException();
		}
		else if (viewSetup.OffCenter) {
			throw new NotImplementedException();
		}
		else if (viewSetup.ViewToProjectionOverride) {
			throw new NotImplementedException();
		}
		else
			viewToProjection = Matrix4x4.CreatePerspectiveFieldOfView(fovX, aspectRatio, viewSetup.ZNear, viewSetup.ZFar);

		worldToProjection = Matrix4x4.Multiply(viewToProjection, worldToView);

		return aspectRatio;
	}

	private static Matrix4x4 baseRotation;
	private static bool didInit = false;
	private void ComputeViewMatrix(ref Matrix4x4 worldToView, in Vector3 origin, in QAngle angles) {
		if (!didInit) {
			baseRotation = Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, MathLib.DEG2RAD(-90));

			didInit = true;
		}

		worldToView = baseRotation;

		worldToView *= Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, MathLib.DEG2RAD(-angles.Z)); // -angles[2]
		worldToView *= Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, MathLib.DEG2RAD(-angles.X)); // -angles[0]
		worldToView *= Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, MathLib.DEG2RAD(-angles.Y)); // -angles[1]

		worldToView *= Matrix4x4.CreateTranslation(-origin);
	}
}
