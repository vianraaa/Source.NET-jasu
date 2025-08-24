using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public interface ITextureInternal : ITexture
{
	public static string NormalizeTextureName(ReadOnlySpan<char> name) {

		return new(name); // todo.
	}

	void Bind(in MaterialVarGPU hardwareTarget, int frame);
	void Precache();
}
