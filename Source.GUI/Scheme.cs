using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;

namespace Source.GUI;

public class Scheme : IScheme
{
	public IBorder? GetBorder(ReadOnlySpan<char> borderName) {
		throw new NotImplementedException();
	}

	public IBorder? GetBorderAtIndex(int index) {
		throw new NotImplementedException();
	}

	public int GetBorderCount() {
		throw new NotImplementedException();
	}

	public IEnumerable<IBorder> GetBorders() {
		throw new NotImplementedException();
	}

	public KeyValues GetColorData() {
		throw new NotImplementedException();
	}

	public IFont? GetFont(ReadOnlySpan<char> fontName, bool proportional = false) {
		throw new NotImplementedException();
	}

	public IFont? GetFontAtIndex() {
		throw new NotImplementedException();
	}

	public int GetFontCount() {
		throw new NotImplementedException();
	}

	public IEnumerable<IFont> GetFonts() {
		throw new NotImplementedException();
	}

	public ReadOnlySpan<char> GetResourceString(ReadOnlySpan<char> stringName) {
		throw new NotImplementedException();
	}

	internal void LoadFromFile(IPanel? sizingPanel, ReadOnlySpan<char> fileName, ReadOnlySpan<char> tag, KeyValues data) {
		throw new NotImplementedException();
	}
}
