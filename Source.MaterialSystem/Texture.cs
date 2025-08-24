// TODO: Remove unused flags that aren't applicable in our use cases.
// (although this goal applies to the entire project, frankly)

using Source.Common;
using Source.Common.Bitmap;
using Source.Common.Filesystem;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Security.Principal;
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
	public ReadOnlySpan<char> GetTextureGroupName() => TextureGroupName;

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

	static ThreadLocal<IVTFTexture> VTFTextures = new(() => IVTFTexture.Create());

	private IVTFTexture? GetScratchVTFTexture() {
		return VTFTextures.Value;
	}

	private void ReleaseScratchVTFTexture(IVTFTexture scratchVTF) {

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
			nAdditionalFlags = (int)CompiledVtfFlags.StreamableCoarse;
			Assert((Flags & (long)CompiledVtfFlags.StreamableFine) == 0);
		}

		using ScratchVTF scratch = new(this);
		IVTFTexture vtfTexture = scratch.Get()!;

		// The texture name doubles as the relative file name
		// It's assumed to have already been set by this point	
		// Compute the cache name
		Span<char> cacheFileName = stackalloc char[MATERIAL_MAX_PATH];
		sprintf(cacheFileName, "materials/%s.vtf", Name);

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

	/// <summary>
	/// FrameCount and TextureHandles are closely associated
	/// todo: can we just make it an array in C# land with no FrameCount...?
	/// </summary>
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
		if (TextureHandles != null) {
			TextureHandles = null;
		}
	}

	private void SetErrorTexture(bool v) {
		throw new NotImplementedException();
	}

	private bool HasBeenAllocated() {
		return (InternalFlags & (int)InternalTextureFlags.Allocated) != 0;
	}

	public void Bind(in MaterialVarGPU hardwareTarget, int frame) {
		if (HasBeenAllocated()) {
			materials.ShaderAPI.BindTexture(in hardwareTarget, frame, TextureHandles![frame]);
		}
		else {
			Warning($"Tried to bind texture {GetName()}, but texture handles are not valid.\n");
		}
	}

	public void Download() {

	}


	public void Download(Rectangle rect, int additionalCreationFlags) {
		if (materials.ShaderAPI.CanDownloadTextures()) {
			Flags |= (uint)additionalCreationFlags;
			DownloadTexture(rect);
		}
	}

	private void DownloadTexture(Rectangle rect, bool copyFromCurrent = false) {
		if (!materials.ShaderDevice.IsUsingGraphics())
			return;

		if (rect == default) {
			ReconstructTexture(copyFromCurrent);
		}
		else {
			Assert(!copyFromCurrent);
			ReconstructPartialTexture(rect);
		}

		SetFilteringAndClampingMode();

		if (((InternalTextureFlags)InternalFlags & InternalTextureFlags.ShouldExclude) > 0)
			InternalFlags |= (uint)InternalTextureFlags.Excluded;
		else
			InternalFlags &= ~(uint)InternalTextureFlags.Excluded;

		ActualDimensionLimit = DesiredDimensionLimit;
	}

	private void SetFilteringAndClampingMode() {

	}

	private void ReconstructTexture(bool copyFromCurrent) {
		int oldWidth = DimsAllocated.Width;
		int oldHeight = DimsAllocated.Height;
		int oldDepth = DimsAllocated.Depth;
		int oldMipCount = DimsAllocated.MipCount;
		int oldFrameCount = FrameCount;

		string? resolvedFilename = null;
		IVTFTexture? vtfTexture = null;

		{
			if (IsProcedural()) {
				// This will call the installed texture bit regeneration interface
				vtfTexture = ReconstructProceduralBits();
			}
			else if (IsRenderTarget()) {
				// Compute the actual size + format based on the current mode
				bool ignorePicmip = RenderTargetSizeMode != RenderTargetSizeMode.LiteralPicmip;
				ComputeActualSize(ignorePicmip);
			}
			else if (copyFromCurrent) {
				ComputeActualSize(false, null, true);
			}
			else {
				NotifyUnloadedFile();

				Span<char> cacheFileName = stackalloc char[MATERIAL_MAX_PATH];
				GetCacheFilename(ref cacheFileName);

				// Get the data from disk...
				// NOTE: Reloading the texture bits can cause the texture size, frames, format, pretty much *anything* can change.
				vtfTexture = LoadTextureBitsFromFile(cacheFileName, out resolvedFilename);
			}
		}

		if (!HasBeenAllocated() ||
			DimsAllocated.Width != oldWidth || DimsAllocated.Height != oldHeight ||
			DimsAllocated.Depth != oldDepth || DimsAllocated.MipCount != oldMipCount ||
			FrameCount != oldFrameCount) {

			const bool canStretchRectTextures = true;
			bool shouldMigrateTextures = ((Flags & (uint)CompiledVtfFlags.StreamableFine) != 0) && FrameCount == oldFrameCount;

			// If we're just streaming in more data--or demoting ourselves, do a migration instead. 
			if (copyFromCurrent || (canStretchRectTextures && shouldMigrateTextures)) {
				MigrateShaderAPITextures();

				// Ahh--I feel terrible about this, but we genuinely don't need anything else if we're streaming.
				if (copyFromCurrent)
					return;
			}
			else {
				// If we're doing a wholesale copy, we need to restore these values that will be cleared by FreeShaderAPITextures.
				// Record them here, restore them below.
				uint restoreStreamingFlag = (Flags & (uint)CompiledVtfFlags.Streamable);
				ResidencyType restoreResidenceCurrent = ResidenceCurrent;
				ResidencyType restoreResidenceTarget = ResidenceTarget;

				if (HasBeenAllocated()) {
					FreeShaderAPITextures();
				}

				// Create the shader api textures
				if (!AllocateShaderAPITextures())
					return;

				// Restored once we successfully allocate the shader api textures, but only if we're 
				// 
				if (!canStretchRectTextures && shouldMigrateTextures) {
					Flags |= restoreStreamingFlag;
					ResidenceCurrent = restoreResidenceCurrent;
					ResidenceTarget = restoreResidenceTarget;
				}
			}
		}
		else if (copyFromCurrent) {
			AssertMsg(false, "We're about to crash, last chance to examine this texture.");
		}


		// Render Targets just need to be cleared, they have no upload
		if (IsRenderTarget()) {
			throw new NotImplementedException();
		}

		// Blit down the texture faces, frames, and mips into the board memory
		int firstFace, faceCount;
		GetDownloadFaceCount(out firstFace, out faceCount);

		WriteDataToShaderAPITexture(FrameCount, faceCount, firstFace, DimsActual.MipCount, vtfTexture, ImageFormat);

		ReleaseScratchVTFTexture(vtfTexture);
		vtfTexture = null;
	}

	private void WriteDataToShaderAPITexture(ushort frameCount, int faceCount, int firstFace, ushort mipCount, IVTFTexture? vtfTexture, ImageFormat imageFormat) {
		if ((Flags & (uint)CompiledVtfFlags.StagingMemory) > 0)
			return;

		for (int i = 0; i < FrameCount; i++) {
			Modify(i);
			materials.ShaderAPI.TexImageFromVTF(vtfTexture, i);
		}
	}

	private void Modify(int frame) {
		Assert(frame >= 0 && frame < FrameCount);
		Assert(HasBeenAllocated());
		materials.ShaderAPI.ModifyTexture(TextureHandles![frame]);
	}

	private void GetDownloadFaceCount(out int firstFace, out int faceCount) {
		faceCount = 1;
		firstFace = 0;
		if (IsCubeMap()) {
			if (materials.HardwareConfig.SupportsCubeMaps()) {
				faceCount = (int)CubeMapFaceIndex.Count - 1;
			}
			else {
				// This will cause us to use the spheremap instead of the cube faces
				// in the case where we don't support cubemaps
				firstFace = (int)CubeMapFaceIndex.Spheremap;
			}
		}
	}

	private bool AllocateShaderAPITextures() {
		int count = FrameCount;

		CreateTextureFlags createFlags = 0;
		if ((Flags & (uint)CompiledVtfFlags.EnvMap) > 0 && materials.HardwareConfig.SupportsCubeMaps()) {
			createFlags |= CreateTextureFlags.Cubemap;
		}

		bool bIsFloat = (ImageFormat == ImageFormat.RGBA16161616F) || (ImageFormat == ImageFormat.R32F) ||
						(ImageFormat == ImageFormat.RGB323232F) || (ImageFormat == ImageFormat.RGBA32323232F);

		// Don't do sRGB on floating point textures
		if ((Flags & (uint)CompiledVtfFlags.SRGB) > 0 && !bIsFloat) {
			createFlags |= CreateTextureFlags.SRGB;    // for Posix/GL only
		}

		if ((Flags & (uint)CompiledVtfFlags.RenderTarget) > 0) {
			createFlags |= CreateTextureFlags.RenderTarget;

			// This here is simply so we can use a different call to
			// create the depth texture below	
			if ((Flags & (uint)CompiledVtfFlags.DepthRenderTarget) > 0 && (count == 2)) //nCount must be 2 on pc
			{
				--count;
			}
		}
		else {
			// If it's not a render target, use the texture manager in dx
			if ((Flags & (uint)CompiledVtfFlags.StagingMemory) > 0)
				createFlags |= CreateTextureFlags.SysMem;
			else {
				createFlags |= CreateTextureFlags.Managed;
			}
		}

		if ((Flags & (uint)CompiledVtfFlags.PointSample) > 0) {
			createFlags |= CreateTextureFlags.UnfilterableOK;
		}

		if ((Flags & (uint)CompiledVtfFlags.VertexTexture) > 0) {
			createFlags |= CreateTextureFlags.VertexTexture;
		}

		int nCopies = 1;
		if (IsProcedural()) {
			// This is sort of hacky... should we store the # of copies in the VTF?
			if ((Flags & (uint)CompiledVtfFlags.SingleCopy) == 0) {
				// FIXME: That 6 there is heuristically what I came up with what I
				// need to get eyes not to stall on map alyx3. We need a better way
				// of determining how many copies of the texture we should store.
				nCopies = 6;
			}
		}

		// For depth only render target: adjust texture width/height
		// Currently we just leave it the same size, will update with further testing
		int shaderApiCreateTextureDepth = ((Flags & (uint)CompiledVtfFlags.DepthRenderTarget) != 0 && (OriginalRenderTargetType == RenderTargetType.OnlyDepth)) ? 1 : DimsAllocated.Depth;

		// Create all animated texture frames in a single call
		materials.ShaderAPI.CreateTextures(
			TextureHandles!, count,
			DimsAllocated.Width, DimsAllocated.Height, shaderApiCreateTextureDepth, ImageFormat, DimsAllocated.MipCount,
			nCopies, createFlags, GetName(), GetTextureGroupName());

		int accountingCount = count;

		// Create the depth render target buffer
		if ((Flags & (uint)CompiledVtfFlags.DepthRenderTarget) > 0) {
			Assert(count == 1);

			Span<char> debugName = stackalloc char[128];
			sprintf(debugName, "%s_ZBuffer", new string(GetName()));
			Assert(FrameCount >= 2);
			TextureHandles![1] = materials.ShaderAPI.CreateDepthTexture(
					ImageFormat,
					DimsAllocated.Width,
					DimsAllocated.Height,
					debugName,
					(OriginalRenderTargetType == RenderTargetType.OnlyDepth));
			accountingCount += 1;
		}

		InternalFlags |= (uint)InternalTextureFlags.Allocated;

		return true;
	}

	private void FreeShaderAPITextures() {
		if (TextureHandles != null && HasBeenAllocated()) {
			for (int i = FrameCount; --i >= 0;) {
				if (materials.ShaderAPI.IsTexture(TextureHandles[i])) {
					materials.ShaderAPI.DeleteTexture(TextureHandles[i]);
					TextureHandles[i] = INVALID_SHADERAPI_TEXTURE_HANDLE;
				}
			}
		}
	}

	private void MigrateShaderAPITextures() {
		throw new NotImplementedException();
	}

	TextureLODControlSettings CachedFileLodSettings;

	private IVTFTexture? LoadTextureBitsFromFile(Span<char> cacheFileName, out string? resolvedFilename) {
		resolvedFilename = null;

		IVTFTexture? vtfTexture = StreamingVTF;
		if (vtfTexture == null) {
			vtfTexture = GetScratchVTFTexture();

			IFileHandle? fileHandle = null;

			if (!GetFileHandle(out fileHandle, cacheFileName, out resolvedFilename))
				return HandleFileLoadFailedTexture(vtfTexture);

			vtfTexture!.Unserialize(fileHandle.Stream, false);

			fileHandle?.Dispose();
		}

		// TODO: How does Source stream textures?

		return vtfTexture;
	}

	private IVTFTexture? HandleFileLoadFailedTexture(IVTFTexture? vtfTexture) {
		throw new Exception("File load failed (time to implement HandleFileLoadFailedTexture, or something went horribly wrong)");
	}

	private bool GetFileHandle([NotNullWhen(true)] out IFileHandle? fileHandle, Span<char> cacheFileName, out string? resolvedFilename) {
		fileHandle = null;
		resolvedFilename = null; // Requires OpenEx; do later
		while (fileHandle == null) {
			fileHandle = materials.FileSystem.Open(cacheFileName, FileOpenOptions.Read | FileOpenOptions.Binary, materials.GetForcedTextureLoadPathID());
			if (fileHandle == null) {
				break; // TODO; fallbacks
			}
		}

		if (fileHandle == null) {
			DevWarning($"\"{cacheFileName}\": can't be found on disk\n");
			return false;
		}

		return true;
	}

	private void GetCacheFilename(ref Span<char> pCacheFileName) {
		int written = sprintf(pCacheFileName, "materials/%s.vtf", Name);
		pCacheFileName = pCacheFileName[..written];
	}

	private void NotifyUnloadedFile() {

	}

	private void ComputeActualSize(bool ignorePicmip = false, IVTFTexture? vtfTexture = null, bool textureMigration = false) {

	}

	private IVTFTexture? ReconstructProceduralBits() {
		throw new NotImplementedException();
	}

	private void ReconstructPartialTexture(Rectangle rect) {

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
	ushort StreamingMips;
	ushort OriginalRTWidth;
	ushort OriginalRTHeight;
	byte LowResImageWidth;
	byte LowResImageHeight;
	ushort DesiredDimensionLimit;
	ushort ActualDimensionLimit;

	IVTFTexture? StreamingVTF;
	ResidencyType ResidenceTarget;
	ResidencyType ResidenceCurrent;

	ShaderAPITextureHandle_t[]? TextureHandles;
	RenderTargetType OriginalRenderTargetType;
	RenderTargetSizeMode RenderTargetSizeMode;
}
