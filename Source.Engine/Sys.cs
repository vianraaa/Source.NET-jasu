using Source.Common;

using System.Diagnostics;

namespace Source.Engine;

public class Sys(Host host) {
	public Lazy<Stopwatch> Timer = new Lazy<Stopwatch>(() => {
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		return stopwatch;
	});
	public double Time => Timer.Value.Elapsed.TotalSeconds;
	public bool Dedicated;

	public bool InitGame(bool dedicated, string rootDirectory) {
		host.Initialized = false;
		Dedicated = dedicated;

		host.Init(Dedicated);
		if (!host.Initialized)
			return false;

		return true;
	}
	public void ShutdownGame() {

	}
}