using Source.Common.Engine;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Common.Launcher;

namespace Source.GUI;

public class Scheme : IScheme
{
	[Imported] ISystem System;
	IPanel SizingPanel;
	KeyValues Data;
	KeyValues BaseSettings;
	KeyValues Colors;
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

	internal void LoadFromFile(IPanel? sizingPanel, ReadOnlySpan<char> fileName, ReadOnlySpan<char> inTag, KeyValues inKeys) {
		Data = inKeys;
		BaseSettings = Data.FindKey("BaseSettings", true)!;
		Colors = Data.FindKey("Colors", true)!;

		KeyValues name = Data.FindKey("Name", true)!;
		name.SetString("Name", inTag);

		LoadFonts();
		LoadBorders();
	}

	private void LoadBorders() {

	}

	private void LoadFonts() {
		Span<char> language = stackalloc char[64];
		bool valid = System.GetRegistryString("HKEY_CURRENT_USER\\Software\\Valve\\Source\\Language", language);
	}
}
