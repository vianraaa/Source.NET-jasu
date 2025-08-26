using Source.Bitmap;
using Source.Common;
using Source.Common.Bitmap;
using Source.Common.Engine;

using System.Buffers;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
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

	List<ResourceEntryInfo> Resources = [];
	float IVTFTexture.BumpScale() => BumpScale;

	public void ComputeAlphaFlags() {
		throw new NotImplementedException();
	}

	public int ComputeFaceSize(int startingMipLevel = 0) {
		return ComputeFaceSize(startingMipLevel, Format);
	}

	public int ComputeFaceSize(int startingMipLevel, ImageFormat format) {
		int size = 0;
		int w = Width;
		int h = Height;
		int d = Depth;

		for (int i = 0; i < MipCount; ++i) {
			if (i >= startingMipLevel) {
				size += ImageLoader.GetMemRequired(w, h, d, format, false);
			}
			w >>= 1;
			h >>= 1;
			d >>= 1;
			if (w < 1) {
				w = 1;
			}
			if (h < 1) {
				h = 1;
			}
			if (d < 1) {
				d = 1;
			}
		}
		return size;
	}

	public void ComputeMipLevelDimensions(int level, out int width, out int height, out int depth) {
		width = Width >> level;
		height = Height >> level;
		depth = Depth >> level;

		if (width < 1)
			width = 1;
		if (height < 1)
			height = 1;
		if (depth < 1)
			depth = 1;
	}

	public void ComputeMipLevelSubRect(Rectangle srcRect, int mipLevel, out Rectangle subRect) {
		throw new NotImplementedException();
	}

	public int ComputeMipSize(int mipLevel) {
		return ComputeMipSize(mipLevel, Format);
	}

	public int ComputeMipSize(int mipLevel, ImageFormat format) {
		ComputeMipLevelDimensions(mipLevel, out int w, out int h, out int d);
		return ImageLoader.GetMemRequired(w, h, d, Format, false);
	}

	public void ComputeReflectivity() {
		throw new NotImplementedException();
	}

	public int ComputeTotalSize() {
		return ComputeTotalSize(Format);
	}

	public int ComputeTotalSize(ImageFormat format) {
		nint memRequired = ComputeFaceSize(0, format);
		return (int)(FaceCount * FrameCount * memRequired);
	}

	public void ConstructLowResImage() {
		throw new NotImplementedException();
	}

	public void ConvertImageFormat(ImageFormat fmt, bool normalToDUDV) {
		if (ImageData == null)
			return;

		nint convertedSize = ComputeTotalSize(fmt);
		byte[] convertedImage = ArrayPool<byte>.Shared.Rent((int)convertedSize);

		for (int mip = 0; mip < MipCount; mip++) {
			ComputeMipLevelDimensions(mip, out int width, out int height, out int depth);
			nint srcFaceStride = ImageLoader.GetMemRequired(width, height, 1, Format, false);
			nint dstFaceStride = ImageLoader.GetMemRequired(width, height, 1, fmt, false);
			for (int frame = 0; frame < FrameCount; frame++) {
				for (int face = 0; face < FaceCount; face++) {
					Span<byte> srcData = GetImageData(frame, face, mip);
					Span<byte> dstData = convertedImage.AsSpan()[(int)GetImageOffset(frame, face, mip, fmt)..];
					for (int z = 0; z < depth; ++z, srcData = srcData[(int)srcFaceStride..], dstData = dstData[(int)dstFaceStride..]) {
						if (normalToDUDV) {
							Error("No\n");
							return;
						}
						else {
							ImageLoader.ConvertImageFormat(srcData, Format, dstData, fmt, width, height);
						}
					}
				}
			}
		}

		memcpy<byte>(ImageData, convertedImage[..(int)convertedSize]);
		Format = fmt;

		if (!ImageLoader.IsCompressed(fmt)) {
			ref ImageFormatInfo info = ref ImageLoader.ImageFormatInfo(fmt);
			int alphaBits = info.AlphaBits;
			if (alphaBits > 1) {
				Flags |= (int)(CompiledVtfFlags.EightBitAlpha);
				Flags &= ~(int)(CompiledVtfFlags.OneBitAlpha);
			}
			if (alphaBits <= 1) {
				Flags &= ~(int)(CompiledVtfFlags.EightBitAlpha);
				if (alphaBits == 0) {
					Flags &= ~(int)(CompiledVtfFlags.OneBitAlpha);
				}
			}
		}
		else {
			if ((fmt == ImageFormat.DXT5) || (fmt == ImageFormat.ATI2N) || (fmt == ImageFormat.ATI1N)) {
				Flags &= ~(int)(CompiledVtfFlags.OneBitAlpha | CompiledVtfFlags.EightBitAlpha);
			}
		}

		ArrayPool<byte>.Shared.Return(convertedImage, true);
	}

	int IVTFTexture.Depth() => Depth;

	public void Dispose() {

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
		return GetResourceData((ResourceEntryType)type);
	}

	private Span<byte> GetResourceData(ResourceEntryType type) {
		ref ResourceEntryInfo info = ref FindResourceEntryInfo(type);
		if(!Unsafe.IsNullRef(ref info)) {
			return ImageData!.AsSpan()[(int)info.Offset..];
		}

		return null;
	}

	public bool HasResourceEntry(uint type) {
		throw new NotImplementedException();
	}

	int IVTFTexture.Height() => Height;

	Span<byte> IVTFTexture.ImageData() {
		return GetImageData(0, 0, 0);
	}

	Span<byte> IVTFTexture.ImageData(int frame, int face, int mipLevel) {
		return GetImageData(frame, face, mipLevel);
	}

	public nint GetImageOffset(int frame, int face, int mipLevel, ImageFormat format) {
		Assert(frame < FrameCount);
		Assert(face < FaceCount);
		Assert(mipLevel < MipCount);

		int i;
		nint iOffset = 0;

		int iFaceSize = ComputeFaceSize(0, format);
		iOffset = frame * FaceCount * iFaceSize;

		// Get to the right face
		iOffset += face * iFaceSize;

		// Get to the right mip level
		for (i = 0; i < mipLevel; ++i) {
			iOffset += ComputeMipSize(i, format);
		}

		return iOffset;
	}

	Span<byte> IVTFTexture.ImageData(int frame, int face, int mipLevel, int x, int y, int z = 0) {
		throw new NotImplementedException();
	}


	private ref ResourceEntryInfo FindOrCreateResourceEntryInfo(ResourceEntryType type) {
		for (int i = 0, c = Resources.Count; i < c; i++) {
			ref ResourceEntryInfo searchResource = ref CollectionsMarshal.AsSpan(Resources)[i];
			if (searchResource.Tag == type)
				return ref searchResource;
		}

		Resources.Add(new() {
			Tag = type
		});
		ref ResourceEntryInfo newResource = ref CollectionsMarshal.AsSpan(Resources)[Resources.Count - 1];
		return ref newResource;
	}

	private ref ResourceEntryInfo FindResourceEntryInfo(ResourceEntryType type) {
		for (int i = 0, c = Resources.Count; i < c; i++) {
			ref ResourceEntryInfo searchResource = ref CollectionsMarshal.AsSpan(Resources)[i];
			if (searchResource.Tag == type)
				return ref searchResource;
		}

		return ref Unsafe.NullRef<ResourceEntryInfo>();
	}

	public void ImageFileInfo(int frame, int face, int mipLevel, out int startLocation, out int sizeInBytes) {
		int i, mipWidth, mipHeight, mipDepth;

		ref ResourceEntryInfo imageDataInfo = ref FindResourceEntryInfo(ResourceEntryType.HighResImageData);

		if (Unsafe.IsNullRef(ref imageDataInfo)) {
			Dbg.Assert(false);
			startLocation = 0;
			sizeInBytes = 0;
			return;
		}

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
		if (header.ImageFormat < ImageFormat.Unknown || header.ImageFormat >= ImageFormat.Count) {
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

		if (header.LowResImageWidth == 0 || header.LowResImageHeight == 0) {
			LowResImageWidth = LowResImageHeight = 0;
		}
		else {
			LowResImageWidth = header.LowResImageWidth;
			LowResImageHeight = header.LowResImageHeight;
		}
		LowResImageFormat = header.LowResImageFormat;

		if (LowResImageFormat < ImageFormat.Unknown || LowResImageFormat >= ImageFormat.Count)
			return false;

		if (header.NumResources > 0) {
			Resources = new((int)header.NumResources);
			long curStreamPos = stream.Position;
			stream.Seek(header.ResourcesOffset, SeekOrigin.Begin);
			using (BinaryReader reader = new(stream, Encoding.ASCII, true)) {
				reader.ReadNothing(8); // what is this?
				for (int i = 0; i < header.NumResources; i++) {
					Resources.Add(new());
					ref ResourceEntryInfo resource = ref CollectionsMarshal.AsSpan(Resources)[i];

					byte b1 = reader.ReadByte(), b2 = reader.ReadByte(), b3 = reader.ReadByte();
					resource.Tag = ResourceEntryInfo.ParseTag(b1, b2, b3);
					resource.Flags = reader.ReadByte();
					resource.Offset = reader.ReadUInt32();
				}
			}
			stream.Seek(curStreamPos, SeekOrigin.Begin);
		}
		else {
			// VTF 7.0 -> 7.2 does not have resource entries
			// have to write our own based on the old offsets
			int lowResImageSize = ImageLoader.GetMemRequired(LowResImageWidth, LowResImageHeight, 1, LowResImageFormat, false);
			if (lowResImageSize > 0) {
				ref ResourceEntryInfo reiLq = ref FindOrCreateResourceEntryInfo(ResourceEntryType.LowResThumbnail);
				reiLq.Offset = (uint)stream.Position;
			}

			ref ResourceEntryInfo reiHq = ref FindOrCreateResourceEntryInfo(ResourceEntryType.HighResImageData);
			reiHq.Offset = (uint)(lowResImageSize + stream.Position);
		}
		// Caller wants the header component only
		if (headerOnly)
			return true;

		ref ResourceEntryInfo lowResDataInfo = ref FindResourceEntryInfo(ResourceEntryType.LowResThumbnail);
		if (!Unsafe.IsNullRef(ref lowResDataInfo)) {
			stream.Seek(lowResDataInfo.Offset, SeekOrigin.Begin);
			if (!LoadLowResData(stream))
				return false;
		}

		// TODO: LoadNewResources

		ref ResourceEntryInfo imageDataInfo = ref FindResourceEntryInfo(ResourceEntryType.HighResImageData);
		if (!Unsafe.IsNullRef(ref imageDataInfo)) {
			stream.Seek(imageDataInfo.Offset, SeekOrigin.Begin);
			if (!LoadImageData(stream, header, skipMipLevels))
				return false;
		}
		else
			return false;

		return true;
	}

	private bool LoadImageData(Stream stream, VTFFileHeader header, int skipMipLevels) {
		if(skipMipLevels > 0) {
			if(header.NumMipLevels < skipMipLevels) {
				Warning("Warning! Encountered old format VTF file; please rebuild it!\n");
				return false;
			}

			ComputeMipLevelDimensions(skipMipLevels, out Width, out Height, out Depth);
			MipCount -= skipMipLevels;
		}

		nint imageSize = ComputeFaceSize();
		imageSize *= FaceCount * FrameCount;

		int facesToRead = FaceCount;
		if (IsCubeMap())
			throw new NotImplementedException("No cubemap support yet");

		if (!AllocateImageData(imageSize))
			return false;

		bool mipDataPresent = true;
		int firstAvailableMip = 0;
		int lastAvailableMip = MipCount - 1;

		Span<byte> data = GetResourceData(ResourceEntryType.TextureLOD);
		if (data != null) {
			ref TextureStreamSettings_t streamSettings = ref MemoryMarshal.Cast<byte, TextureStreamSettings_t>(data)[0];
			firstAvailableMip = Math.Max(0, streamSettings.FirstAvailableMip - skipMipLevels);
			lastAvailableMip = Math.Max(0, streamSettings.LastAvailableMip - skipMipLevels);
			mipDataPresent = false;
		}

		// Not doing streamable textures right now

		Assert(firstAvailableMip >= 0 && firstAvailableMip <= lastAvailableMip && lastAvailableMip < MipCount);

		FinestMipmapLevel = firstAvailableMip;
		CoarsestMipmapLevel = lastAvailableMip;

		for (int mip = MipCount; --mip >= 0;) {
			if (header.NumMipLevels - skipMipLevels <= mip)
				continue;

			int mipSize = ComputeMipSize(mip);

			if (mip > lastAvailableMip || mip < firstAvailableMip) {
				if (mipDataPresent)
					for (int frame = 0; frame < FrameCount; frame++)
						for (int face = 0; face < facesToRead; face++)
							stream.Seek(mipSize, SeekOrigin.Current);
				continue;
			}

			for (int frame = 0; frame < FrameCount; frame++) {
				for (int face = 0; face < facesToRead; face++) {
					Span<byte> mipBits = GetImageData(frame, face, mip);
					int bytesRead = stream.Read(mipBits);
					Debug.Assert(mipBits.Length == bytesRead);
				}
			}
		}

		return stream.Position <= stream.Length;
	}

	private Span<byte> GetImageData(int frame, int face, int mip) {
		Assert(ImageData != null);
		nint offset = GetImageOffset(frame, face, mip, Format);
		nint size = ComputeMipSize(mip, Format);
		return ImageData.AsSpan()[(int)offset..(int)(offset + size)];
	}

	private bool AllocateImageData(nint imageSize) {
		return GenericAllocateReusableData(ref ImageData, imageSize);
	}

	private static bool GenericAllocateReusableData(ref byte[]? imageData, nint numRequested) {
		imageData = new byte[numRequested];
		return true;
	}

	private bool LoadLowResData(Stream stream) {
		return true; // not reading that right now
	}

	private int ComputeMipCount() {
		return ImageLoader.GetNumMipMapLevels(Width, Height, Depth);
	}

	private static readonly sbyte[] VTF0 = [86, 84, 70, 0]; // VTF\0
	private unsafe bool ReadHeader(Stream stream, out VTFFileHeader header) {
		using BinaryReader reader = new(stream, Encoding.ASCII, true);
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
		header.ResourcesOffset = reader.BaseStream.Position;
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

	public uint GetResourceTypes(Span<ResourceEntryType> arrRsrcTypes) {
		if (arrRsrcTypes != null) 
			for (int i = 0; i < Math.Min(arrRsrcTypes.Length, Resources.Count); i++) 
				arrRsrcTypes[i] = Resources[i].Tag;

		return (uint)Resources.Count;
	}
}
