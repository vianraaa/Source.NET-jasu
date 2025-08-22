using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;

/// <summary>
/// Various render util functions.
/// </summary>
public class RenderUtils(IMaterialSystem materials)
{
	public void DrawScreenSpaceRectangle(IMaterial material, int destX, int destY, int width, int height,
										 float srcTextureX0, float srcTextureY0, float srcTextureX1, float srcTextureY1,
										 int srcTextureWidth, int srcTextureHeight, object? clientRenderable, int xDice, int yDice,
										 float depth
										) {
		using MatRenderContextPtr renderContext = new(materials);

		if (width <= 0 || height <= 0)
			return;

		renderContext.MatrixMode(MaterialMatrixMode.View);
		renderContext.PushMatrix();
		renderContext.LoadIdentity();

		renderContext.MatrixMode(MaterialMatrixMode.Projection);
		renderContext.PushMatrix();
		renderContext.LoadIdentity();

		renderContext.Bind(material, clientRenderable);

		int xSegments = Math.Max(xDice, 1);
		int ySegments = Math.Max(yDice, 1);

		renderContext.MatrixMode(MaterialMatrixMode.View);
		renderContext.PopMatrix();

		renderContext.MatrixMode(MaterialMatrixMode.Projection);
		renderContext.PopMatrix();
	}
}
