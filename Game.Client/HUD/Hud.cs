using Game.Shared;

using Source;
using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;
using Source.GUI.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.HUD;

[EngineComponent]
public class Hud(HudElementHelper HudElementHelper, IFileSystem filesystem)
{
	public readonly List<IHudElement> HudList = [];
	internal InButtons KeyBits;

	public void Init() {
		HudElementHelper.CreateAllElements(this);
		foreach (var element in HudList)
			element.Init();

		KeyValues kv = new KeyValues("layout");
		if (kv.LoadFromFile(filesystem, "scripts/HudLayout.res")) {
			int numelements = HudList.Count;

			for (int i = 0; i < numelements; i++) {
				IHudElement element = HudList[i];

				if (element is not Panel panel) {
					Msg($"Non-vgui hud element {HudList[i].ElementName}\n");
					continue;
				}

				KeyValues? key = kv.FindKey(panel.GetName(), false);
				if (key == null) 
					Msg($"Hud element '{element.ElementName}' doesn't have an entry '{panel.GetName()}' in scripts/HudLayout.res\n");
				
				if (!element.IsParentedToClientDLLRootPanel && panel.GetParent() == null) 
					DevMsg($"Hud element '{element.ElementName}'/'{panel.GetName()}' doesn't have a parent\n");
			}

		}
	}
	internal void AddHudElement(IHudElement element) {
		HudList.Add(element);
		element.NeedsRemove = true;
	}

	public IHudElement? FindElement(ReadOnlySpan<char> name) {
		foreach (var hudElement in HudList) {
			if (name.Equals(hudElement.ElementName, StringComparison.OrdinalIgnoreCase))
				return hudElement;
		}

		DevWarning(1, $"Could not find HUD element: {name}\n");
		Assert(false);
		return null;
	}

	public int GetSensitivity() {
		return 0;
	}

	internal void ResetHUD() {
		HLClient.ClientMode!.GetViewportAnimationController()!.CancelAllAnimations();
	}

	internal void RefreshHudTextures() {
	}
}

public class HudElementHelper
{
	Hud HUD;
	public void CreateAllElements(Hud HUD) {
		this.HUD = HUD;
		var declaredHudElements = ReflectionUtils.GetLoadedTypesWithAttribute<DeclareHudElementAttribute>();
		foreach (var kvp in declaredHudElements) {
			Type type = kvp.Key;
			DeclareHudElementAttribute hudElement = kvp.Value;
			string name = hudElement.Name ?? type.Name;

			IHudElement? element = Activator.CreateInstance(type, [name]) as IHudElement;
			if (element != null)
				HUD.AddHudElement(element);
		}
	}
}