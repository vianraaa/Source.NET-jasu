namespace Source.Common.Server;

/// <summary>
/// Interface the game DLL exposes to the engine
/// </summary>
public interface IServerGameDLL
{
	void GameShutdown() { }
	public void PostInit();
}
