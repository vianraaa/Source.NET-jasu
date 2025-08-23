using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.MaterialSystem;

public interface IShaderSystem
{
	void BindTexture(Sampler sampler, ITexture texture);
	void Draw(bool makeActualDrawCall = true);
	void Init();
	void ResetShaderState();
}
public interface IShaderDLL
{
	public IEnumerable<IShader> GetShaders();
}