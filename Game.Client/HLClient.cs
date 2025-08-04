using Source.Common;

namespace Game.Client;

public class HLClient : IBaseClientDLL
{
	public void CreateMove(int sequenceNumber, double inputSampleFrametime, bool active) {

	}

	public void IN_SetSampleTime(double frameTime) {

	}

	public void PostInit() {

	}

	public bool WriteUsercmdDeltaToBuffer(bf_write buf, int from, int to, bool isNewCommand) {
		return false;
	}
}
