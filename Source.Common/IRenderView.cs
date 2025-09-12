using Source.Common.MaterialSystem;
using Source.Common.Mathematics;
using Source.Engine;

using System.Numerics;

namespace Source.Common;

public interface IRenderView
{
	void VGui_Paint(PaintMode mode);
	void Push2DView(ViewSetup view, ClearFlags flags, ITexture? renderTarget, Frustum frustumPlanes);
	void PopView(Frustum frustumPlanes);
	void SetMainView(in Vector3 origin, in QAngle angles);
}
