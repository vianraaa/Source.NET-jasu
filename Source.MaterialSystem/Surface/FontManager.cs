using Source.Common.Filesystem;
using Source.Common.GUI;
using Source.Common.MaterialSystem;

using FreeTypeSharp;
using static FreeTypeSharp.FT;

using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Source.Common.Launcher;
using System.Text;

namespace Source.MaterialSystem.Surface;

public abstract class BaseFont
{
	public abstract SurfaceFontFlags GetFlags();
	public abstract int GetHeight();
	public abstract int GetMaxCharWidth();
	public abstract bool IsEqualTo(ReadOnlySpan<char> fontName, int tall, int weight, int blur, int scanlines, SurfaceFontFlags flags);
}

public unsafe class FreeTypeFont : BaseFont
{
	internal readonly FT_LibraryRec_* lib;
	internal readonly FontManager fontManager;
	internal readonly ISystem system;
	public FreeTypeFont(FontManager fontManager, ISystem system) {
		this.fontManager = fontManager;
		this.system = system;

		FT_Error error;
		fixed (FT_LibraryRec_** outRec = &lib)
			error = FT_Init_FreeType(outRec);
		if (error != 0)
			throw new Exception("FT_Init_FreeType failed");
	}
	string? Name;
	ulong Symbol;
	short Tall;
	ushort Weight;
	SurfaceFontFlags Flags = 0;
	ushort ScanLines;
	ushort Blur;
	bool Underlined;
	uint Height;
	uint MaxCharWidth;
	uint Ascent;
	uint DropShadowOffset;
	uint OutlineSize;
	bool AntiAliased;
	bool Rotary;
	bool Additive;

	public override SurfaceFontFlags GetFlags() => Flags;
	public override int GetHeight() => (int)Height;
	public override int GetMaxCharWidth() => (int)MaxCharWidth;

	public override bool IsEqualTo(ReadOnlySpan<char> fontName, int tall, int weight, int blur, int scanlines, SurfaceFontFlags flags) {
		return fontName.Hash() == Symbol && Tall == tall && Weight == weight && ScanLines == scanlines && flags == Flags;
	}

	FT_FaceRec_* face;

	public bool Create(ReadOnlySpan<char> fontName, int tall, int weight, int blur, int scanlines, SurfaceFontFlags flags) {
		Name = new(fontName);
		Symbol = fontName.Hash();
		Tall = (short)tall;
		Weight = (ushort)weight;
		Flags = flags;
		AntiAliased = flags.HasFlag(SurfaceFontFlags.Antialias);
		Underlined = flags.HasFlag(SurfaceFontFlags.Underline);
		DropShadowOffset = flags.HasFlag(SurfaceFontFlags.DropShadow) ? 1u : 0u;
		OutlineSize = flags.HasFlag(SurfaceFontFlags.Outline) ? 1u : 0u;
		Rotary = flags.HasFlag(SurfaceFontFlags.Rotary);
		Additive = flags.HasFlag(SurfaceFontFlags.Additive);
		Blur = (ushort)blur;
		ScanLines = (ushort)scanlines;

		ReadOnlySpan<char> systemFont = system.GetSystemFontPath(fontName);
		Span<byte> newFaceName = stackalloc byte[Encoding.UTF8.GetByteCount(systemFont)];
		Encoding.UTF8.GetBytes(systemFont, newFaceName);

		FT_Error error;

		fixed (byte* bytes = newFaceName)
		fixed (FT_FaceRec_** facePtr = &face)
			error = FT_New_Face(lib, bytes, 0, facePtr);
		if (error != FT_Error.FT_Err_Ok) { Warning($"FreeType error during new face initialization: {error}\n"); return false; }

		error = FT_Set_Pixel_Sizes(face, 0, (uint)tall);
		if (error != FT_Error.FT_Err_Ok) { Warning($"FreeType error during pixel size set: {error}\n"); return false; }

		return true;
	}
}

public record struct FontRange
{
	public int LowRange;
	public int HighRange;
	public BaseFont Font;
}

public class FontAmalgam : IFont
{
	List<FontRange> Fonts = new(4);
	string Name = "";
	int MaxWidth;
	int MaxHeight;


	public ReadOnlySpan<char> GetName() {
		throw new NotImplementedException();
	}

	public void SetName(ReadOnlySpan<char> name) {
		Name = new(name);
	}

	public SurfaceFontFlags GetFlags(int i) {
		if (Fonts.Count > 0 && Fonts[i].Font != null)
			return Fonts[i].Font.GetFlags();
		else
			return 0;
	}

	public void SetFontScale(float sx, float sy) {
		if (Fonts.Count == 0)
			return;

		if (GetFlags(0).HasFlag(SurfaceFontFlags.Bitmap))
			DevWarning("Bitmap isn't supported yet\n");
		else
			Warning("Bitmap isn't supported yet but that's an illegal operation anyway...\n");
	}


	public ReadOnlySpan<char> GetFontName(char c) {
		throw new NotImplementedException();
	}

	public ReadOnlySpan<char> GetFontFamilyName(char c) {
		throw new NotImplementedException();
	}

	public nint GetCount() {
		return Fonts.Count;
	}

	public void RemoveAll() {
		Fonts.Clear();
		MaxHeight = 0;
		MaxWidth = 0;
	}

