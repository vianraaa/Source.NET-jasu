using Source.Common.Client;
using Source.Common.GUI;

namespace Game.Client;

public class HLInput(IServiceProvider provider, ISurface surface, IViewRender view, ThirdPersonManager tpm) : Input(provider, surface, view, tpm)
{
	
}
