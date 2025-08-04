namespace Source
{
	public static class ClassUtils
	{
		public static void EnsureCount<T>(this List<T> list, int ensureTo) where T : class, new() {
			while (list.Count < ensureTo) {
				list.Add(new T());
			}
		}
	}
	public static class UnmanagedUtils
	{
		public static void EnsureCount<T>(this List<T> list, int ensureTo) where T : unmanaged {
			while (list.Count < ensureTo) {
				list.Add(new T());
			}
		}
	}
}