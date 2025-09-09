using System.Numerics;
using System.Runtime.Intrinsics.X86;

namespace Source.Common.Mathematics;

public struct AddAngle
{
	public double Total;
	public double StartTime;
}

public struct QAngle
{
	public float X, Y, Z;

	public QAngle() {
		X = 0;
		Y = 0;
		Z = 0;
	}

	public QAngle(float xyz) {
		X = xyz;
		Y = xyz;
		Z = xyz;
	}
	public QAngle(float x, float y, float z) {
		X = x;
		Y = y;
		Z = z;
	}

	public QAngle(Vector3 vec) {
		X = vec.X;
		Y = vec.Y;
		Z = vec.Z;
	}

	public void Init() {
		X = 0;
		Y = 0;
		Z = 0;
	}
	const float TORADS = MathF.PI / 180f;
	// TODO: is there a C# + SIMD way to do this?
	public void Vectors(out Vector3 forward, out Vector3 right, out Vector3 up) {
		Vector4 radians = new Vector4(X, Y, Z, 0) * new Vector4(TORADS, TORADS, TORADS, TORADS);

		float sp = MathF.Sin(radians[0]), sy = MathF.Sin(radians[1]), sr = MathF.Sin(radians[2]);
		float cp = MathF.Cos(radians[0]), cy = MathF.Cos(radians[1]), cr = MathF.Cos(radians[2]);

		forward = new(cp * cy, cp * sy, -sp);
		right = new(-1 * sr * sp * cy + -1 * cr * -sy, -1 * sr * sp * sy + -1 * cr * cy, -1 * sr * cp);
		up = new(cr * sp * cy + -sr * -sy, cr * sp * sy + -sr * cy, cr * cp);
	}

	public static float Normalize(float angle) {
		angle = MathLib.Fmodf(angle, 360.0f);
		if (angle > 180) {
			angle -= 360;
		}
		if (angle < -180) {
			angle += 360;
		}
		return angle;
	}

	public static QAngle Normalize(in QAngle angle) => new(Normalize(angle.X), Normalize(angle.Y), Normalize(angle.Z));

	public float this[int index] {
		get {
			switch (index) {
				case 0: return X;
				case 1: return Y;
				case 2: return Z;
				default: throw new IndexOutOfRangeException();
			}
		}
		set {
			switch (index) {
				case 0: X = value; return;
				case 1: Y = value; return;
				case 2: Z = value; return;
				default: throw new IndexOutOfRangeException();
			}
		}
	}
}
