using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;

namespace Source.GUI;

public class SchemeManager(IFileSystem fileSystem, IServiceProvider services) : ISchemeManager
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
		return LoadSchemeFromFileEx(null, fileName, tag);
	}

	public IScheme? LoadSchemeFromFileEx(IPanel? sizingPanel, ReadOnlySpan<char> fileName, ReadOnlySpan<char> tag) {
		IScheme? scheme = FindLoadedScheme(fileName);
		if(scheme != null) {
			return scheme;
		}

		KeyValues? data = new("Scheme");
		data.UsesEscapeSequences(true);

		bool result = data.LoadFromFile(fileSystem, fileName, "GAME");
		if(!result) 
			result = data.LoadFromFile(fileSystem, fileName, null);

		if (!result) {
			data = null;
			return null;
		}

		Scheme newScheme = services.New<Scheme>();
		newScheme.LoadFromFile(sizingPanel, fileName, tag, data);
		Schemes.Add(newScheme);
		return newScheme;
	}

	readonly List<Scheme> Schemes = [];

	private IScheme? FindLoadedScheme(ReadOnlySpan<char> fileName) {
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