using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

using static System.Runtime.Intrinsics.X86.Avx10v1;

namespace Source.Common.Mathematics;

public struct AddAngle
{
	public double Total;
	public double StartTime;
}

[StructLayout(LayoutKind.Sequential)]
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

	public static implicit operator Vector3(QAngle angle) => new(angle.X, angle.Y, angle.Z);
	public static implicit operator QAngle(Vector3 vector) => new(vector);
	public static QAngle operator +(QAngle a, QAngle b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
	public static QAngle operator -(QAngle a, QAngle b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
	public static QAngle operator *(QAngle a, QAngle b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
	public static QAngle operator /(QAngle a, QAngle b) => new(a.X / b.X, a.Y / b.Y, a.Z / b.Z);

	public static QAngle operator +(float a, QAngle b) => new(a + b.X, a + b.Y, a + b.Z);
	public static QAngle operator -(float a, QAngle b) => new(a - b.X, a - b.Y, a - b.Z);
	public static QAngle operator *(float a, QAngle b) => new(a * b.X, a * b.Y, a * b.Z);
	public static QAngle operator /(float a, QAngle b) => new(a / b.X, a / b.Y, a / b.Z);

	public static QAngle operator +(QAngle a, float b) => new(a.X + b, a.Y + b, a.Z + b);
	public static QAngle operator -(QAngle a, float b) => new(a.X - b, a.Y - b, a.Z - b);
	public static QAngle operator *(QAngle a, float b) => new(a.X * b, a.Y * b, a.Z * b);
	public static QAngle operator /(QAngle a, float b) => new(a.X / b, a.Y / b, a.Z / b);

	public static bool operator ==(QAngle a, QAngle b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
	public static bool operator !=(QAngle a, QAngle b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z;
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

	public static QAngle Lerp(in QAngle q1, in QAngle q2, float percent) {
		Quaternion src = Quaternion.CreateFromYawPitchRoll(q1.Y, q1.X, q1.Z);
		Quaternion dst = Quaternion.CreateFromYawPitchRoll(q2.Y, q2.X, q2.Z);

		Quaternion result = Quaternion.Slerp(src, dst, percent);;
		return FromQuaternion(in result);
	}

	public static QAngle FromQuaternion(in Quaternion result) {
		Matrix4x4 matrix = Matrix4x4.CreateFromQuaternion(result);
		QAngle ret;
		ExtractYawPitchRoll(in matrix, out ret.Y, out ret.X, out ret.Z);
		return ret;
	}

	// untested
	private static void ExtractYawPitchRoll(in Matrix4x4 matrix, out float yaw, out float pitch, out float roll) {
		yaw = (float)Math.Atan2(matrix.M12, matrix.M33);
		pitch = (float)Math.Asin(-matrix.M23);
		roll = (float)Math.Atan2(matrix.M21, matrix.M22);
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
