using Source.Common;
using Source.Common.MaterialSystem;

using System.Drawing;
using System.Numerics;

namespace Source.VTF;

public class VTFTexture : IVTFTexture
{
	public float BumpScale() {
		throw new NotImplementedException();
	}

	public void ComputeAlphaFlags() {
		throw new NotImplementedException();
	}

	public int ComputeFaceSize(int startingMipLevle = 0) {
		throw new NotImplementedException();
	}

	public void ComputeMipLevelDimensions(int level, out int width, out int height, out int depth) {
		throw new NotImplementedException();
	}

	public void ComputeMipLevelSubRect(Rectangle srcRect, int mipLevel, out Rectangle subRect) {
		throw new NotImplementedException();
	}

	public int ComputeMipSize(int mipLevel) {
		throw new NotImplementedException();
	}

	public void ComputeReflectivity() {
		throw new NotImplementedException();
	}

	public int ComputeTotalSize() {
		throw new NotImplementedException();
	}

	public void ConstructLowResImage() {
		throw new NotImplementedException();
	}

	public void ConvertImageFormat(ImageFormat format, bool normalToDUDV) {
		throw new NotImplementedException();
	}

	public int Depth() {
		throw new NotImplementedException();
	}

	public void Dispose() {
		throw new NotImplementedException();
	}

	public int FaceCount() {
		throw new NotImplementedException();
	}

	public int FaceSizeInBytes(int mipLevel) {
		throw new NotImplementedException();
	}

	public int FileSize(int mipSkipCount = 0) {
		throw new NotImplementedException();
	}

	public int Flags() {
		throw new NotImplementedException();
	}

	public ImageFormat Format() {
		throw new NotImplementedException();
	}

	public int FrameCount() {
		throw new NotImplementedException();
	}

	public void GenerateMipmaps() {
		throw new NotImplementedException();
	}

	public Span<byte> GetResourceData(uint type) {
		throw new NotImplementedException();
	}

	public bool HasResourceEntry(uint type) {
		throw new NotImplementedException();
	}

	public int Height() {
		throw new NotImplementedException();
	}

	public Span<byte> ImageData() {
		throw new NotImplementedException();
	}

	public Span<byte> ImageData(int frame, int face, int mipLevel) {
		throw new NotImplementedException();
	}

	public Span<byte> ImageData(int frame, int face, int mipLevel, int x, int y, int z = 0) {
		throw new NotImplementedException();
	}

	public void ImageFile(int frame, int face, int mip, out int startLocation, out int sizeInBytes) {
		throw new NotImplementedException();
	}

	public bool Init(int width, int height, int depth, ImageFormat format, int flags, int frameCount, int forceMipCount = -1) {
		throw new NotImplementedException();
	}

	public void InitLowResImage(int width, int height, ImageFormat format) {
		throw new NotImplementedException();
	}

	public bool IsCubeMap() {
		throw new NotImplementedException();
	}

	public bool IsNormalMap() {
		throw new NotImplementedException();
	}

	public bool IsVolumeTexture() {
		throw new NotImplementedException();
	}

	public void LowResFileInfo(out int startLocation, out int sizeInBytes) {
		throw new NotImplementedException();
	}

	public ImageFormat LowResFormat() {
		throw new NotImplementedException();
	}

	public Span<byte> LowResImageData() {
		throw new NotImplementedException();
	}

	public int LowResWidth() {
		throw new NotImplementedException();
	}

	public int LowRetHeight() {
		throw new NotImplementedException();
	}

	public int MipCount() {
		throw new NotImplementedException();
	}

	public ReadOnlySpan<Vector3> Reflectivity() {
		throw new NotImplementedException();
	}

	public int RowSizeInBytes(int mipLevel) {
		throw new NotImplementedException();
	}

	public bool Serialize(Stream stream) {
		throw new NotImplementedException();
	}

	public void SetBumpScale(float scale) {
		throw new NotImplementedException();
	}

	public void SetReflectivity(in Vector3 vecReflectivity) {
		throw new NotImplementedException();
	}

	public Span<byte> SetResourceData(uint type, Span<byte> data) {
		throw new NotImplementedException();
	}

	public bool Unserialize(Stream stream, bool headerOnly = false, int skipMipLevels = 0) {
		throw new NotImplementedException();
	}

	public int Width() {
		throw new NotImplementedException();
	}
}
