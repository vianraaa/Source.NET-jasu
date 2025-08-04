namespace Source.Common.Engine;

public interface IMod
{
	public enum Result
	{
		RunOK = IEngineAPI.Result.RunOK,
		RunRestart = IEngineAPI.Result.RunRestart,
	}
	public bool Init(string initialMod, string initialGame);
	public Result Run();
	public void Shutdown();
}
