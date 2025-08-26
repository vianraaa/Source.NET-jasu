using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common;

public static unsafe class MemUtils
{
	public static void memset<T>(T* field, byte data, nuint size) where T : unmanaged {
		byte* write = (byte*)field;
		for (nuint i = 0; i < size; i++)
			write[i] = data;
	}
	/// <summary>
	/// This honestly might not be faster.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="field"></param>
	public static void memreset<T>(ref T field) where T : struct {
		Unsafe.InitBlock(ref Unsafe.As<T, byte>(ref field), 0, (uint)Unsafe.SizeOf<T>());
	}
	/// <summary>
	/// Performs C-style memory comparison on two unmanaged types.
	/// </summary>
	public static int memcmp<T>(in T buf1, in T buf2, nint size) where T : unmanaged {
		fixed (T* pBuf1 = &buf1) {
			fixed (T* pBuf2 = &buf2) {
				byte* b1 = (byte*)pBuf1;
				byte* b2 = (byte*)pBuf2;

				for (nint i = 0; i < size; i++) {
					int diff = b1[i] - b2[i];
					if (diff != 0)
						return diff;
				}

				return 0;
			}
		}
	}
	/// <summary>
	/// Performs C-style memory comparison on two unmanaged types, returning a boolean instead of an integer.
	/// </summary>
	public static bool memcmpb<T>(in T buf1, in T buf2, nint size) where T : unmanaged {
		fixed (T* pBuf1 = &buf1) {
			fixed (T* pBuf2 = &buf2) {
				byte* b1 = (byte*)pBuf1;
				byte* b2 = (byte*)pBuf2;

				for (nint i = 0; i < size; i++) {
					int diff = b1[i] - b2[i];
					if (diff != 0)
						return false;
				}

				return true;
			}
		}
	}
	public static void memcpy<T>(ref T dest, ref T src) where T : unmanaged {
		dest = src;
	}
	public static void memcpy<T>(Span<T> dest, Span<T> src) where T : unmanaged {
		src.CopyTo(dest);
	}
}

public unsafe ref struct UnmanagedHeapMemory {
	byte* Pointer;
	nuint Len;

	public UnmanagedHeapMemory(nuint bytes) {
		Pointer = (byte*)NativeMemory.Alloc(bytes);
		Len = bytes;
	}
	public UnmanagedHeapMemory(int bytes) {
		Pointer = (byte*)NativeMemory.Alloc((nuint)bytes);
		Len = (nuint)bytes;
	}

	public static implicit operator Span<byte>(UnmanagedHeapMemory memory) => new(memory.Pointer, (int)memory.Length);

	public void Dispose() {
		if (Pointer == null) {
			Warning("WARNING: ATTEPTED TO DISPOSE OF NO LONGER VALID UNMANAGED HEAP MEMORY\n");
			return;
		}
		NativeMemory.Free(Pointer);
		Pointer = null;
		Len = 0;
	}

	public readonly nuint Length => Len;
	public readonly nuint Handle => (nuint)Pointer;
}