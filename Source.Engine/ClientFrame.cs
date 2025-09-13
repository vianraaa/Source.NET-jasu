
namespace Source.Engine;

public class ClientFrame {
	public const int MAX_CLIENT_FRAMES = 128;

	public int LastEntity;
	public long TickCount;

	public MaxEdictsBitVec TransmitEntity;
	public MaxEdictsBitVec FromBaseline;
	public MaxEdictsBitVec TransmitAlways;
	public ClientFrame? Next;

	internal void Init(int v) {
		throw new NotImplementedException();
	}
}
