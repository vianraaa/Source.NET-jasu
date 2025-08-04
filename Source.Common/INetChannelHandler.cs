namespace Source.Common;

public interface INetChannelHandler
{
	public virtual void ConnectionStart(NetChannel channel) { }
	public virtual void ConnectionClosing(string reason) { }
	public virtual void ConnectionCrashed(string reason) { }
	public virtual void PacketStart(int incomingSequence, int outgoingAcknowledged) { }
	public virtual void PacketEnd() { }
	public virtual void FileRequested(string fileName, uint transferID) { }
	public virtual void FileReceived(string fileName, uint transferID) { }
	public virtual void FileDenied(string fileName, uint transferID) { }
	public virtual void FileSent(string fileName, uint transferID) { }

	public virtual bool ProcessMessage(INetMessage message) => false;
}