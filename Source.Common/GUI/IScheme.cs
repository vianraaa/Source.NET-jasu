using Source.Common.Formats.Keyvalues;

namespace Source.Common.GUI;

public interface IScheme {
	ReadOnlySpan<char> GetResourceString(ReadOnlySpan<char> stringName);
	IBorder? GetBorder(ReadOnlySpan<char> borderName);
	IFont? GetFont(ReadOnlySpan<char> fontName, bool proportional = false);
	int GetBorderCount();
	IBorder? GetBorderAtIndex(int index);
	int GetFontCount();
	IFont? GetFontAtIndex();

	IEnumerable<IBorder> GetBorders();
	IEnumerable<IFont> GetFonts();

	KeyValues GetColorData();
}

public interface ISchemeManager {
	IScheme? LoadSchemeFromFile(ReadOnlySpan<char> fileName, ReadOnlySpan<char> tag);
	void ReloadSchemes();
	void ReloadFonts();
	IScheme GetDefaultScheme();
	IScheme GetScheme(ReadOnlySpan<char> tag);
	IImage GetImage(ReadOnlySpan<char> imageName, bool hardwareFiltered);
	void Shutdown(bool full = true);
	int GetProportionalScaledValue(int normalized);
	int GetProportionalNormalizedValue(int scaled);

	IScheme LoadSchemeFromFileEx(IPanel sizingPanel, ReadOnlySpan<char> fileName, ReadOnlySpan<char> tag);
	bool DeleteImage(ReadOnlySpan<char> imageName);
}