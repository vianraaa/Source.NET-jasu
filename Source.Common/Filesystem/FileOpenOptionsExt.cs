namespace Source.Common.Filesystem;

public static class FileOpenOptionsExt
{
	public static FileOpenOptions GetOperation(this FileOpenOptions options) => options & (FileOpenOptions)0b11;

	public static bool Extended(this FileOpenOptions options) =>
		(options & FileOpenOptions.Extended) != 0;
	public static bool IsBinary(this FileOpenOptions options) =>
		(options & FileOpenOptions.Binary) != 0;
	public static bool IsText(this FileOpenOptions options) =>
		(options & FileOpenOptions.Text) != 0;
}
