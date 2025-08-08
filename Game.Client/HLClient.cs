using Microsoft.Extensions.DependencyInjection;

using Source.Common.Bitbuffers;
using Source.Common.Client;

namespace Game.Client;

public class HLClient(IInput input) : IBaseClientDLL
{
	public static void DLLInit(IServiceCollection services) {
		services.AddSingleton<IInput, HLInput>();
	}

	public void IN_SetSampleTime(double frameTime) {

	}

	public void PostInit() {

	}

	public void CreateMove(int sequenceNumber, double inputSampleFrametime, bool active) {
		input.CreateMove(sequenceNumber, inputSampleFrametime, active);
	}

	public bool WriteUsercmdDeltaToBuffer(bf_write buf, int from, int to, bool isNewCommand) {
		return input.WriteUsercmdDeltaToBuffer(buf, from, to, isNewCommand);
	}
}
