using System.Runtime.CompilerServices;

namespace Source.Common;

public delegate IClientNetworkable CreateClientClassFn(int entNum, int serialNum);
public delegate IClientNetworkable CreateEventFn();

public class ClientClass
{
	public static ClientClass? Head;

	public CreateClientClassFn? CreateFn;
	public CreateEventFn? CreateEventFn;
	public string? NetworkName;
	public RecvTable? RecvTable;
	public ClientClass? Next;
	public int ClassID;

	public ClientClass(ReadOnlySpan<char> networkName, CreateClientClassFn createFn, CreateEventFn? createEventFn, RecvTable recvTable, [CallerArgumentExpression(nameof(recvTable))] string? nameOfTable = null) {
		CreateFn = createFn;
		CreateEventFn = createEventFn;
		NetworkName = new(networkName);
		RecvTable = recvTable;

		if(nameOfTable != null)
			recvTable.NetTableName = nameOfTable;

		Next = Head;
		Head = this;
		ClassID = -1;
	}

	public ReadOnlySpan<char> GetName() => NetworkName;
	public ClientClass WithManualClassID(int classID) {
		ClassID = classID;
		return this;
	}
}


/// <summary>
/// Declares a client class
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DeclareClientClass : Attribute;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ImplementClientClassAttribute : Attribute
{
	public required string ClientClassName;
	public required string RecvTable;
	public required string ServerClassName;
}