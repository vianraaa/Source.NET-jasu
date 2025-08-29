namespace Source.Common.MaterialSystem.TextureRegenerators;
public class CheckerboardTexture(int checkerSize, Color color1, Color color2) : ITextureRegenerator
{
	public void RegenerateTextureBits(ITexture texture, IVTFTexture vtfTexture, in System.Drawing.Rectangle rect) {
		for (int iFrame = 0; iFrame < vtfTexture.FrameCount(); ++iFrame) {
			for (int iFace = 0; iFace < vtfTexture.FaceCount(); ++iFace) {
				int nWidth = vtfTexture.Width();
				int nHeight = vtfTexture.Height();
				int nDepth = vtfTexture.Depth();
				for (int z = 0; z < nDepth; ++z) {
					using PixelWriter pixelWriter = new();
					pixelWriter.SetPixelMemory(vtfTexture.Format(), vtfTexture.ImageData(iFrame, iFace, 0, 0, 0, z), vtfTexture.RowSizeInBytes(0));

					for (int y = 0; y < nHeight; ++y) {
						pixelWriter.Seek(0, y);
						for (int x = 0; x < nWidth; ++x) {
							if ((((x & checkerSize) ^ (y & checkerSize)) ^ (z & checkerSize)) != 0) { 
								pixelWriter.WritePixel(color1.R, color1.G, color1.B, color1.A);
							}
							else {
								pixelWriter.WritePixel(color2.R, color2.G, color2.B, color2.A);
							}
						}
					}
				}
			}
		}
	}
}