	public BaseFont? GetFontForChar(char ch) {
		foreach (var font in Fonts) {
			if (ch >= font.LowRange && ch <= font.HighRange) {
				return font.Font;
			}
		}

		return null;
	}

	internal void AddFont(BaseFont font, int low, int high) {
		Fonts.Add(new() {
			Font = font,
			LowRange = low,
			HighRange = high
		});

		MaxHeight = Math.Max(font.GetHeight(), MaxHeight);
		MaxWidth = Math.Max(font.GetMaxCharWidth(), MaxWidth);
	}
}

// NOTE ABOUT ARG #3: Not a circular reference because it's instantiated by the ISurface implementation itself.
public unsafe class FontManager(IMaterialSystem materialSystem, IFileSystem fileSystem, ISystem system, ISurface surface)
{
	List<FontAmalgam> FontAmalgams = [];
	List<FreeTypeFont> FreeTypeFonts = [];
	internal IFont CreateFont() {
		FontAmalgam font = new();
		FontAmalgams.Add(font);
		return font;
	}

	internal bool SetFontGlyphSet(IFont font, ReadOnlySpan<char> fontName, int tall, int weight, int blur, int scanlines, SurfaceFontFlags flags, int rangeMin, int rangeMax) {
		if (font is not FontAmalgam fontAmalgam) {
			Warning($"Invalid IFont input into SetFontGlyphSet!\n");
			return false;
		}

		if (fontAmalgam.GetCount() > 0)
			fontAmalgam.RemoveAll();

		FreeTypeFont? baseFont = CreateOrFindFreeTypeFont(fontName, tall, weight, blur, scanlines, flags);

		do {
			if (IsFontForeignLanguageCapable(fontName)) {
				if (baseFont != null) {
					fontAmalgam.AddFont(baseFont, 0x0000, 0xFFFF);
					return true;
				}
			}
			else {
				ReadOnlySpan<char> localizedFontName = GetForeignFallbackFontName();
				if (baseFont != null && localizedFontName.Equals(fontName, StringComparison.OrdinalIgnoreCase)) {
					fontAmalgam.AddFont(baseFont, 0x0000, 0xFFFF);
					return true;
				}

				FreeTypeFont? extendedFont = CreateOrFindFreeTypeFont(localizedFontName, tall, weight, blur, scanlines, flags);
				if (baseFont != null && extendedFont != null) {
					int min = 0x0000, max = 0x00FF;

					if (rangeMin > 0 || rangeMax > 0) {
						min = rangeMin;
						max = rangeMax;

						if (min > max)
							(max, min) = (min, max);
					}

					if (min > 0)
						fontAmalgam.AddFont(extendedFont, 0x0000, min - 1);

					fontAmalgam.AddFont(baseFont, min, max);

					if (max < 0xFFFF)
						fontAmalgam.AddFont(extendedFont, max + 1, 0xFFFF);

					return true;
				}
				else if (extendedFont != null) {
					fontAmalgam.AddFont(extendedFont, 0x0000, 0xFFFF);
					return true;
				}
			}
		}
		while ((fontName = GetFallbackFontName(fontName)) != null);

		return false;
	}

	private ReadOnlySpan<char> GetFallbackFontName(ReadOnlySpan<char> fontName) {
		int i;
		for (i = 0; FallbackFonts[i].Font != null; i++) {
			if (fontName.Equals(FallbackFonts[i].Font, StringComparison.OrdinalIgnoreCase))
				return FallbackFonts[i].Fallback;
		}

		return FallbackFonts[i].Fallback;
	}

	private ReadOnlySpan<char> GetForeignFallbackFontName() {
#if WIN32
		return "Tahoma";
#else
#error Please define GetForeignFallbackFontName for this platform.
#endif
	}

	public struct FallbackFont
	{
		public string? Font;
		public string? Fallback;
		public FallbackFont(string? font, string? fallback) {
			Font = font;
			Fallback = fallback;
		}
	}

#if WIN32
	static readonly string?[] ValidAsianFonts = ["Marlett", null];
	static readonly FallbackFont[] FallbackFonts = [
		new("Times New Roman", "Courier New"),
		new("Courier New", "Courier"),
		new("Verdana", "Arial"),
		new("Trebuchet MS", "Arial"),
		new("Tahoma", null),
		new(null, "Tahoma")
	];
#else
#error Please define fallback fonts for this platform
#endif

	private bool IsFontForeignLanguageCapable(ReadOnlySpan<char> fontName) {
		for (int i = 0; ValidAsianFonts[i] != null; i++) {
			if (fontName.Equals(ValidAsianFonts[i], StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}

	private FreeTypeFont? CreateOrFindFreeTypeFont(ReadOnlySpan<char> fontName, int tall, int weight, int blur, int scanlines, SurfaceFontFlags flags) {
		FreeTypeFont? foundFont = null;
		foreach (var font in FreeTypeFonts) {
			if (font.IsEqualTo(fontName, tall, weight, blur, scanlines, flags)) {
				foundFont = font;
				break;
			}
		}
		if (foundFont == null) {
			FreeTypeFont font = new FreeTypeFont(this, system);
			if (font.Create(fontName, tall, weight, blur, scanlines, flags)) {
				foundFont = font;
				FreeTypeFonts.Add(font);
			}
			else
				return null;
		}
		return foundFont;
	}
}
