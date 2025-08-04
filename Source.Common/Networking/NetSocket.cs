using System.Net.Sockets;

namespace Source.Common.Networking;

public class NetSocket
{
	public const int MAX = (int)NetSocketType.Max;
	public int Port;
	public bool Listening;
	public Socket? UDP;
	public Socket? TCP;
}
