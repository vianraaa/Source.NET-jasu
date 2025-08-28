using Source.Common.GUI;

namespace Source.GUI;

public class SchemeManager : ISchemeManager
{
	public bool DeleteImage(ReadOnlySpan<char> imageName) {
		throw new NotImplementedException();
	}

	public IScheme GetDefaultScheme() {
		throw new NotImplementedException();
	}

	public IImage GetImage(ReadOnlySpan<char> imageName, bool hardwareFiltered) {
		throw new NotImplementedException();
	}

	public int GetProportionalNormalizedValue(int scaled) {
		throw new NotImplementedException();
	}

	public int GetProportionalScaledValue(int normalized) {
		throw new NotImplementedException();
	}

	public IScheme GetScheme(ReadOnlySpan<char> tag) {
		throw new NotImplementedException();
	}

	public IScheme? LoadSchemeFromFile(ReadOnlySpan<char> fileName, ReadOnlySpan<char> tag) {
		throw new NotImplementedException();
	}

	public IScheme LoadSchemeFromFileEx(IPanel sizingPanel, ReadOnlySpan<char> fileName, ReadOnlySpan<char> tag) {
		throw new NotImplementedException();
	}

	public void ReloadFonts() {
		throw new NotImplementedException();
	}

	public void ReloadSchemes() {
		throw new NotImplementedException();
	}

	public void Shutdown(bool full = true) {
		throw new NotImplementedException();
	}
}