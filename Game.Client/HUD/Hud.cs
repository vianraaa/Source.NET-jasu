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

	public void Init() {
		HudElementHelper.CreateAllElements(this);
	}

	internal void AddHudElement(IHudElement element) {
		HudList.Add(element);
		element.NeedsRemove = true;
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