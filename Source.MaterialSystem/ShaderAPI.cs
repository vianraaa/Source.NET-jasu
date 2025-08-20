using Source.Common;
using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.MaterialSystem;

public class ShaderAPIGl46 : IShaderAPI
{
	public VertexFormat ComputeVertexUsage(Span<short> snapshots) {
		throw new NotImplementedException();
	}
}