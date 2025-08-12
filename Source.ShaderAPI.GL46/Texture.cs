using Source.Common;
using Source.Common.Bitmap;
using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Source.MaterialSystem;

public enum InternalTextureFlags
{
	Error = 0x00000001,
	Allocated = 0x00000002,
	Excluded = 0x00000020, // actual exclusion state
	ShouldExclude = 0x00000040, // desired exclusion state
}
public struct TexDimensions {
	public ushort Width;
	public ushort Height;
	public ushort MipCount;
	public ushort Depth;

	public TexDimensions(ushort width = 0, ushort height = 0, ushort mips = 0, ushort depth = 1) {
		Width = width;
		Height = height;
		MipCount = mips;
		Depth = depth;
	}
}

public class Texture : ITextureInternal
{
	public void DecrementReferenceCount() {
		throw new NotImplementedException();
	}

	public void Dispose() {
		throw new NotImplementedException();
	}

	public void Download(out Rectangle rect, int additionalCreationFlags = 0) {
		throw new NotImplementedException();
	}

	public void ForceLODOverride(int numLodOverrideUpOrDown) {
		throw new NotImplementedException();
	}

	public int GetActualDepth() => DimsActual.Depth;

	public int GetActualHeight() => DimsActual.Height;
	public int GetActualWidth() => DimsActual.Width;

	public nint GetApproximateVidMemBytes() {
		throw new NotImplementedException();
	}

	public int GetFlags() => (int)Flags;
	public ImageFormat GetImageFormat() => ImageFormat;

	public void GetLowResColorSample(float s, float t, Span<float> color) {
		throw new NotImplementedException();
	}

	public int GetMappingDepth() => DimsMapping.Height;

	public int GetMappingHeight() => DimsMapping.Height;

	public int GetMappingWidth() => DimsMapping.Height;

	public ReadOnlySpan<char> GetName() => Name;

	public NormalDecodeMode GetNormalDecodeMode() {
		throw new NotImplementedException();
	}

	public int GetNumAnimationFrames() {
		throw new NotImplementedException();
	}

	public Span<byte> GetResourceData(uint type) {
		throw new NotImplementedException();
	}

	public void IncrementReferenceCount() {
		throw new NotImplementedException();
	}

	public bool IsCubeMap() => ((CompiledVtfFlags)Flags & CompiledVtfFlags.EnvMap) != 0;
	public bool IsError() => ((InternalTextureFlags)InternalFlags & InternalTextureFlags.Error) != 0;
	public bool IsMipmapped() => ((CompiledVtfFlags)Flags & CompiledVtfFlags.NoMip) == 0;
	public bool IsNormalMap() => ((CompiledVtfFlags)Flags & CompiledVtfFlags.Normal) != 0;
	public bool IsProcedural() => ((CompiledVtfFlags)Flags & CompiledVtfFlags.Procedural) != 0;
	public bool IsRenderTarget() => ((CompiledVtfFlags)Flags & CompiledVtfFlags.RenderTarget) != 0;
	public bool IsTranslucent() => ((((CompiledVtfFlags)Flags) & CompiledVtfFlags.OneBitAlpha) | (((CompiledVtfFlags)Flags) & CompiledVtfFlags.EightBitAlpha)) != 0;
	public bool IsVolumeTexture() => DimsMapping.Depth > 1;

	public bool SaveToFile(ReadOnlySpan<char> fileName) {
		throw new NotImplementedException();
	}

	public void SetTextureGenerator(ITextureRegenerator textureRegen) {
		throw new NotImplementedException();
	}

	public void SwapContents(ITexture other) {
		throw new NotImplementedException();
	}

	internal void InitFileTexture(ReadOnlySpan<char> fileName, ReadOnlySpan<char> textureGroupName) {
		SetName(fileName);
		TextureGroupName = new(textureGroupName);
	}

	private void SetName(ReadOnlySpan<char> fileName) {
		Name = ITextureInternal.NormalizeTextureName(fileName);
	}

	Vector3 Reflectivity;
	string Name;
	string TextureGroupName;
	uint Flags;
	uint InternalFlags;
	// int refCount 
	ImageFormat ImageFormat;

	TexDimensions DimsMapping;
	TexDimensions DimsActual;
	TexDimensions DimsAllocated;

	ushort FrameCount;
	ushort OriginalRTWidth;
	ushort OriginalRTHeight;
	byte LowResImageWidth;
	byte LowResImageHeight;
	ushort DesiredDimensionLimit;
	ushort ActualDimensionLimit;


}
