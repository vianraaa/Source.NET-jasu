using Pastel;

using Source.Common;
using Source.Common.Commands;
using Source.Engine.Server;

using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Source.Engine;
public class Sys(Host host, GameServer sv, ICommandLine CommandLine)
{
	public static double Time => Platform.Time;
	public bool Dedicated;
	public bool TextMode;

	public string? DisconnectReason = null;
	public string? ExtendedDisconnectReason = null;
	public bool ExtendedError = false;

	public Thread? MainThread;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool InMainThread() => Thread.CurrentThread == MainThread;

	public bool InitGame(bool dedicated, string rootDirectory) {
		MainThread = Thread.CurrentThread;
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
	ThreadLocal<string> groupWrite = new();
	private void Write(string group, ReadOnlySpan<char> str, in Color color) {
		if (!groupWrite.IsValueCreated)
			groupWrite.Value = "";

		Span<char> buffer = stackalloc char[256];
		int bufferIdx = 0;
		for (int i = 0; i < str.Length; i++) {
			char c = str[i];
			if (c == '\n') {
				groupWrite.Value = "";

				if (bufferIdx > 0)
					Console.Write(((ReadOnlySpan<char>)buffer[..bufferIdx]).Pastel(color));
				bufferIdx = 0;

				Console.WriteLine();
			}
			else {
				if (groupWrite.Value != group) {
					if (bufferIdx > 0)
						Console.Write(((ReadOnlySpan<char>)buffer[..bufferIdx]).Pastel(color));
					bufferIdx = 0;

					Console.Write($"[{group}] ".Pastel(color));
					groupWrite.Value = group;
				}
				if (bufferIdx >= buffer.Length) {
					Console.Write(((ReadOnlySpan<char>)buffer).Pastel(color));
					bufferIdx = 0;
				}
				buffer[bufferIdx++] = c;
			}
		}

		if (bufferIdx > 0)
			Console.Write(((ReadOnlySpan<char>)buffer[..bufferIdx]).Pastel(color));
	}
	public SpewRetval SpewFunc(SpewType spewType, ReadOnlySpan<char> msg) {
		if (!inSpew.IsValueCreated)
			inSpew.Value = false;

		bool suppress = inSpew.Value;
		inSpew.Value = true;

		const string engineGroup = "engine";
		string? group = Dbg.GetSpewOutputGroup();
		group = string.IsNullOrEmpty(group) ? engineGroup : group;

		if (!suppress) {
			/*if (TextMode) {
				if(spewType == SpewType.Message || spewType == SpewType.Log) {
					Console.Write($"[{group}] {msg}");
				}
				else {
					Console.Write($"[{group}] {msg}");
				}
			}*/

			if ((spewType != SpewType.Log) || sv.GetMaxClients() == 1) {
				Color color = new();
				switch (spewType) {
					case SpewType.Warning: color.SetColor(255, 90, 90, 255); break;
					case SpewType.Assert: color.SetColor(255, 20, 20, 255); break;
					case SpewType.Error: color.SetColor(20, 70, 255, 255); break;
					default: color = Dbg.GetSpewOutputColor(); break;
				}
				Write(group, msg, color);
			}
			else {
				Color color = new Color(255, 255, 255);
				Write(group, msg, in color);
			}
		}

		inSpew.Value = false;
		if (spewType == SpewType.Error) {
			Error($"[{group}] {msg}");
			return SpewRetval.Abort;
		}

		if (spewType == SpewType.Assert) {
			if (CommandLine.FindParm("-noassert") == 0)
				return SpewRetval.Debugger;
			else
				return SpewRetval.Continue;
		}

		return SpewRetval.Continue;
	}

	public void Error(string v) {
		Dbg.Warning(v);
		Dbg.AssertMsg(false, v);
	}
}