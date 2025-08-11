using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source;

public static class Platform
{
	readonly static Lazy<Stopwatch> Timer = new(() => {
		Stopwatch stopwatch = new();
		stopwatch.Start();
		return stopwatch;
	});

	public static double Time => Timer.Value.Elapsed.TotalSeconds;
}