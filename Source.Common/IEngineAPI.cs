namespace Source.Common;

public interface IEngineAPI
{
	public enum RunResult
	{
		OK = 0,
		Error = 1,
		Restart = 2
	}

	public RunResult Run();
}