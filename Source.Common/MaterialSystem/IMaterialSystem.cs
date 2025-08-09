using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.MaterialSystem;

public enum MaterialBufferTypes {
	Front,
	Back
}

public enum MaterialPrimitiveType {
	Points,
	Lines,
	Triangles,
	TriangleStrip,
	LineStrip,
	LineLoop,
	Polygon,
	Quads,
	InstancedQuads,
	Heterogenous
}

public enum MaterialCullMode
{
	CounterClockwise,
	Clockwise
}

public enum MaterialIndexFormat {
	Unknown = -1,
	x16Bits,
	x32Bits
}

public interface IMaterialSystem
{

}
