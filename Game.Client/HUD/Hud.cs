using Game.Shared;

using Source;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.HUD;

[EngineComponent]
public class Hud(HudElementHelper HudElementHelper)
{
	public readonly List<IHudElement> HudList = [];
	internal InButtons KeyBits;

	public void Init() {
		HudElementHelper.CreateAllElements(this);
		foreach (var element in HudList)
			element.Init();
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
		foreach(var kvp in declaredHudElements) {
			Type type = kvp.Key;
			DeclareHudElementAttribute hudElement = kvp.Value;
			string name = hudElement.Name ?? type.Name;

			IHudElement? element = Activator.CreateInstance(type, [name]) as IHudElement;
			if (element != null) 
				HUD.AddHudElement(element);
		}
	}
}