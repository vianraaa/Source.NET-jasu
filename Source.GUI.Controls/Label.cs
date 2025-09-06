using CommunityToolkit.HighPerformance;

using Source.Common.Engine;
using Source.Common.GUI;

using static System.Net.Mime.MediaTypeNames;

namespace Source.GUI.Controls;

public class Label : Panel
{
	public static Panel Create_Label() => new Label(null, null, "Label");

	protected Alignment ContentAlignment;
	protected ColorState TextColorState;
	protected string? FontOverrideName;
	protected bool Wrap;
	protected bool CenterWrap;
	protected bool AutoWideToContents;
	protected bool UseProportionalInsets;
	protected bool AutoWideDirty;

	protected TextImage? TextImage;

	public Label(Panel? parent, string? panelName, string text) : base(parent, panelName) {
		Init();

		TextImage = new TextImage(text);
		TextImage.SetColor(new(0, 0, 0, 0));
		SetText(text);
		TextImageIndex = AddImage(TextImage, 0);
	}

	public nint AddImage(TextImage textImage, int offset) {
		nint newImage = Images.Count;
		Images.Add(new() {
			Image = textImage,
			Offset = (short)offset,
			XPos = -1,
			Width = -1
		});
		InvalidateLayout();
		return newImage;
	}

	public override void PerformLayout() {
		GetSize(out int wide, out int tall);
		wide -= TextInsetX;

		int twide, ttall;
		if (Images.Count == 1 && TextImageIndex == 0) {
			if (Wrap || CenterWrap) {
				TextImage!.GetContentSize(out twide, out ttall);
				TextImage!.SetSize(wide, ttall);
			}
			else {
				TextImage!.GetContentSize(out twide, out ttall);

				if (wide < twide)
					TextImage!.SetSize(wide, ttall);
				else
					TextImage!.SetSize(twide, ttall);
			}

			HandleAutoSizing();
			HandleAutoSizing();

			return;
		}

		if (TextImageIndex < 0)
			return;

		int widthOfImages = 0;
		Span<ImageInfo> images = Images.AsSpan();
		for (int i = 0; i < images.Length; i++) {
			ref ImageInfo imageInfo = ref images[i];
			IImage? image = imageInfo.Image;
			if (image == null)
				continue;

			if (i == TextImageIndex)
				continue;

			image.GetSize(out int iWide, out int iTall);
			widthOfImages += iWide;
		}

		int spaceAvail = wide - widthOfImages;

		if (spaceAvail < 0)
			return;

		TextImage!.GetSize(out twide, out ttall);
		TextImage!.SetSize(spaceAvail, ttall);

		HandleAutoSizing();
	}

	private void HandleAutoSizing() {
		if (AutoWideDirty) {
			AutoWideDirty = false;

			GetContentSize(out int wide, out int tall);
			SetSize(wide, GetTall());
		}
	}

	void Init() {
		ContentAlignment = Alignment.West;
		TextColorState = ColorState.Normal;
		TextInsetX = 0;
		TextInsetY = 0;
		FontOverrideName = null;
		Wrap = false;
		CenterWrap = false;
		AutoWideToContents = false;
		UseProportionalInsets = false;
		AutoWideDirty = false;
	}

	Color DisabledFgColor1;
	Color DisabledFgColor2;

	public const int Content = 8;
	int TextInsetX, TextInsetY;

	public virtual void GetTextInset(out int xInset, out int yInset) {
		xInset = TextInsetX;
		yInset = TextInsetY;
	}
	public virtual void SetTextInset(int xInset, int yInset) {
		TextInsetX = xInset;
		TextInsetY = yInset;

		GetSize(out int wide, out int tall);
		TextImage!.SetDrawWidth(wide - TextInsetX);
	}
	public virtual Color GetDisabledFgColor1() => DisabledFgColor1;
	public virtual Color GetDisabledFgColor2() => DisabledFgColor2;
	public virtual void SetDisabledFgColor1(in Color c) => DisabledFgColor1 = c;
	public virtual void SetDisabledFgColor2(in Color c) => DisabledFgColor2 = c;
	ColorState ColorState;
	public virtual void SetTextColorState(ColorState state) => ColorState = state;
	struct ImageInfo
	{
		public IImage? Image;
		public short Offset;
		public short XPos;
		public short Width;
	}

