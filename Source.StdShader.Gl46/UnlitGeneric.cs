using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.StdShader.Gl46;

public class UnlitGeneric : BaseShaderGl46
{
	public override string? GetFallbackShader(IMaterialVar[] vars) {
		return null;
	}
}
