namespace Source.Common;
public interface IEngineAPI : IServiceProvider
{
	public enum Result
	{
		InitFailed = 0,
		InitOK,
		InitRestart,
		RunOK,
		RunRestart
	}

	public Result Run();
	public void SetStartupInfo(in StartupInfo info);
	bool MainLoop();
}
