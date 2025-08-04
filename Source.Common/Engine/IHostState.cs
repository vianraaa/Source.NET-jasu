using Source.Common.Mathematics;

using System.Numerics;

namespace Source.Common.Engine;

public interface IHostState
{
	public void Init();
	public void Frame(double time);
	public void RunGameInit();
	public void NewGame(string mapName, bool rememberLocation, bool background);
	public void LoadGame(string mapName, bool rememberLocation);
	public void ChangeLevelSP(string mapName, string? landmark);
	public void ChangeLevelMP(string mapName, string? landmark);
	public void GameShutdown();
	public void Shutdown();
	public void Restart();
	public bool IsShuttingDown();
	public void OnClientConnected();
	public void OnClientDisconnected();
	public void SetSpawnPoint(in Vector3 pos, in QAngle angles);
}