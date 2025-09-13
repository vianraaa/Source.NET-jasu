namespace Source.Common;

public interface ICenterPrint
{
	void SetTextColor(int r, int g, int b, int a);
	void Print(ReadOnlySpan<char> text);
	void ColorPrint(int r, int g, int b, int a, ReadOnlySpan<char> text);
	void Clear();
}
