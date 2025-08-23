using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Source;

public static class PlatformMacros
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsPC() => true;
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsPlatform64Bits() => IntPtr.Size == 8;

	public const int MAX_PATH = 260;
}

public static class Platform
{
	readonly static Lazy<Stopwatch> __timer = new(() => {
		Stopwatch stopwatch = new();
		stopwatch.Start();
		return stopwatch;
	});

	public static double Time => __timer.Value.Elapsed.TotalSeconds;

#if WIN32
	[DllImport("kernel32.dll")]
	unsafe static extern void OutputDebugStringW(char* lpOutputString);
#endif

	public static unsafe void DebugString(ReadOnlySpan<char> buf) {
#if WIN32
		fixed (char* cbuf = buf)
			OutputDebugStringW(cbuf);
#endif
	}
}