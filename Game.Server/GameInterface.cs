using Microsoft.Extensions.DependencyInjection;

using Source;
using Source.Common;
using Source.Common.Filesystem;
using Source.Common.Server;

namespace Game.Server;

[EngineComponent]
public static class GameInterface
{

}

public class ServerGameDLL(IEngineServer engine, IFileSystem filesystem) : IServerGameDLL
{
	public static void DLLInit(IServiceCollection services) {
		
	}

	public ServerClass? GetAllServerClasses() {
		return ServerClass.Head;
	}

	public void PostInit() {

	}
}
