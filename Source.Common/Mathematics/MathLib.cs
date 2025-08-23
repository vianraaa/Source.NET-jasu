using System.Runtime.CompilerServices;

namespace Source.Common.Mathematics;

public static class MathLib
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Lerp(float f1, float f2, float i1, float i2, float x) {
		return f1 + (f2 - f1) * (x - i1) / (i2 - i1);
	}

	public static int Modulo(int a, int b) {
		return (Math.Abs(a * b) + a) % b;
	}
}
