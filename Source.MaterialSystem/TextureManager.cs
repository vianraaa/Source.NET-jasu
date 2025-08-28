using Source.Common;
using Source.Common.Bitmap;
using Source.Common.MaterialSystem;

using Steamworks;

using System.Drawing;

namespace Source.MaterialSystem;



public class SolidTexture(Color Color) : ITextureRegenerator
{
	public void RegenerateTextureBits(ITexture texture, IVTFTexture vtfTexture, in System.Drawing.Rectangle rect) {
		int nMipCount = texture.IsMipmapped() ? vtfTexture.MipCount() : 1;
		for (int iFrame = 0; iFrame < vtfTexture.FrameCount(); ++iFrame) {
			for (int iFace = 0; iFace < vtfTexture.FaceCount(); ++iFace) {
				for (int iMip = 0; iMip < nMipCount; ++iMip) {
					vtfTexture.ComputeMipLevelDimensions(iMip, out int width, out int height, out int depth);
					for (int z = 0; z < depth; ++z) {
						using PixelWriter pixelWriter = new PixelWriter();
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

public class TextureManager : ITextureManager
{
	public MaterialSystem MaterialSystem;
	public ITextureInternal CreateFileTexture(ReadOnlySpan<char> fileName, ReadOnlySpan<char> textureGroupName) {
		Texture tex = new(MaterialSystem);
		tex.InitFileTexture(fileName, textureGroupName);
		return tex;
	}

	public ITextureInternal ErrorTexture() {
		return null;
	}

	private ITexture error;
	private ITexture white;

	const int ERROR_TEXTURE_SIZE = 32;
	const int WHITE_TEXTURE_SIZE = 1;
	const int BLACK_TEXTURE_SIZE = 1;
	const int GREY_TEXTURE_SIZE = 1;

	public void Init() {
		Color color = new();

		white = CreateProceduralTexture("white", TEXTURE_GROUP_OTHER, WHITE_TEXTURE_SIZE, WHITE_TEXTURE_SIZE, 1, ImageFormat.BGRX8888, CompiledVtfFlags.NoMip | CompiledVtfFlags.SingleCopy)!;
		color.R = color.G = color.B = color.A = 255;
		CreateSolidTexture(white, color);
	}

	private void CreateSolidTexture(ITexture tex, Color color) => tex.SetTextureGenerator(new SolidTexture(color));

	public ITexture? CreateProceduralTexture(ReadOnlySpan<char> name, ReadOnlySpan<char> textureGroup, int w, int h, int d, ImageFormat imageFormat, CompiledVtfFlags flags, ITextureRegenerator? generator = null) {
		Texture newTexture = new(MaterialSystem);
		newTexture.InitProceduralTexture(name, textureGroup, w, h, d, imageFormat, flags, generator);
		if (newTexture == null)
			return null;

		TextureList[newTexture.GetName().Hash()] = newTexture;
		((ITextureInternal)newTexture).Download();

		return newTexture;
	}

	Dictionary<ulong, ITextureInternal> TextureList = [];

	public ITextureInternal? FindOrLoadTexture(ReadOnlySpan<char> textureName, ReadOnlySpan<char> textureGroupName, int additionalCreationFlags) {
		ITextureInternal? texture = FindTexture(textureName);
		if (texture == null) {
			texture = LoadTexture(textureName, textureGroupName, additionalCreationFlags);
			if (texture != null) 
				TextureList.Add(texture.GetName().Hash(), texture);
		}

		return texture;
	}

	public ITextureInternal? LoadTexture(ReadOnlySpan<char> textureName, ReadOnlySpan<char> textureGroupName, int additionalCreationFlags, bool download = true) {
		ITextureInternal? newTexture = CreateFileTexture(textureName, textureGroupName);
		if(newTexture != null) {
			if (download)
				newTexture.Download();
		}

		return newTexture;
	}

	public void RestoreTexture(ITextureInternal texture) {
		texture.OnRestore();
		texture.Download();
	}

	public ITextureInternal? FindTexture(ReadOnlySpan<char> textureName) {
		if (TextureList.TryGetValue(textureName.Hash(), out ITextureInternal? tex))
			return tex;

		return null;
	}

	internal void RestoreRenderTargets() {
		foreach (var tex in TextureList)
			if (tex.Value.IsRenderTarget())
				RestoreTexture(tex.Value);
	}

	internal void RestoreNonRenderTargetTextures() {
		foreach(var tex in TextureList) 
			if (!tex.Value.IsRenderTarget())
				RestoreTexture(tex.Value);
	}
}
