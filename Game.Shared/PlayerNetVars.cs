using Source;
using Source.Common;

using System.Numerics;

namespace Game.Shared;

public static class PlayerNetVars
{

}

public struct FogParams()
{
	public readonly Handle<SharedBaseEntity> Ctrl = new();
	public float TransitionTime;

	public Color OldColor;
	public float OldStart;
	public float OldEnd;

	public Color NewColor;
	public float NewStart;
	public float NewEnd;
}


public struct Sky3DParams()
{
	public int Scale;
	public Vector3 Origin;
	public int Area;

	public FogParams Fog = new();
}

public struct AudioParams()
{
	public InlineArrayNumLocalAudioSounds<Vector3> LocalSound;
	public int SoundscapeIndex;
	public int LocalBits;
	public EHANDLE handle;
}
