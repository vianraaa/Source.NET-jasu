using Source.Common.Bitmap;
using Source.Common.Utilities;

namespace Source.Common.MaterialSystem;

public class TextureReference : Reference<ITexture>
{
	readonly IMaterialSystem materials = Singleton<IMaterialSystem>();

	public void Init(ReadOnlySpan<char> texture, ReadOnlySpan<char> textureGroupName, bool complain = true) {

	}

	public void InitProceduralTexture(ReadOnlySpan<char> textureName, ReadOnlySpan<char> textureGroupName, int w, int h, ImageFormat format, TextureFlags flags) {

	}

	public void InitRenderTarget(int w, int h, RenderTargetSizeMode sizeMod, ImageFormat format, MaterialRenderTargetDepth depth, bool hdr, ReadOnlySpan<char> optionalName = default) {

	}

	public void Init(ITexture texture) {

	}

	public void Shutdown(bool deleteIfUnreferenced = false) {

	}
}
