using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.MaterialSystem;

public interface IShaderSystem
{
	void BindTexture(in MaterialVarGPU hardwareTarget, ITexture texture, int frame = 0);
	void Draw(bool makeActualDrawCall = true);
	void Init();
	void ResetShaderState();
}
public interface IShaderDLL
{
	public IEnumerable<IShader> GetShaders();
}