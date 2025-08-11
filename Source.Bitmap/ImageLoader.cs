
namespace Source.Bitmap;

public static class ImageLoader
{
	public static int GetNumMipMapLevels(int width, int height, int depth) {
		if (depth <= 0)
			depth = 1;

		if (width < 1 || height < 1)
			return 0;

		int mipLevels = 1;
		while (true) {
			if (width == 1 && height == 1 && depth == 1)
				break;

			width >>= 1;
			height >>= 1;
			depth >>= 1;

			if (width < 1)
				width = 1;

			if (height < 1)
				height = 1;

			if (depth < 1)
				depth = 1;

			mipLevels++;
		}

		return mipLevels;
	}
}