namespace Source;

public static class MathLib
{
	public static int Modulo(int a, int b) {
		return (Math.Abs(a * b) + a) % b;
	}
}
