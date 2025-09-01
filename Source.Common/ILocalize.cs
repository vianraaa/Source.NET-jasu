using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common;

public interface ILocalize
{
	bool AddFile(ReadOnlySpan<char> fileName, ReadOnlySpan<char> pathID = default, bool includeFallbackSearchPaths = false);
}