	public virtual void SetFont(IFont? font) {
		TextImage!.SetFont(font);
		Repaint();
	}
	public virtual IFont? GetFont() => TextImage!.GetFont();


	public void SetContentAlignment(Alignment alignment) {
		ContentAlignment = alignment;
	}

	public virtual void GetContentSize(out int wide, out int tall) {
		if (GetFont() == null) {
			IScheme? scheme = GetScheme();
			if (scheme != null) {
				SetFont(scheme.GetFont("Default", IsProportional()));
			}
		}

		ComputeAlignment(out int tx0, out int ty0, out int tx1, out int ty1);

		wide = (tx1 - tx0) + TextInsetX;

		TextImage!.GetSize(out int iWide, out int iTall);
		wide -= iWide;
		TextImage.GetContentSize(out iWide, out iTall);
		wide += iWide;

		foreach (var i in Images)
			wide += i.Offset;

		tall = Math.Max((ty1 - ty0) + TextInsetY, iTall);
	}
	public virtual void GetText(Span<char> textOut) { }
	public virtual void SetText(ReadOnlySpan<char> text) {
		if (text == null)
			text = "";

		TextImage!.SetText(text);
		AutoWideDirty = AutoWideToContents;
		InvalidateLayout();
		Repaint();
	}

	public override void ApplySchemeSettings(IScheme scheme) {
		base.ApplySchemeSettings(scheme);

		if (FontOverrideName != null)
			SetFont(scheme.GetFont(FontOverrideName, IsProportional()));

		if (GetFont() == null)
			SetFont(scheme.GetFont("Default", IsProportional()));

		int wide, tall;
		if (Wrap || CenterWrap) {
			GetSize(out wide, out tall);
			wide -= TextInsetX;
			TextImage!.SetSize(wide, tall);

			TextImage!.RecalculateNewLinePositions();
		}
		else {
			TextImage!.GetContentSize(out wide, out tall);
			TextImage!.SetSize(wide, tall);
		}

		if (AutoWideToContents) {
			AutoWideDirty = true;
			HandleAutoSizing();
		}

		// clear out any the images, since they will have been invalidated
		Span<ImageInfo> images = Images.AsSpan();
		for (int i = 0; i < images.Length; i++) {
			IImage? image = images[i].Image;
			if (image == null)
				continue; // skip over null images

			if (i == TextImageIndex)
				continue;

			ref ImageInfo imageInfo = ref images[i];
			imageInfo.Image = null;
		}

		SetDisabledFgColor1(GetSchemeColor("Label.DisabledFgColor1", scheme));
		SetDisabledFgColor2(GetSchemeColor("Label.DisabledFgColor2", scheme));
		SetBgColor(GetSchemeColor("Label.BgColor", scheme));

		switch (TextColorState) {
			case ColorState.Dull:
				SetFgColor(GetSchemeColor("Label.TextDullColor", scheme));
				break;
			case ColorState.Bright:
				SetFgColor(GetSchemeColor("Label.TextBrightColor", scheme));
				break;
			case ColorState.Normal:
			default:
				SetFgColor(GetSchemeColor("Label.TextColor", scheme));
				break;
		}
	}

	nint TextImageIndex;

	public void ClearImages() {
		Images.Clear();
		TextImageIndex = -1;
	}

