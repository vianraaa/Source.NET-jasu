namespace Source.Common.Audio;

public enum SoundSampleRates
{
	Sound11khz = 11025,
	Sound22khz = 22050,
	Sound44khz = 44100
}

public enum SoundMix
{
	Wet,
	Dry,
	Speaker,
	SpecialDSP
}

public enum SoundBus
{
	Room = 1 << 0,
	Facing = 1 << 1,
	FacingAway = 1 << 2,
	Speaker = 1 << 3,
	Dry = 1 << 4,
	SpecialDSP = 1 << 5
}

public struct AudioChannel {
	public int GUID;
	public int UserData;
}

public class AudioSource {

}

public class SfxTable {
	public AudioSource? Source;
	public bool UseErrorFilename;
	public bool IsUISound;
	public bool IsLateLoad;
	public bool MixGroupsCached;
	public byte MixGroupCount;
}

public interface IAudioDevice {
	bool IsActive();
	bool Init();
	void Shutdown();
	void Pause();
	void UnPause();
	float MixDryVolume();
	bool Should3DMix();
	void StopAllSounds();
}