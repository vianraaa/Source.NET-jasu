using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common;

public static unsafe class MemUtils
{
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
}