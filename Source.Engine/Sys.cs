using Pastel;

using Source.Common;
using Source.Common.Commands;
using Source.Engine.Server;

using System.Diagnostics;

namespace Source.Engine;

public class Sys(Host host, GameServer sv, ICommandLine CommandLine) {
	public Lazy<Stopwatch> Timer = new Lazy<Stopwatch>(() => {
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		return stopwatch;
	});
	public double Time => Timer.Value.Elapsed.TotalSeconds;
	public bool Dedicated;
	public bool TextMode;

	public bool InitGame(bool dedicated, string rootDirectory) {
		Dbg.SpewActivate("console", 1);
		Dbg.SpewOutputFunc(SpewFunc);
		host.Initialized = false;
		Dedicated = dedicated;

		host.Init(Dedicated);
		if (!host.Initialized)
			return false;

		return true;
	}
	public void ShutdownGame() {

	}
	ThreadLocal<bool> inSpew = new();
	public SpewRetval SpewFunc(SpewType spewType, string msg) {
		if (!inSpew.IsValueCreated)
			inSpew.Value = false;

		bool suppress = inSpew.Value;
		inSpew.Value = true;

		const string engineGroup = "engine";
		string? group = Dbg.GetSpewOutputGroup();
		group = string.IsNullOrEmpty(group) ? engineGroup : group;

		if (!suppress) {
			if (TextMode) {
				if(spewType == SpewType.Message || spewType == SpewType.Log) {
					Console.Write($"[{group}] {msg}");
				}
				else {
					Console.Write($"[{group}] {msg}");
				}
			}

			if((spewType != SpewType.Log) || sv.GetMaxClients() == 1) {
				Color color = new();
				switch (spewType) {
					case SpewType.Warning: color.SetColor(255, 90, 90, 255); break;
					case SpewType.Assert: color.SetColor(255, 20, 20, 255); break;
					case SpewType.Error: color.SetColor(20, 70, 255, 255); break;
					default: color = Dbg.GetSpewOutputColor(); break;
				}
				Console.Write($"[{group}] {msg}".Pastel(color));
			}
			else {
				Console.Write($"[{group}] {msg}");
			}
		}

		inSpew.Value = false;
		if(spewType == SpewType.Error) {
			Error($"[{group}] {msg}");
			return SpewRetval.Abort;
		}

		if(spewType == SpewType.Assert) {
			if (CommandLine.FindParm("-noassert") == 0)
				return SpewRetval.Debugger;
			else
				return SpewRetval.Continue;
		}

		return SpewRetval.Continue;
	}

	private void Error(string v) {
		Dbg.Warning(v);
		Dbg.AssertMsg(false, v);
	}
}