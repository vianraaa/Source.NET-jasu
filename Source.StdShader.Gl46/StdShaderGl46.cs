using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.Common.ShaderLib;

namespace Source.StdShader.Gl46;

public abstract class BaseShaderGl46 : BaseShader {

}

public class StdShaderGl46 : IShaderDLL
{
	List<BaseShaderGl46> shaders = [];
	public StdShaderGl46(IEngineAPI engineAPI) {
		foreach(var type in GetType().Assembly.GetTypes()) {
			if (type.IsAssignableTo(typeof(BaseShaderGl46))) {
				shaders.Add((BaseShaderGl46)engineAPI.New(type));
			}
		}
	}

	public IEnumerable<IShader> GetShaders() {
		return shaders;
	}
}