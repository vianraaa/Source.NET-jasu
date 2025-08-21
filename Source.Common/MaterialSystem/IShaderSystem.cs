using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.MaterialSystem;

public enum ShaderUsing {
	ColorModulation = 0x1,
	AlphaModulation = 0x2,
	Flashlight = 0x4,
	FixedFunctionBakedLighting = 0x8,
	Editor = 0x10
}
public interface IShaderSystem
{
	void BindTexture(Sampler sampler, ITexture texture);
	void DrawSnapshot(bool makeActualDrawCall = true);
	void TakeSnapshot();
}
public interface IShaderDLL
{
	public IEnumerable<IShader> GetShaders();
}