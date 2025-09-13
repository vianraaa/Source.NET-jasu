using Source.Common;
using Source.Common.Mathematics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client;
public class Prediction : IPrediction
{

	bool InPrediction;
	bool FirstTimePredicted;
	bool OldCLPredictValue;
	bool EnginePaused;

	int PreviousStartFrame;

	int CommandsPredicted;
	int ServerCommandsAcknowledged;
	int PreviousAckHadErrors;
	int IncomingPacketNumber;

	float IdealPitch;

	public void GetLocalViewAngles(out QAngle ang) {
		throw new NotImplementedException();
	}

	public void GetViewAngles(out QAngle ang) {
		throw new NotImplementedException();
	}

	public void GetViewOrigin(out Vector3 org) {
		throw new NotImplementedException();
	}

	public void Init() {
		throw new NotImplementedException();
	}

	public void OnReceivedUncompressedPacket() {
		CommandsPredicted = 0;
		ServerCommandsAcknowledged = 0;
		PreviousStartFrame = -1;
	}

	public void PostEntityPacketReceived() {
		throw new NotImplementedException();
	}

	public void PostNetworkDataReceived(int commandsAcknowledged) {
		throw new NotImplementedException();
	}

	public void PreEntityPacketReceived(int commandsAcknowledged, int currentWorldUpdatePacket) {
		throw new NotImplementedException();
	}

	public void SetLocalViewAngles(in QAngle ang) {
		throw new NotImplementedException();
	}

	public void SetViewAngles(in QAngle ang) {
		throw new NotImplementedException();
	}

	public void SetViewOrigin(in Vector3 org) {
		throw new NotImplementedException();
	}

	public void Shutdown() {
		throw new NotImplementedException();
	}

	public void Update(int startFrame, bool validFrame, int incomingAcknowledged, int outgoingCommand) {
		throw new NotImplementedException();
	}
}