	public override void Paint() {
		ComputeAlignment(out int tx0, out int ty0, out int tx1, out int ty1);

		// TODO: Research what associates are in vgui

		GetSize(out int labelWide, out int labelTall);
		int x = tx0, y = TextInsetY + ty0;
		int imageYPos = 0;

		Span<ImageInfo> images = Images.AsSpan();
		for (int i = 0; i < images.Length; i++) {
			ref ImageInfo imageInfo = ref images[i];
			IImage? image = imageInfo.Image;
			if (image == null)
				continue;

			x += imageInfo.Offset;

			if (i == TextImageIndex) {
				switch (ContentAlignment) {
					case Alignment.Northwest:
					case Alignment.West:
					case Alignment.Southwest:
						x += TextInsetX;
						break;

					case Alignment.Northeast:
					case Alignment.East:
					case Alignment.Southeast:
						x -= TextInsetX;
						break;
				}
			}

			if (imageInfo.XPos >= 0) {
				x = imageInfo.XPos;
			}

			imageYPos = y;
			image.SetPos(x, y);

			if (ContentAlignment == Alignment.West || ContentAlignment == Alignment.Center || ContentAlignment == Alignment.East) {
				image.GetSize(out int iw, out int it);
				if (it < (ty1 - ty0)) {
					imageYPos = ((ty1 - ty0) - it) / 2 + y;
					image.SetPos(x, ((ty1 - ty0) - it) / 2 + y);
				}
			}

			if (imageInfo.Width >= 0) {
				image.GetSize(out int w, out int t);
				if (w > imageInfo.Width)
					image.SetSize(imageInfo.Width, t);
			}

			if (image == TextImage) {
				if (IsEnabled()) {
					// TODO: Associates...
					TextImage.SetColor(GetFgColor());
					TextImage.Paint();
				}
				else {
					TextImage.SetPos(x + 1, imageYPos + 1);
					TextImage.SetColor(DisabledFgColor1);
					TextImage.Paint();

					Surface.DrawFlushText();
					TextImage.SetPos(x, imageYPos);
					TextImage.SetColor(DisabledFgColor2);
					TextImage.Paint();
				}
			}
			else
				image.Paint();

			image.GetSize(out int wide, out int tall);
			x += wide;
		}
	}

	List<ImageInfo> Images = [];

	private void ComputeAlignment(out int tx0, out int ty0, out int tx1, out int ty1) {
		GetPaintSize(out int wide, out int tall);
		int tWide, tTall;

		tx0 = 0;
		ty0 = 0;

		int maxX = 0, maxY = 0;

		Alignment actualXAlignment = ContentAlignment;
		Span<ImageInfo> images = Images.AsSpan();
		for (int i = 0; i < images.Length; i++) {
			ref ImageInfo imageInfo = ref images[i];
			IImage? image = imageInfo.Image;
			if (image == null)
				continue;

			image.GetSize(out int iWide, out int iTall);
			if (iWide > wide)
				actualXAlignment = Alignment.West;

			maxY = Math.Max(maxY, iTall);
			maxX += iWide;

			maxX += imageInfo.Offset;
		}

		tWide = maxX;
		tTall = maxY;

		switch (actualXAlignment) {
			case Alignment.Northwest:
			case Alignment.West:
			case Alignment.Southwest:
				tx0 = 0;
				break;
			case Alignment.North:
			case Alignment.Center:
			case Alignment.South:
				tx0 = (wide - tWide) / 2;
				break;

			case Alignment.Northeast:
			case Alignment.East:
			case Alignment.Southeast:
				tx0 = wide - tWide;
				break;
		}

		switch (ContentAlignment) {
			case Alignment.Northwest:
			case Alignment.North:
			case Alignment.Northeast:
				ty0 = 0;
				break;

			case Alignment.West:
			case Alignment.Center:
			case Alignment.East:
				ty0 = (tall - tTall) / 2;
				break;

			case Alignment.Southwest:
			case Alignment.South:
			case Alignment.Southeast:
				ty0 = tall - tTall;
				break;
		}

		tx1 = tx0 + tWide;
		ty1 = ty0 + tTall;
	}

	internal TextImage GetTextImage() {
		return TextImage;
	}
}
