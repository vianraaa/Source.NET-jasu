using Source.Engine.Client;
using Source.Engine.Server;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;

/// <summary>
/// Shared Util class. Various components of the engine can add their own things here.
/// </summary>
public class Util
{
	public readonly ClientState? Client;
	public readonly GameServer Server;

	public readonly Host Host;
	
	public Util(ClientState? client, GameServer server, Host host) {
		Client = client;
		Server = server;
		Host = host;
	}
}