namespace Source.Common;

public delegate IClientNetworkable CreateClientClassFn(int entNum, int serialNum);
public delegate IClientNetworkable CreateEventFn();

public class ClientClass {
	public static ClientClass? ClientClassHead;

	public CreateClientClassFn? CreateFn;
	public CreateEventFn? CreateEventFn; 
	public string? NetworkName;
	public RecvTable? RecvTable;
	public ClientClass? Next;
	public int ClassID;

	public ClientClass(ReadOnlySpan<char> networkName, CreateClientClassFn createFn, CreateEventFn createEventFn, RecvTable recvTable) {
		CreateFn = createFn;
		CreateEventFn = createEventFn;
		NetworkName = new(networkName);
		RecvTable = recvTable;

		Next = ClientClassHead;
		ClientClassHead = this;
		ClassID = -1;
	}

	public ReadOnlySpan<char> GetName() => NetworkName;
}
