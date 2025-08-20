using Source.Common.Commands;

namespace Source.MaterialSystem;

[EngineComponent]
public static class Cvars
{
	public static ConVar mat_supportflashlight = new("-1", FCvar.Hidden, "0 - do not support flashlight (don't load flashlight shader combos), 1 - flashlight is supported");
}
