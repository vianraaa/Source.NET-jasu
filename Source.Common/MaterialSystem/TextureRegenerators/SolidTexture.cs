namespace Source.Common.MaterialSystem.TextureRegenerators;
public class SolidTexture(Color Color) : ITextureRegenerator
{
	public void RegenerateTextureBits(ITexture texture, IVTFTexture vtfTexture, in System.Drawing.Rectangle rect) {
		int nMipCount = texture.IsMipmapped() ? vtfTexture.MipCount() : 1;
		for (int iFrame = 0; iFrame < vtfTexture.FrameCount(); ++iFrame) {
			for (int iFace = 0; iFace < vtfTexture.FaceCount(); ++iFace) {
				for (int iMip = 0; iMip < nMipCount; ++iMip) {
					vtfTexture.ComputeMipLevelDimensions(iMip, out int width, out int height, out int depth);
					for (int z = 0; z < depth; ++z) {
						using PixelWriter pixelWriter = new();
						pixelWriter.SetPixelMemory(vtfTexture.Format(), vtfTexture.ImageData(iFrame, iFace, iMip, 0, 0, z), vtfTexture.RowSizeInBytes(iMip));

						for (int y = 0; y < height; ++y) {
							pixelWriter.Seek(0, y);
							for (int x = 0; x < width; ++x) {
								pixelWriter.WritePixel(Color.R, Color.G, Color.B, Color.A);
							}
						}
					}
				}
			}
		}
	}
}