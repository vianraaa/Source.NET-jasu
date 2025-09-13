using Source.Common.Mathematics;

using System.Numerics;

namespace Source.Common;

public interface IPrediction
{
	void Init();
	void Shutdown();

	void Update(int startFrame, bool validFrame, int incomingAcknowledged, int outgoingCommand);

	void PreEntityPacketReceived(int commandsAcknowledged, int currentWorldUpdatePacket);
	void PostEntityPacketReceived();
	void PostNetworkDataReceived(int commandsAcknowledged);

	void OnReceivedUncompressedPacket();

	void GetViewOrigin(out Vector3 org);
	void SetViewOrigin(in Vector3 org);
	void GetViewAngles(out QAngle ang);
	void SetViewAngles(in QAngle ang);
	void GetLocalViewAngles(out QAngle ang);
	void SetLocalViewAngles(in QAngle ang);
}
