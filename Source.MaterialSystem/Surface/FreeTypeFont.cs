using Source.Common.GUI;

using FreeTypeSharp;
using static FreeTypeSharp.FT;
using Source.Common.Launcher;

namespace Source.MaterialSystem.Surface;

public unsafe class FreeTypeFont : BaseFont
{
	internal static readonly FT_LibraryRec_* Library;
	static FreeTypeFont() {
		FT_Error error;
		fixed (FT_LibraryRec_** outRec = &Library)
			error = FT_Init_FreeType(outRec);

		if (error != 0)
			throw new Exception("FT_Init_FreeType failed");
	}

	internal readonly FontManager fontManager;
	internal readonly ISystem system;
	public FreeTypeFont(FontManager fontManager, ISystem system) {
		this.fontManager = fontManager;
		this.system = system;
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
	ushort[] BitmapSize = new ushort[2];

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

		FT_Error error;

		byte* font = fontManager.GetFontBinary(fontName, out nint length);
		if (font == null)
			return false;

		fixed (FT_FaceRec_** facePtr = &face)
			error = FT_New_Memory_Face(Library, font, length, 0, facePtr);
		if (error != FT_Error.FT_Err_Ok) { DevMsg($"Upcoming error info: {fontName}\n"); Assert(false); Warning($"FreeType error during new face initialization: {error}\n"); return false; }

		error = FT_Set_Pixel_Sizes(face, 0, (uint)tall);
		if (error != FT_Error.FT_Err_Ok) { Warning($"FreeType error during pixel size set: {error}\n"); return false; }

		FT_Size_Metrics_ tm = face->size->metrics;
		Height = (uint)(tm.height >> 6) + DropShadowOffset + 2 * OutlineSize;
		MaxCharWidth = (uint)(tm.max_advance >> 6);
		Ascent = (uint)(tm.ascender >> 6);

		BitmapSize[0] = (ushort)((int)(tm.max_advance >> 6) + OutlineSize * 2);
		BitmapSize[1] = (ushort)((int)(tm.height >> 6) + DropShadowOffset + OutlineSize * 2);
		return true;
	}

	public struct GlyphABC {
		public int A;
		public int B;
		public int C;
	}
	Dictionary<char, GlyphABC> Glyphs = [];

	public override void GetCharABCwidths(char ch, out int a, out int b, out int c) {
		if (!Glyphs.TryGetValue(ch, out GlyphABC glyphABC)) {
			FT_Load_Char(face, ch, FT_LOAD.FT_LOAD_DEFAULT);
			FT_GlyphSlotRec_* g = face->glyph;

			a = (int)(g->metrics.horiBearingX >> 6);          // leading
			b = (int)(g->metrics.width >> 6);                 // glyph width
			c = (int)((g->metrics.horiAdvance >> 6) - a - b); // trailing
			glyphABC = new() {
				A = a,
				B = b,
				C = c
			};
			Glyphs.Add(ch, glyphABC);
			return;
		}

		a = glyphABC.A;
		b = glyphABC.B;
		c = glyphABC.C;
	}

	internal override void GetCharRGBA(char ch, int rgbaWide, int rgbaTall, Span<byte> rgba) {
		FT_Load_Char(face, ch, FT_LOAD.FT_LOAD_DEFAULT);
		FT_GlyphSlotRec_* slot = face->glyph;
		FT_Render_Glyph(slot, FT_Render_Mode_.FT_RENDER_MODE_NORMAL);
		DrawBitmap(slot, rgbaWide, rgbaTall, rgba);
	}

	private unsafe void DrawBitmap(FT_GlyphSlotRec_* rec, int rgbaWide, int rgbaTall, Span<byte> rgba) {
		FT_Bitmap_* bitmap = &rec->bitmap;
		byte* buffer = bitmap->buffer;
		int bmpWidth = (int)bitmap->width;
		int bmpHeight = (int)bitmap->rows;

		for (int y = 0; y < bmpHeight; y++) {
			byte* row = buffer + y * Math.Abs(bitmap->pitch);
			for (int x = 0; x < bmpWidth; x++) {
				int dstX = x + rec->bitmap_left;
				int dstY = (int)(Ascent - rec->bitmap_top + y);

				if (dstX < 0 || dstY < 0 || dstX >= rgbaWide || dstY >= rgbaTall)
					continue;

				byte coverage = row[x];
				int dstIndex = (dstY * rgbaWide + dstX) * 4;
				rgba[dstIndex + 0] = 255;
				rgba[dstIndex + 1] = 255;
				rgba[dstIndex + 2] = 255;
				rgba[dstIndex + 3] = coverage;
			}
		}
	}

	public override bool GetUnderlined() {
		return false;
	}
}
