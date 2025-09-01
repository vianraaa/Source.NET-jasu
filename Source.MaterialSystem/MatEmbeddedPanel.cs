using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.GUI.Controls;
namespace Source.MaterialSystem;

public class MatEmbeddedPanel() : Panel(null, "MatSystemTopPanel")
{
	[Imported] public IMaterialSystem materials;
	public override void OnThink() {
		using MatRenderContextPtr renderContext = new(materials);
		renderContext.GetViewport(out int x, out int y, out int width, out int height);
		SetSize(width, height);
		SetPos(x, y);
		Repaint();
	}
}
