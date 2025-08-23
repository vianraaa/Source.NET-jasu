using Source.Common.MaterialSystem;
using Source.Common.Mathematics;

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

		using MeshBuilder meshBuilder = new();
		IMesh mesh = renderContext.GetDynamicMesh(true);
		meshBuilder.Begin(mesh, MaterialPrimitiveType.Quads, xSegments * ySegments);
		{
			renderContext.GetRenderTargetDimensions(out int screenWidth, out int screenHeight);
			float flOffset = 0.5f;

			float flLeftX = destX - flOffset;
			float flRightX = destX + width - flOffset;

			float flTopY = destY - flOffset;
			float flBottomY = destY + height - flOffset;

			float flSubrectWidth = srcTextureX1 - srcTextureX0;
			float flSubrectHeight = srcTextureY1 - srcTextureY0;

			float texelsPerPixelX = (width > 1) ? flSubrectWidth / (width - 1) : 0.0f;
			float texelsPerPixelY = (height > 1) ? flSubrectHeight / (height - 1) : 0.0f;

			float flLeftU = srcTextureX0 + 0.5f - (0.5f * texelsPerPixelX);
			float flRightU = srcTextureX1 + 0.5f + (0.5f * texelsPerPixelX);
			float flTopV = srcTextureY0 + 0.5f - (0.5f * texelsPerPixelY);
			float flBottomV = srcTextureY1 + 0.5f + (0.5f * texelsPerPixelY);

			float flOOTexWidth = 1.0f / srcTextureWidth;
			float flOOTexHeight = 1.0f / srcTextureHeight;
			flLeftU *= flOOTexWidth;
			flRightU *= flOOTexWidth;
			flTopV *= flOOTexHeight;
			flBottomV *= flOOTexHeight;

			int vx, vy, vw, vh;
			renderContext.GetViewport(out vx, out vy, out vw, out vh);

			// map from screen pixel coords to -1..1
			flRightX = MathLib.Lerp(-1, 1, 0, vw, flRightX);
			flLeftX = MathLib.Lerp(-1, 1, 0, vw, flLeftX);
			flTopY = MathLib.Lerp(1, -1, 0, vh, flTopY);
			flBottomY = MathLib.Lerp(1, -1, 0, vh, flBottomY);

			if ((xSegments > 1) || (ySegments > 1)) {
				// Screen height and width of a subrect
				float flWidth = (flRightX - flLeftX) / (float)xSegments;
				float flHeight = (flTopY - flBottomY) / (float)ySegments;

				// UV height and width of a subrect
				float flUWidth = (flRightU - flLeftU) / (float)xSegments;
				float flVHeight = (flBottomV - flTopV) / (float)ySegments;

				for (int x = 0; x < xSegments; x++) {
					for (int y = 0; y < ySegments; y++) {
						// Top left
						meshBuilder.Position3f(flLeftX + (float)x * flWidth, flTopY - (float)y * flHeight, depth);
						meshBuilder.Normal3f(0.0f, 0.0f, 1.0f);
						meshBuilder.TexCoord2f(0, flLeftU + (float)x * flUWidth, flTopV + (float)y * flVHeight);
						meshBuilder.TangentS3f(0.0f, 1.0f, 0.0f);
						meshBuilder.TangentT3f(1.0f, 0.0f, 0.0f);
						meshBuilder.AdvanceVertex();

						// Top right (x+1)
						meshBuilder.Position3f(flLeftX + (float)(x + 1) * flWidth, flTopY - (float)y * flHeight, depth);
						meshBuilder.Normal3f(0.0f, 0.0f, 1.0f);
						meshBuilder.TexCoord2f(0, flLeftU + (float)(x + 1) * flUWidth, flTopV + (float)y * flVHeight);
						meshBuilder.TangentS3f(0.0f, 1.0f, 0.0f);
						meshBuilder.TangentT3f(1.0f, 0.0f, 0.0f);
						meshBuilder.AdvanceVertex();

						// Bottom right (x+1), (y+1)
						meshBuilder.Position3f(flLeftX + (float)(x + 1) * flWidth, flTopY - (float)(y + 1) * flHeight, depth);
						meshBuilder.Normal3f(0.0f, 0.0f, 1.0f);
						meshBuilder.TexCoord2f(0, flLeftU + (float)(x + 1) * flUWidth, flTopV + (float)(y + 1) * flVHeight);
						meshBuilder.TangentS3f(0.0f, 1.0f, 0.0f);
						meshBuilder.TangentT3f(1.0f, 0.0f, 0.0f);
						meshBuilder.AdvanceVertex();

						// Bottom left (y+1)
						meshBuilder.Position3f(flLeftX + (float)x * flWidth, flTopY - (float)(y + 1) * flHeight, depth);
						meshBuilder.Normal3f(0.0f, 0.0f, 1.0f);
						meshBuilder.TexCoord2f(0, flLeftU + (float)x * flUWidth, flTopV + (float)(y + 1) * flVHeight);
						meshBuilder.TangentS3f(0.0f, 1.0f, 0.0f);
						meshBuilder.TangentT3f(1.0f, 0.0f, 0.0f);
						meshBuilder.AdvanceVertex();
					}
				}
			}
			else // just one quad
			{
				for (int corner = 0; corner < 4; corner++) {
					bool bLeft = (corner == 0) || (corner == 3);
					meshBuilder.Position3f((bLeft) ? flLeftX : flRightX, (corner & 2) != 0 ? flBottomY : flTopY, depth);
					meshBuilder.Normal3f(0.0f, 0.0f, 1.0f);
					meshBuilder.TexCoord2f(0, (bLeft) ? flLeftU : flRightU, (corner & 2) != 0 ? flBottomV : flTopV);
					meshBuilder.TangentS3f(0.0f, 1.0f, 0.0f);
					meshBuilder.TangentT3f(1.0f, 0.0f, 0.0f);
					meshBuilder.AdvanceVertex();
				}
			}

		}
		meshBuilder.End();
		mesh.Draw();

		renderContext.MatrixMode(MaterialMatrixMode.View);
		renderContext.PopMatrix();

		renderContext.MatrixMode(MaterialMatrixMode.Projection);
		renderContext.PopMatrix();
	}
}
