using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common;

public static class COM
{
	readonly static CharacterSet BreakSet = new("{}()");
	readonly static CharacterSet BreakSetIncludingColons = new("{}()':");

	public static bool IsValidPath(ReadOnlySpan<char> filename) {
		if (filename == null)
			return false;

		if (filename.Length == 0
			|| filename.Contains("\\\\", StringComparison.OrdinalIgnoreCase) // To protect network paths
			|| filename.Contains(":", StringComparison.OrdinalIgnoreCase) // To protect absolute paths
			|| filename.Contains("..", StringComparison.OrdinalIgnoreCase) // To protect relative paths
			|| filename.Contains("\n", StringComparison.OrdinalIgnoreCase)
			|| filename.Contains("\r", StringComparison.OrdinalIgnoreCase)
		)
			return false;

		return true;
	}
}