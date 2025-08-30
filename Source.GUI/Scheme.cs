using Source.Common.Client;
using Source.Common.Engine;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Common.Launcher;

using System;
using System.Globalization;

using static System.Net.Mime.MediaTypeNames;

namespace Source.GUI;

public class Scheme : IScheme
{
	[Imported] public ISystem System;
	[Imported] public ISurface Surface;
	IPanel SizingPanel;
	KeyValues Data;
	KeyValues BaseSettings;
	KeyValues Colors;

	public string fileName;
	public string tag;

	struct FontAlias {
		public ulong TrueFontSymbol;
		public IFont Font;
		public bool Proportional;
	}
	Dictionary<ulong, FontAlias> FontAliases = [];

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

		if (inTag != null)
			tag = new(inTag);

		LoadFonts();
		LoadBorders();
	}

	private void LoadBorders() {

	}

	private void LoadFonts() {
		Span<char> language = stackalloc char[64];
		bool valid = System.GetRegistryString("HKEY_CURRENT_USER\\Software\\Valve\\Source\\Language", language);
		if (!valid)
			"english".CopyTo(language);

		for (var kv = Data.FindKey("CustomFontFiles", true)!.GetFirstSubKey(); kv != null; kv = kv.GetNextKey()) {
			ReadOnlySpan<char> fontFile = kv.GetString();
			if (fontFile != null && fontFile[0] != 0) {
				Surface.AddCustomFontFile(null, fontFile);
			}
			else {
				int rangeMin = 0, rangeMax = 0;
				ReadOnlySpan<char> name = null;
				bool useRange = false;

				for (KeyValues? data = kv.GetFirstSubKey(); data != null; data = data.GetNextKey()) {
					ReadOnlySpan<char> key = data.Name;
					if (key.Equals("font", StringComparison.OrdinalIgnoreCase))
						fontFile = data.GetString();
					else if (key.Equals("name", StringComparison.OrdinalIgnoreCase))
						name = data.GetString();
					else {
						if (key.Equals(language, StringComparison.OrdinalIgnoreCase)) {
							KeyValues? pRange = data.FindKey("range");
							if (pRange != null) {
								useRange = true;
								// TODO: scanf or some other faster/less allocationy way to do this...
								string[] parts = new string(pRange.GetString()).Split(' ', StringSplitOptions.RemoveEmptyEntries);
								if (
									parts.Length >= 2 &&
									int.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out rangeMin) &&
									int.TryParse(parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out rangeMax)
								)
									if (rangeMin > rangeMax)
										(rangeMax, rangeMin) = (rangeMin, rangeMax);
							}
						}
					}

					if (fontFile != null && fontFile[0] != 0) {
						Surface.AddCustomFontFile(name, fontFile);

						if (useRange)
							SetFontRange(name, rangeMin, rangeMax);
					}
				}
			}
		}

		for (KeyValues? kv = Data.FindKey("BitmapFontFiles", true)!.GetFirstSubKey(); kv != null; kv = kv.GetNextKey()) {
			ReadOnlySpan<char> fontFile = kv.GetString();
			if (fontFile != null && fontFile[0] != 0) {
				bool success = Surface.AddBitmapFontFile(fontFile);
				if (success) 
					Surface.SetBitmapFontName(kv.Name, fontFile);
			}
		}

		// create the fonts
		for (KeyValues? kv = Data.FindKey("Fonts", true)!.GetFirstSubKey(); kv != null; kv = kv.GetNextKey()) {
			for (int i = 0; i < 2; i++) {
				// create the base font
				bool proportionalFont = i > 0;
				ReadOnlySpan<char> fontName = GetMungedFontName(kv.Name, tag, proportionalFont);
				IFont font = Surface.CreateFont();

				FontAlias alias = new();
				alias.TrueFontSymbol = kv.Name.Hash();
				alias.Font = font;
				alias.Proportional = proportionalFont;
				FontAliases[alias.TrueFontSymbol] = alias;
			}
		}

		// load in the font glyphs
		ReloadFontGlyphs();
	}

	static char[] mungeBuffer = new char[64];
	private ReadOnlySpan<char> GetMungedFontName(ReadOnlySpan<char> fontName, ReadOnlySpan<char> scheme, bool proportional) {
		if(scheme != null) 
			sprintf(mungeBuffer, $"{fontName}{scheme}-{(proportional ? "p" : "no")}");
		else 
			sprintf(mungeBuffer, $"{fontName}-{(proportional ? "p" : "no")}");

		return mungeBuffer;
	}

	private void ReloadFontGlyphs() {

	}

	record struct FontRange {
		public int Min;
		public int Max;
	}

	Dictionary<ulong, FontRange> FontRanges = [];

	private void SetFontRange(ReadOnlySpan<char> name, int rangeMin, int rangeMax) {
		ulong symbol = name.Hash();
		FontRanges[symbol] = new() {
			Min = rangeMin,
			Max = rangeMax
		};
	}
}
