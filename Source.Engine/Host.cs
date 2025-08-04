namespace Source.Engine;

public class CommonHostState {
	public double IntervalPerTick;
}

public class Host(EngineParms host_parms) {
	public string GetCurrentMod() => host_parms.Mod;
	public string GetCurrentGame() => host_parms.Game;
	public string GetBaseDirectory() => host_parms.BaseDir;

	public bool Initialized;
	public double FrameTime;
	public double FrameTimeUnbounded;
	public double FrameTimeStandardDeviation;
	public double RealTime;
	public double IdealTime;
	public double NextTick;
	public double[] JitterHistory = new double[128];
	public uint JitterHistoryPos;
	public long FrameCount;
	public int HunkLevel;


}