using System.Numerics;

namespace Source.Common.Mathematics;

public enum FrustumPlane
{
	Right = 0,
	Left = 1,
	Top = 2,
	Bottom = 3,
	NearZ = 4,
	FarZ = 5,
	NumPlanes = 6
}

public struct Frustum {
	public VPlane Right;
	public VPlane Left;
	public VPlane Top;
	public VPlane Bottom;
	public VPlane NearZ;
	public VPlane FarZ;
}

public struct VPlane
{
	public const int SIDE_FRONT = 0;
	public const int SIDE_BACK = 1;
	public const int SIDE_ON = 2;
	public const float VP_EPSILON = 0.01f;

	public Vector3 Normal;
	public vec_t Dist;

	public void Init(in Vector3 normal, in vec_t dist) {

	}
	public vec_t DistTo(in Vector3 vec) {
		return 0; // todo
	}
}
