using Source.Common.Bitmap;

using System.Drawing;
using System;
using System.Runtime.CompilerServices;

namespace Source.Common.MaterialSystem;

public unsafe struct PixelWriterState
{
	public ImageFormat Format;
	public int Bits;
	public byte Size;
	public int BytesPerRow;
	public byte Flags;
	public short RShift;
	public short GShift;
	public short BShift;
	public short AShift;
	public uint RMask;
	public uint GMask;
	public uint BMask;
	public uint AMask;

	internal void SetFormat(ImageFormat format, uint rowSize) {
		throw new NotImplementedException();
	}
}

public ref struct PixelWriter 
{
	Span<byte> Base;
	PixelWriterState State;

	/// <summary>
	/// Allows reuse of the internal state structure if we lose control of the pixel writer ref struct
	/// </summary>
	/// <param name="state"></param>
	public readonly void Export(out PixelWriterState state, out Span<byte> memory) {
		state = State;
		memory = Base;
	}


	/// <summary>
	/// Allows reuse of the internal state structure if we lose control of the pixel writer ref struct
	/// </summary>
	/// <param name="state"></param>
	public void Import(in PixelWriterState state, in Span<byte> memory) {
		State = state;
		Base = memory;
	}


	public void SetPixelMemory(ImageFormat format, in Span<byte> memory, int rowSize) {
		State.SetFormat(format, (uint)rowSize);
		Base = memory;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Seek(int x, int y) {
		State.Bits = y * State.BytesPerRow + x * State.Size;
	}

	public void Dispose() {
		Base = null;
		State = default;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WritePixel(int r, int g, int b, int a) {
		WritePixelNoAdvance(r, g, b, a);
		State.Bits += State.Size;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe void WritePixelNoAdvance(int r, int g, int b, int a) {
		if (State.Size <= 0) return;
		long val = ((long)(r & State.RMask)) << State.RShift;
		val |= ((long)(g & State.GMask)) << State.GShift;
		val |= (State.BShift > 0) ? (((long)(b & State.BMask)) << State.BShift) : (((long)(b & State.BMask)) >> -State.BShift);
		val |= ((long)(a & State.AMask)) << State.AShift;

		fixed (byte* pBits = Base) {
			byte* bits = pBits + State.Bits;
			switch (State.Size) {
				case 6: {
						((uint*)bits)[0] = (uint)(val & 0xffffffff);
						((ushort*)bits)[2] = (ushort)((val >> 32) & 0xffff);

						return;
					}
				case 8: {
						((uint*)bits)[0] = (uint)(val & 0xffffffff);
						((uint*)bits)[1] = (uint)((val >> 32) & 0xffffffff);
						return;
					}
				default:
					Assert(0);
					return;
			}
		}
	}

	public void WritePixel(in Color color) => WritePixel(color.R, color.G, color.B, color.A);
}