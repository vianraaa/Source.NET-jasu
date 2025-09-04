using System.Numerics;
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double SimpleSpline(double value) {
		return (value * value) * (3 - 2 * value);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Bias(double x, double biasAmount) {
		double fRet = Math.Pow(x, Math.Log(biasAmount) * -1.4427);
		Assert(!double.IsNaN(fRet));
		return fRet;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Gain(double x, double biasAmount) {
		if (x < 0.5)
			return 0.5f * Bias(2 * x, 1 - biasAmount);
		else
			return 1 - 0.5f * Bias(2 - 2 * x, 1 - biasAmount);
	}

	public static Matrix4x4 CreateOpenGLOrthoOffCenter(float left, float right, float bottom, float top, float near, float far) {
		float m11 = 2.0f / (right - left);
		float m22 = -2.0f / (top - bottom);
		float m33 = -2.0f / (far - near);

		float m41 = -(right + left) / (right - left);
		float m42 = -(top + bottom) / (top - bottom);
		float m43 = -(far + near) / (far - near);

		return new Matrix4x4(
			m11, 0, 0, 0,
			0, m22, 0, 0,
			0, 0, m33, 0,
			m41, m42, m43, 1
		);
	}
}
