using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;

namespace Source.GUI;

public class SchemeManager : ISchemeManager
{
	readonly IFileSystem fileSystem;
	readonly IServiceProvider services;
	public SchemeManager(IFileSystem fileSystem, IServiceProvider services) {
		this.services = services;
		this.fileSystem = fileSystem;
	}

	public void Init() {
		Schemes.Add(services.New<Scheme>());
	}

	public bool DeleteImage(ReadOnlySpan<char> imageName) {
		throw new NotImplementedException();
	}

	public IScheme GetDefaultScheme() {
		return Schemes[0];
	}

	bool initializedFirstScheme;

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
		if (scheme != null) {
			return scheme;
		}

		KeyValues? data = new("Scheme");
		data.UsesEscapeSequences(true);

		bool result = data.LoadFromFile(fileSystem, fileName, "GAME");
		if (!result)
			result = data.LoadFromFile(fileSystem, fileName, null);

		if (!result) {
			data = null;
			return null;
		}

		Scheme newScheme = initializedFirstScheme ? services.New<Scheme>() : Schemes[0];
		newScheme.LoadFromFile(sizingPanel, fileName, tag, data);
		if (initializedFirstScheme)
			Schemes.Add(newScheme);
		initializedFirstScheme = true;

		return newScheme;
	}

	readonly List<Scheme> Schemes = [];

	private IScheme? FindLoadedScheme(ReadOnlySpan<char> fileName) {
		return null;
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