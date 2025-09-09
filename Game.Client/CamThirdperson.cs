using Source;
using Source.Common.Mathematics;

namespace Game.Client;

[EngineComponent]
public class ThirdPersonManager
{
	public QAngle GetCameraOffsetAngles() {
		return new();
	}
}
