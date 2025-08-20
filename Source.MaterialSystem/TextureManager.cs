using Source.Common.MaterialSystem;

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
}
