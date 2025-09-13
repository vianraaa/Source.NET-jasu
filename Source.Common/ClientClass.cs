namespace Source.Common;

public delegate IClientNetworkable CreateClientClassFn(int entNum, int serialNum);
public delegate IClientNetworkable CreateEventFn();

public class ClientClass
{
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


/// <summary>
/// Manually sets the class index. This is required at the moment for Garry's Mod networking compat. Ideally at some point,
/// we can support all entity classes... and then we can just sort alphabetically.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ManualClassIndexAttribute : Attribute
{
	public required int Index;
}


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ImplementClientClassAttribute : Attribute
{
	public required string ClientClassName;
	public required string DataTable;
	public required string ServerClassName;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ImplementClientClassDTAttribute : ImplementClientClassAttribute
{

}