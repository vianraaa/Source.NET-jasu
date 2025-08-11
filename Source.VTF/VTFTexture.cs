using Source.Bitmap;
using Source.Common;
using Source.Common.Bitmap;

using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Source.VTF;

public sealed class VTFTexture : IVTFTexture
{
	readonly int[] Version = new int[2];
	int Width;
	int Height;
	int Depth;
	ImageFormat Format;
	
	int MipCount;
	int FaceCount;
	int FrameCount;

	int Flags;
	byte[]? ImageData;

	Vector3 Reflectivity;
	float BumpScale;

	int StartFrame;

	ImageFormat LowResImageFormat;
	int LowResImageWidth;
	int LowResImageHeight;
	byte[]? LowResImageData;

	float AlphaThreshold;
	float AlphaHiFreqThreshold;

	int FinestMipmapLevel;
	int CoarsestMipmapLevel;

	List<ResourceEntryInfo> ResourcesInfo = [];
	float IVTFTexture.BumpScale() => BumpScale;

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

	int IVTFTexture.Depth() => Depth;

	public void Dispose() {
		throw new NotImplementedException();
	}

	int IVTFTexture.FaceCount() => FaceCount;

	public int FaceSizeInBytes(int mipLevel) {
		throw new NotImplementedException();
	}

	public int FileSize(int mipSkipCount = 0) {
		throw new NotImplementedException();
	}

	int IVTFTexture.Flags() => Flags;

	ImageFormat IVTFTexture.Format() => Format;

	int IVTFTexture.FrameCount() => FrameCount;

	public void GenerateMipmaps() {
		throw new NotImplementedException();
	}

	public Span<byte> GetResourceData(uint type) {
		throw new NotImplementedException();
	}

	public bool HasResourceEntry(uint type) {
		throw new NotImplementedException();
	}

	int IVTFTexture.Height() => Height;

	Span<byte> IVTFTexture.ImageData() {
		throw new NotImplementedException();
	}

	Span<byte> IVTFTexture.ImageData(int frame, int face, int mipLevel) {
		throw new NotImplementedException();
	}

	Span<byte> IVTFTexture.ImageData(int frame, int face, int mipLevel, int x, int y, int z = 0) {
		throw new NotImplementedException();
	}

	private ResourceEntryInfo? FindResourceEntryInfo(ResourceEntryType type) {
		return null; // ??????
	}

	public void ImageFileInfo(int frame, int face, int mipLevel, out int startLocation, out int sizeInBytes) {
		int i, mipWidth, mipHeight, mipDepth;

		ResourceEntryInfo? pInfo = FindResourceEntryInfo(ResourceEntryType.HighResImageData);

		if (!pInfo.HasValue) {
			Dbg.Assert(false);
			startLocation = 0;
			sizeInBytes = 0;
			return;
		}

		ResourceEntryInfo imageDataInfo = pInfo.Value;

		int offset = (int)imageDataInfo.Offset;
		for (i = MipCount - 1; i > mipLevel; --i) {
			ComputeMipLevelDimensions(i, out mipWidth, out mipHeight, out mipDepth);
			int mipLevelSize = ImageLoader.GetMemRequired(mipWidth, mipHeight, mipDepth, Format, false);
			offset += mipLevelSize;
		}

		ComputeMipLevelDimensions(mipLevel, out mipWidth, out mipHeight, out mipDepth);
		int faceSize = ImageLoader.GetMemRequired(mipWidth, mipHeight, mipDepth, Format, false);

		int facesToRead = FaceCount;
		if (IsCubeMap()) {
			if (Version[0] == 7 && Version[1] < 1) {
				facesToRead = 6;
				if (face == (int)CubeMapFaceIndex.Spheremap)
					face--;
			}
		}

		int framesize = facesToRead * faceSize;
		offset += framesize * frame;

		offset += face * faceSize;

		startLocation = offset;
		sizeInBytes = faceSize;
	}

	public bool Init(int width, int height, int depth, ImageFormat format, int flags, int frameCount, int forceMipCount = -1) {
		throw new NotImplementedException();
	}

	public void InitLowResImage(int width, int height, ImageFormat format) {
		throw new NotImplementedException();
	}

