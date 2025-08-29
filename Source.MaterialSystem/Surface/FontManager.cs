using Source.Common.Filesystem;
using Source.Common.GUI;
using Source.Common.MaterialSystem;

namespace Source.MaterialSystem.Surface;

public class FontAmalgam : IFont
{
	public ReadOnlySpan<char> GetName() {
		throw new NotImplementedException();
	}
}

public class FontManager(IMaterialSystem materialSystem, IFileSystem fileSystem)
{
	List<FontAmalgam> FontAmalgams = [];
	internal IFont CreateFont() {
		FontAmalgam font = new();
		FontAmalgams.Add(font);
		return font;
	}
}
