using Game.Client.HUD;

using Source;
using Source.Common.GUI;
using Source.GUI.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.HL2;

[DeclareHudElement(Name = "CHudHealth")]
public class HudHealth : HudNumericDisplay
{
	protected int Value;
	protected int SecondaryValue;
	protected InlineArray32<char> LabelText;
	bool DisplayValue, DisplaySecondaryValue;
	bool Indent;
	bool IsTime;

	[PanelAnimationVar("0")] protected float Blur;
	[PanelAnimationVar("FgColor")] protected Color TextColor;
	[PanelAnimationVar("FgColor")] protected Color Ammo2Color;
	[PanelAnimationVar("HudNumbers")] protected IFont NumberFont;
	[PanelAnimationVar("HudNumbersGlow")] protected IFont NumberGlowFont;
	[PanelAnimationVar("HudNumbersSmall")] protected IFont SmallNumberFont;
	[PanelAnimationVar("Default")] protected IFont TextFont;

	[PanelAnimationVar("text_xpos", "8", "proportional_float")] protected float ;

	public HudHealth(string? panelName) : base(null, "HudHealth") {
		var parent = HLClient.ClientMode!.GetViewport();
		SetParent(parent);


	}
}
