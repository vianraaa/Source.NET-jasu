using Source.Common.Mathematics;
using Source.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Source.Common.Client;

namespace Game.Client;

public class Base3dView
{
	protected Frustum frustum;
	protected ViewRender mainView;
	public Base3dView(ViewRender mainView) {
		this.mainView = mainView;
		frustum = mainView.GetFrustum();
	}

	protected ViewSetup setup;
	public Frustum GetFrustrum() => frustum;
	public virtual DrawFlags GetDrawFlags() => 0;
}

public class Rendering3dView : Base3dView
{
	DrawFlags DrawFlags;
	ClearFlags ClearFlags;
	IWorldRenderList? WorldRenderList = null;
	
	public Rendering3dView(ViewRender mainView) : base(mainView) {

	}
	public virtual void Setup(in ViewSetup setup) {

	}
	public override DrawFlags GetDrawFlags() {
		return DrawFlags;
	}
	public virtual void Draw() {

	}

	protected void EnableWorldFog() => throw new NotImplementedException();
	protected void SetupRenderablesList(int viewID) => throw new NotImplementedException();
	protected void DrawWorld(float waterZAdjust) => throw new NotImplementedException();
}

public class SkyboxView : Rendering3dView
{
	public SkyboxView(ViewRender mainView) : base(mainView) {

	}
}
