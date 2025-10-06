
using Source.Common.GUI;
using Source.GUI.Controls;

namespace Game.Client;

public static class ClientVGui
{
	internal static void CreateGlobalPanels() {
		IPanel toolParent = enginevgui.GetPanel(Source.Engine.VGuiPanelType.Tools);
		IFPSPanel.FPS.Create(toolParent);
	}
}