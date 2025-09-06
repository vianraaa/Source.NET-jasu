using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.GUI.Controls;
namespace Source.MaterialSystem;

public class MatEmbeddedPanel() : Panel(null, "MatSystemTopPanel")
{
	public IMaterialSystem materials = Singleton<IMaterialSystem>();
	public override void OnThink() {
		using MatRenderContextPtr renderContext = new(materials);
		renderContext.GetViewport(out int x, out int y, out int width, out int height);
		SetSize(width, height);
		SetPos(x, y);
		Repaint();
	}
}
