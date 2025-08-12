using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.MaterialSystem;

public enum ShadeMode {
	Flat,
	Smooth
}

public enum TexCoordComponent {
	S,
	T,
	U
}

public enum TexFilterMode {
	Nearest,
	Linear,
	NearestMipmapNearest,
	LinearMipmapNearest,
	NearestMipmapLinear,
	LinearMipmapLinear,
	Anisotropic
}

public enum TexWrapMode {
	Clamp,
	Repeat,
	Border
}

public enum TextureStage {
	Stage0,
	Stage1
}

public enum Sampler {
	Sampler0,
	Sampler1,
	Sampler2,
	Sampler3,
	Sampler4,
	Sampler5,
	Sampler6,
	Sampler7,
	Sampler8,
	Sampler9,
	Sampler10,
	Sampler11,
	Sampler12,
	Sampler13,
	Sampler14,
	Sampler15,
}

public enum VertexTextureSampler {
	Sampler0,
	Sampler1,
	Sampler2,
	Sampler3,
}