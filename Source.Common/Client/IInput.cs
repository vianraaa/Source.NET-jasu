using Source.Common.Bitbuffers;

namespace Source.Common.Client;

public interface IInput
{
	public void CreateMove(int sequenceNumber, double inputSampleFrametime, bool active);
	public bool WriteUsercmdDeltaToBuffer(bf_write buf, int from, int to, bool isNewCommand);
}