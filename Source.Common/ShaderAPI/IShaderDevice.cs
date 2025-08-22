using Source.Common.Bitmap;

namespace Source.Common.ShaderAPI;

[Flags]
public enum GraphicsAPIVersion : ulong {
	OpenGL = 1 << 62,
	// If we even try to do something with these one day (extremely unlikely)
	DirectX = 1 << 61,
	Vulkan = 1 << 60,
	Metal = 1 << 59,

	OpenGL46 = OpenGL | 460
}
public interface IShaderDevice
{
	bool IsDeactivated();
	bool IsUsingGraphics();
	void Present();
}
public struct ShaderDisplayMode {
	public int Width;
	public int Height;
	public ImageFormat Format;
	public int RefreshRateNumerator;
	public int RefreshRateDenominator;

	public readonly double RefreshRate => (double)RefreshRateNumerator / RefreshRateDenominator;
}

public struct ShaderDeviceInfo {
	public ShaderDisplayMode DisplayMode;
	public int BackBufferCount;
	public int AASamples;
	public int AAQuality;
	public GraphicsAPIVersion Driver;
	public int WindowedSizeLimitWidth;
	public int WindowedSizeLimitHeight;

	public bool Windowed;
	public bool Resizing;
	public bool UseStencil;
	public bool LimitWindowedSize;
	public bool WaitForVSync;
	public bool ScaleToOutputResolution;
	public bool Progressive;
	public bool UsingMultipleWindows;
}