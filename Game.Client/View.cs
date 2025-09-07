using Source.Common;
using Source.Common.Client;
using Source.Common.Commands;
using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.Common.Mathematics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client;

public class ViewRender : IViewRender
{
	public ViewSetup? CurrentView;
	bool ForceNoVis;
	DrawFlags BaseDrawFlags;
	Frustum Frustum;

	public int BuildWorldListsNumber() {
		throw new NotImplementedException();
	}

	public void DisableVis() {
		throw new NotImplementedException();
	}

	public void DriftPitch() {
		throw new NotImplementedException();
	}

	public void FreezeFrame(float flFreezeTime) {
		throw new NotImplementedException();
	}

	public DrawFlags GetDrawFlags() {
		throw new NotImplementedException();
	}

	public ref VPlane GetFrustum() {
		throw new NotImplementedException();
	}

	public ViewSetup GetPlayerViewSetup() {
		throw new NotImplementedException();
	}

	public void GetScreenFadeDistances(out float min, out float max) {
		throw new NotImplementedException();
	}

	public IMaterial? GetScreenOverlayMaterial() {
		throw new NotImplementedException();
	}

	public ViewSetup GetViewSetup() {
		throw new NotImplementedException();
	}

	public void GetWaterLODParams(ref float cheapWaterStartDistance, ref float cheapWaterEndDistance) {
		throw new NotImplementedException();
	}

	public float GetZFar() {
		throw new NotImplementedException();
	}

	public float GetZNear() {
		throw new NotImplementedException();
	}

	public void Init() {
		m_bDrawOverlay = false;

		m_pDrawEntities = cvar->FindVar("r_drawentities");
		m_pDrawBrushModels = cvar->FindVar("r_drawbrushmodels");

		beams->InitBeams();
		tempents->Init();

		m_TranslucentSingleColor.Init("debug/debugtranslucentsinglecolor", TEXTURE_GROUP_OTHER);
		m_ModulateSingleColor.Init("engine/modulatesinglecolor", TEXTURE_GROUP_OTHER);

		extern CMaterialReference g_material_WriteZ;
		g_material_WriteZ.Init("engine/writez", TEXTURE_GROUP_OTHER);

		// FIXME:  
		QAngle angles;
		engine->GetViewAngles(angles);
		AngleVectors(angles, &m_vecLastFacing);
	}

	public void LevelInit() {
		throw new NotImplementedException();
	}

	public void LevelShutdown() {
		throw new NotImplementedException();
	}

	public void OnRenderStart() {
		throw new NotImplementedException();
	}

	public void QueueOverlayRenderView(ViewSetup view, ClearFlags clearFlags, DrawFlags whatToDraw) {
		throw new NotImplementedException();
	}

	public void Render(ViewRects rect) {
		throw new NotImplementedException();
	}

	public void RenderView(ViewSetup view, ClearFlags clearFlags, DrawFlags whatToDraw) {
		throw new NotImplementedException();
	}

	public void SetCheapWaterEndDistance(float cheapWaterEndDistance) {
		throw new NotImplementedException();
	}

	public void SetCheapWaterStartDistance(float cheapWaterStartDistance) {
		throw new NotImplementedException();
	}

	public void SetScreenOverlayMaterial(IMaterial? pMaterial) {
		throw new NotImplementedException();
	}

	public bool ShouldDrawBrushModels() {
		throw new NotImplementedException();
	}

	public void Shutdown() {
		throw new NotImplementedException();
	}

	public void StartPitchDrift() {
		throw new NotImplementedException();
	}

	public void StopPitchDrift() {
		throw new NotImplementedException();
	}

	public bool UpdateShadowDepthTexture(ITexture? pRenderTarget, ITexture? pDepthTexture, ViewSetup shadowView) {
		throw new NotImplementedException();
	}

	public void WriteSaveGameScreenshot(ReadOnlySpan<char> pFilename) {
		throw new NotImplementedException();
	}

	public void WriteSaveGameScreenshotOfSize(ReadOnlySpan<char> pFilename, int width, int height, bool bCreatePowerOf2Padded = false, bool bWriteVTF = false) {
		throw new NotImplementedException();
	}
}