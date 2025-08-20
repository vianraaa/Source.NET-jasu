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
using System.Xml.Linq;

namespace Source.MaterialSystem;

public enum InternalTextureFlags
{
	Error = 0x00000001,
	Allocated = 0x00000002,
	Excluded = 0x00000020, // actual exclusion state
	ShouldExclude = 0x00000040, // desired exclusion state
}
public struct TexDimensions
{
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

public class Texture(MaterialSystem materials) : ITextureInternal
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

	public ref struct ScratchVTF
	{
		readonly Texture _parent;
		IVTFTexture? scratchVTF;

		public ScratchVTF(Texture tex) {
			_parent = tex;
			scratchVTF = tex.GetScratchVTFTexture();
		}

		public readonly IVTFTexture? Get() => scratchVTF;
		public void TakeOwnership() {
			scratchVTF = null;
		}

		public void Dispose() {
			if (scratchVTF != null)
				_parent.ReleaseScratchVTFTexture(scratchVTF);
			scratchVTF = null;
		}
	}


	private IVTFTexture? GetScratchVTFTexture() {
		throw new NotImplementedException();
	}

	private void ReleaseScratchVTFTexture(IVTFTexture scratchVTF) {
		throw new NotImplementedException();
	}


	public void Precache() {
		if (IsRenderTarget() || IsProcedural())
			return;

		if (HasBeenAllocated())
			return;

		if (Name.Equals("env_cubemap", StringComparison.InvariantCulture))
			return;

		int nAdditionalFlags = 0;
		if ((Flags & (uint)CompiledVtfFlags.Streamable) != 0) {
			// If we were previously streamed in, make sure we still do this time around.
			nAdditionalFlags = (int)CompiledVtfFlags.StreamableCourse;
			Assert((Flags & (long)CompiledVtfFlags.StreamableFine) == 0);
		}

		using ScratchVTF scratch = new(this);
		IVTFTexture vtfTexture = scratch.Get()!;

		// The texture name doubles as the relative file name
		// It's assumed to have already been set by this point	
		// Compute the cache name
		Span<char> cacheFileName = stackalloc char[MATERIAL_MAX_PATH];
		cacheFileName.Print("materials/%s.vtf", Name);

		ushort nHeaderSize = IVTFTexture.FileHeaderSize(IVTFTexture.VTF_MAJOR_VERSION);
		Span<byte> mem = stackalloc byte[nHeaderSize];
		if (!materials.FileSystem.ReadFile(cacheFileName, null, mem, nHeaderSize)) {
			goto precacheFailed;
		}
		unsafe {
			fixed (byte* pMem = mem) {
				using UnmanagedMemoryStream vtfStream = new UnmanagedMemoryStream(pMem, mem.Length);
				if (!vtfTexture.Unserialize(vtfStream, true)) {
					Warning($"Error reading material \"{cacheFileName}\"\n");
					goto precacheFailed;
				}

				// NOTE: Don't set the image format in case graphics are active
				Reflectivity = vtfTexture.Reflectivity();
				DimsMapping.Width = (ushort)vtfTexture.Width();
				DimsMapping.Height = (ushort)vtfTexture.Height();
				DimsMapping.Depth = (ushort)vtfTexture.Depth();
				Flags = (uint)(vtfTexture.Flags() | nAdditionalFlags);
				FrameCount = (ushort)vtfTexture.FrameCount();
				if (TextureHandles == null) {
					// NOTE: m_nFrameCount and m_pTextureHandles are strongly associated
					// whenever one is modified the other must also be modified
					AllocateTextureHandles();
				}

				return;
			}
		}

	precacheFailed:
		Reflectivity = new(0, 0, 0);
		DimsMapping.Width = 32;
		DimsMapping.Height = 32;
		DimsMapping.Depth = 1;
		Flags = (uint)CompiledVtfFlags.NoMip;
		SetErrorTexture(true);
		FrameCount = 1;
		if (TextureHandles == null) 
			AllocateTextureHandles();
		
	}

	private void AllocateTextureHandles() {
		Assert(TextureHandles == null);
		Assert(FrameCount > 0);
		if ((Flags & (uint)CompiledVtfFlags.DepthRenderTarget) != 0)
			Assert(FrameCount >= 2);

		TextureHandles = new ShaderAPITextureHandle_t[FrameCount];
		for (int i = 0; i < FrameCount; i++) 
			TextureHandles[i] = INVALID_SHADERAPI_TEXTURE_HANDLE;
	}
	private void ReleaseTextureHandles() {
		if(TextureHandles != null) {
			TextureHandles = null;
		}
	}

	private void SetErrorTexture(bool v) {
		throw new NotImplementedException();
	}

	private bool HasBeenAllocated() {
		return (InternalFlags & (int)InternalTextureFlags.Allocated) != 0;
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
	ShaderAPITextureHandle_t[]? TextureHandles;


}
