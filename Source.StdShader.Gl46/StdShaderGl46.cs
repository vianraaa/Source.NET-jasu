using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.Common.ShaderLib;

namespace Source.StdShader.Gl46;

public class StdShaderGl46 : IShaderDLL
{
	List<BaseShader> shaders = [];
	public StdShaderGl46(IServiceProvider engineAPI) {
		foreach(var type in GetType().Assembly.GetTypes()) {
			if (!type.IsAbstract && type.IsAssignableTo(typeof(BaseShader))) {
				shaders.Add((BaseShader)engineAPI.New(type));
			}
		}
	}

	public IEnumerable<IShader> GetShaders() {
		return shaders;
	}
}