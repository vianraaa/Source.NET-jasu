namespace Source.Common;

public static class ThreadUtils
{
	static Thread? MainThread;
	public static void SetMainThread() => MainThread = Thread.CurrentThread;
	public static bool ThreadInMainThread() {
		return Thread.CurrentThread == MainThread!;
	}
}