	public bool IsCubeMap() => ((CompiledVtfFlags)Flags & CompiledVtfFlags.EnvMap) == CompiledVtfFlags.EnvMap;
	public bool IsNormalMap() => ((CompiledVtfFlags)Flags & CompiledVtfFlags.Normal) == CompiledVtfFlags.Normal;

	public bool IsVolumeTexture() => Depth > 1;

	public void LowResFileInfo(out int startLocation, out int sizeInBytes) {
		throw new NotImplementedException();
	}

	public ImageFormat LowResFormat() => LowResImageFormat;

	Span<byte> IVTFTexture.LowResImageData() {
		throw new NotImplementedException();
	}

	public int LowResWidth() => LowResImageWidth;

	public int LowResHeight() => LowResImageHeight;

	int IVTFTexture.MipCount() => MipCount;

	Vector3 IVTFTexture.Reflectivity() => Reflectivity;

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
		return UnserializeEx(stream, headerOnly, 0, skipMipLevels);
	}

	public bool UnserializeEx(Stream stream, bool headerOnly, int forceFlags, int skipMipLevels) {
		VTFFileHeader header;
		if (!ReadHeader(stream, out header))
			return false;

		header.Flags |= (uint)forceFlags;
		var flags = (CompiledVtfFlags)header.Flags;


		if ((flags & CompiledVtfFlags.EnvMap) == CompiledVtfFlags.EnvMap && header.Width != header.Height) {
			Dbg.Warning("*** Encountered VTF non-square cubemap!\n");
			return false;
		}
		if ((flags & CompiledVtfFlags.EnvMap) == CompiledVtfFlags.EnvMap && header.Depth != 1) {
			Dbg.Warning("*** Encountered VTF volume texture cubemap!\n");
			return false;
		}
		if (header.Width <= 0 || header.Height <= 0 || header.Depth <= 0) {
			Dbg.Warning("*** Encountered VTF invalid texture size!\n");
			return false;
		}
		if(header.ImageFormat < ImageFormat.Unknown || header.ImageFormat >= ImageFormat.Count) {
			Dbg.Warning("*** Encountered VTF invalid image format!\n");
			return false;
		}

		Width = header.Width;
		Height = header.Height;
		Depth = header.Depth;
		Format = header.ImageFormat;
		Flags = (int)header.Flags;
		FrameCount = header.NumFrames;

		FaceCount = (Flags & (int)CompiledVtfFlags.EnvMap) == (int)CompiledVtfFlags.EnvMap ? (int)CubeMapFaceIndex.Count : 1;
		MipCount = ComputeMipCount();

		FinestMipmapLevel = 0;
		CoarsestMipmapLevel = MipCount - 1;

		Reflectivity = header.Reflectivity;
		BumpScale = header.BumpScale;

		StartFrame = header.StartFrame;

		Version[0] = header.Version[0];
		Version[1] = header.Version[1];

		if(header.LowResImageWidth == 0 || header.LowResImageHeight == 0) {
			LowResImageWidth = LowResImageHeight = 0;
		}
		else {
			LowResImageWidth = header.LowResImageWidth;
			LowResImageHeight = header.LowResImageHeight;
		}
		LowResImageFormat = header.LowResImageFormat;

		if (LowResImageFormat < ImageFormat.Unknown || LowResImageFormat >= ImageFormat.Count)
			return false;

		// todo: read resources. I have the code for this somewhere...

		// Caller wants the header component only
		if (headerOnly)
			return true;

		return true;
	}

	private int ComputeMipCount() {
		return ImageLoader.GetNumMipMapLevels(Width, Height, Depth);
	}

	private static readonly sbyte[] VTF0 = [86, 84, 70, 0]; // VTF\0
	private unsafe bool ReadHeader(Stream stream, out VTFFileHeader header) {
		using BinaryReader reader = new(stream);
		header = new();

		reader.ReadInto<sbyte>(header.FileTypeString);
		reader.ReadInto<int>(header.Version);
		reader.ReadInto(ref header.HeaderSize);

		if (!header.FileTypeString.SequenceEqual(VTF0)) {
			Dbg.Warning("*** Tried to load a non-VTF file as a VTF file!\n");
			return false;
		}

		if (header.Version[0] != IVTFTexture.VTF_MAJOR_VERSION) {
			Dbg.Warning("*** Encountered VTF file with an invalid version!\n");
			return false;
		}

		if (!ReaderHeaderFromBufferPastBaseHeader(reader, header)) {
			Dbg.Warning("*** Encountered VTF file with an invalid full header!\n");
			return false;
		}

		switch (header.Version[1]) {
			case 0:
			case 1:
				header.Depth = 1;
				goto case 2;
			case 2:
				header.NumResources = 0;
				goto case 3;
			case 3:
				header.Flags &= (uint)VersionedVtfFlags.Mask_7_3;
				goto case IVTFTexture.VTF_MINOR_VERSION;
			case IVTFTexture.VTF_MINOR_VERSION:
			case 5:

				break;
		}

		return true;
	}

	private static bool ReadV0(BinaryReader reader, VTFFileBaseHeader header) {
		// Nothing here to do
		return reader.PeekChar() != -1;
	}
	private static bool ReadV1(BinaryReader reader, VTFFileHeaderV7_1 header) {
		reader.ReadInto(ref header.Width);
		reader.ReadInto(ref header.Height);
		reader.ReadInto(ref header.Flags);
		reader.ReadInto(ref header.NumFrames);
		reader.ReadInto(ref header.StartFrame);
		reader.ReadNothing(4); // << what are these?
		reader.ReadInto(ref header.Reflectivity);
		reader.ReadNothing(4); // << what are these?
		reader.ReadInto(ref header.BumpScale);
		reader.ReadInto(ref header.ImageFormat);
		reader.ReadInto(ref header.NumMipLevels);
		reader.ReadInto(ref header.LowResImageFormat);
		reader.ReadInto(ref header.LowResImageWidth);
		reader.ReadInto(ref header.LowResImageHeight);
		return reader.PeekChar() != -1;
	}
	private static bool ReadV2(BinaryReader reader, VTFFileHeaderV7_2 header) {
		reader.ReadInto(ref header.Depth);
		return reader.PeekChar() != -1;
	}
	private static bool ReadV3(BinaryReader reader, VTFFileHeaderV7_3 header) {
		reader.ReadInto<sbyte>(header.Pad4);
		reader.ReadInto(ref header.NumResources);
		return reader.PeekChar() != -1;
	}
	private static bool ReadV4(BinaryReader reader, VTFFileHeader header) {
		return reader.PeekChar() != -1;
	}
	private static bool ReadV5(BinaryReader reader, VTFFileHeader header) {
		return reader.PeekChar() != -1;
	}


	private static bool ReaderHeaderFromBufferPastBaseHeader(BinaryReader reader, VTFFileHeader header) {
		switch (header.Version[1]) {
			case 0:
				if (!ReadV0(reader, header)) return false;
				return true;
			case 1:
				if (!ReadV0(reader, header)) return false;
				if (!ReadV1(reader, header)) return false;
				return true;
			case 2:
				if (!ReadV0(reader, header)) return false;
				if (!ReadV1(reader, header)) return false;
				if (!ReadV2(reader, header)) return false;
				return true;
			case 3:
				if (!ReadV0(reader, header)) return false;
				if (!ReadV1(reader, header)) return false;
				if (!ReadV2(reader, header)) return false;
				if (!ReadV3(reader, header)) return false;
				return true;
			case 4:
				if (!ReadV0(reader, header)) return false;
				if (!ReadV1(reader, header)) return false;
				if (!ReadV2(reader, header)) return false;
				if (!ReadV3(reader, header)) return false;
				if (!ReadV4(reader, header)) return false;
				return true;
			case 5:
				if (!ReadV0(reader, header)) return false;
				if (!ReadV1(reader, header)) return false;
				if (!ReadV2(reader, header)) return false;
				if (!ReadV3(reader, header)) return false;
				if (!ReadV4(reader, header)) return false;
				if (!ReadV5(reader, header)) return false;
				return true;
			default:
				Dbg.Warning("*** Encountered VTF file with an invalid minor version!\n");
				return false;
		}
	}

	int IVTFTexture.Width() => Width;
}
