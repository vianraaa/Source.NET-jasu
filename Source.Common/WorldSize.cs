using System.Numerics;
using System.Runtime.CompilerServices;

namespace Source.Common;
public static class WorldSize
{
	public const float MAX_COORD_INTEGER = 16384;
	public const float MIN_COORD_INTEGER = -MAX_COORD_INTEGER;
	public const float MAX_COORD_FRACTION = 1.0f - (1.0f / 16.0f);
	public const float MIN_COORD_FRACTION = -1.0f + (1.0f / 16.0f);

	public const float MAX_COORD_FLOAT = 16384.0f;
	public const float MIN_COORD_FLOAT = -MAX_COORD_FLOAT;

	public const float COORD_EXTENT = 2 * MAX_COORD_INTEGER;

	public const float MAX_TRACE_LENGTH = 1.732050807569f * COORD_EXTENT;
	public const float MAX_COORD_RANGE = MAX_COORD_INTEGER;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]	
	public static void ASSERT_COORD(Vector3 v) => Assert( (v.X>=MIN_COORD_INTEGER*2) && (v.X<=MAX_COORD_INTEGER*2) && 
														  (v.Y>=MIN_COORD_INTEGER*2) && (v.Y<=MAX_COORD_INTEGER*2) && 
														  (v.Z>=MIN_COORD_INTEGER*2) && (v.Z<=MAX_COORD_INTEGER*2)
														); 
}
