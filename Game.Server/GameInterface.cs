global using static Game.Server.GameInterface;

using Microsoft.Extensions.DependencyInjection;

using Source;
using Source.Common.Client;
using Source.Common.Filesystem;
using Source.Common.Server;
using Source.Engine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server;

[EngineComponent]
public static class GameInterface
{

}

public class ServerGameDLL(IEngineServer engine, IFileSystem filesystem) : IServerGameDLL
{
	public static void DLLInit(IServiceCollection services) {
		
	}

	public void PostInit() {

	}
}
