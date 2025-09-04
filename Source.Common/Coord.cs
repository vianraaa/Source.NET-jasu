using System.Runtime.InteropServices;

namespace Source;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct Coord
{
	public int X;
	public int Y;
}
