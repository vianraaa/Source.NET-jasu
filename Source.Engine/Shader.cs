
using Source.Common.MaterialSystem;

namespace Source.Engine;

public class Shader(IMaterialSystem materials)
{
	public void SwapBuffers() {
		materials.SwapBuffers();
	}
}
