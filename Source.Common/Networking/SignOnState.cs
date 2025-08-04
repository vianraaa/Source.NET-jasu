namespace Source.Common.Networking;

public enum SignOnState
{
	/// <summary>
	/// No sign-on state. Either disconnected, or about to connect.
	/// </summary>
	None,
	/// <summary>
	/// Client is challenging the server. All packets are out-of-band packets.
	/// <br/>
	/// <br/> Client -- <see cref="A2S.GetChallenge"/> -> Server
	/// <br/> Server -- <see cref="S2C.Challenge"/> -> Client
	/// <br/> Client -- <see cref="C2S.Connect"/> -> Server with basic client info and Steam auth ticket
	/// <br/> 
	/// <br/> If everything passes...
	/// <br/>     Server -- <see cref="S2C.Connection"/> -> Client. The server then waits for the client to set up its <see cref="NetChannel"/>. See next step for further details.
	/// <br/> <b>OR</b>
	/// <br/>     Server -- <see cref="S2C.ConnectionRejected"/> -> Client. Includes a string-reason for why.
	/// </summary>
	Challenge,
	/// <summary>
	/// Client has connected to the server. 
	/// <br/>The client has setup its <see cref="NetChannel"/> and communication goes through that at this point.
	/// <br/>
	/// <br/> Client -- <see cref="Net.SetConVar"/> -> Server
	/// <br/> Client -- <see cref="Net.SignOnState"/>(<see cref="Connected"/>) -> Server
	/// <br/>
	/// <br/> These are the first compressed packets sent via NetChan:
	/// <br/> Server -- <see cref="Net.Tick"/> -> Client
	/// <br/> For each stringtable... Server -- <see cref="SVC.CreateStringTable"/> -> Client
	/// <br/> For each stringtable... Server -- <see cref="SVC.UpdateStringTable"/> -> Client
	/// <br/> Server -- <see cref="SVC.GMod_ServerToClient"/> -> Client (unknown purpose)
	/// <br/> Server -- <see cref="Net.SetConVar"/> -> Client (various things like sv_cheats, sv_skyname, etc)
	/// <br/> Server -- <see cref="Net.SignOnState"/>(<see cref="New"/>) -> Client
	/// </summary>
	Connected,
	/// <summary>
	/// Client has received stringtables and some basic state information. It is now the clients job to prepare client info.
	/// <br/>
	/// <br/> Client -- <see cref="CLC.ClientInfo"/> -> Server (SendTableCRC bugged right now, hacky fix)
	/// <br/> Client -- <see cref="CLC.GMod_ClientToServer"/> -> Server (unknown purpose)
	/// <br/> Client -- <see cref="Net.SignOnState"/>(<see cref="New"/>) -> Server 
	/// <br/> 
	/// <br/> Then we wait for the server to send us a few messages...
	/// <br/> Server -- <see cref="SVC.ClassInfo"/> -> Client
	/// <br/> Server -- <see cref="SVC.BSPDecal"/> -> Client
	/// <br/> Server -- <see cref="SVC.VoiceInit"/> -> Client
	/// <br/> Server -- <see cref="SVC.GameEventList"/> -> Client
	/// <br/> Server -- <see cref="Net.SignOnState"/>(<see cref="PreSpawn"/>) -> Client
	/// </summary>
	New,
	/// <summary>
	/// The client has received signon buffers. Specifically, the client receives:
	/// <br/>
	/// <br/>
	/// </summary>
	PreSpawn,
	/// <summary>
	/// The client is ready to receive entity packets.
	/// </summary>
	Spawn,
	/// <summary>
	/// We are fully connected, first non-delta packet
	/// </summary>
	Full,
	/// <summary>
	/// Server is changing level.
	/// </summary>
	ChangeLevel
}