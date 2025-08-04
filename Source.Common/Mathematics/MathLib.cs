namespace Source.Common.Mathematics;

public static class MathLib
{
	public static int Modulo(int a, int b) {
		return (Mathematics.Abs(a * b) + a) % b;
	}
}
