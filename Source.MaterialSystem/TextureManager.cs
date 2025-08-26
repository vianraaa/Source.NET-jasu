using Source.Common.MaterialSystem;

using Steamworks;

namespace Source.MaterialSystem;
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

	public void Init() {

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

	public ITextureInternal? FindTexture(ReadOnlySpan<char> textureName) {
		if (TextureList.TryGetValue(textureName.Hash(), out ITextureInternal? tex))
			return tex;

		return null;
	}
}
