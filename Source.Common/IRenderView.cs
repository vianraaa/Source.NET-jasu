using Source.Common.MaterialSystem;
using Source.Common.Mathematics;
using Source.Engine;

namespace Source.Common;

public interface IRenderView
{
	void VGui_Paint(PaintMode mode);
	void Push2DView(ViewSetup view, ClearFlags flags, ITexture? renderTarget, Frustum frustumPlanes);
	void PopView(Frustum frustumPlanes);
}
