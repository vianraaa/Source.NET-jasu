namespace Source.Common.Server;


/// <summary>
/// Interface the game DLL exposes to the engine
/// </summary>
public interface IServerGameDLL
{
	void GameShutdown() { }
	public void PostInit();
}

/// <summary>
/// Interface to get at server entities
/// </summary>
public interface IServerGameEnts
{

}

/// <summary>
/// Player/client related functions
/// </summary>
public interface IServerGameClients
{

}