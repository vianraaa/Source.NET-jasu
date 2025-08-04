using Source.Common;
using Source.Engine;

namespace Source.Launcher;

internal class Program
{
    static void Main(string[] args)
    {
		bool needsRestart;
		do {
			IEngineAPI engine = new EngineBuilder()

				.Build();

			needsRestart = engine.Run() == IEngineAPI.RunResult.Restart;
		} while (needsRestart);
    }
}