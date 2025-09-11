using Source.Common.Audio;

namespace Source.Engine;



public class Sound {
	public void Init() {

	}

	public SfxTable PrecacheSound(ReadOnlySpan<char> fileName) {
		throw new NotImplementedException();
	}

	public void MarkUISound(SfxTable sound) {
		sound.IsUISound = true;
	}

	internal void StartSound(in StartSoundParams parms) {
		throw new NotImplementedException();
	}

	// todo: everything else here
	// Further research is needed on audio systems
}
