using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.MaterialSystem;

public interface ITextureRegenerator : IDisposable
{
	void RegenerateTextureBits();
}

public interface ITexture : IDisposable
{
	ReadOnlySpan<char> GetName();
	int GetMappingWidth();
	int GetMappingHeight();
	int GetActualWidth();
	int GetActualHeight();
	int GetNumAnimationFrames();
	bool IsTranslucent();
	bool IsMipmapped();

	void GetLowResColorSample(float s, float t, Span<float> color);
	Span<byte> GetResourceData(uint type);

	void IncrementReferenceCount();
	void DecrementReferenceCount();

	void SetTextureGenerator(ITextureRegenerator textureRegen);

	// todo: enum for additionalCreationFlags, if applicable?
	void Download(out Rectangle rect, int additionalCreationFlags = 0);

	nint GetApproximateVidMemBytes();

	bool IsError();

	bool IsVolumeTexture();
	int GetMappingDepth();
	int GetActualDepth();

	ImageFormat GetImageFormat();
	NormalDecodeMode GetNormalDecodeMode();

	bool IsRenderTarget();
	bool IsCubeMap();
	bool IsNormalMap();
	bool IsProcedural();

	void SwapContents(ITexture other);

	/// <summary>
	/// Likely returns <see cref="CompiledVtfFlags"/>
	/// </summary>
	/// <returns></returns>
	int GetFlags();

	void ForceLODOverride(int numLodOverrideUpOrDown);

	bool SaveToFile(ReadOnlySpan<char> fileName);

	public static bool IsError([NotNullWhen(true)] ITexture? tex) => tex != null && !tex.IsError();
}