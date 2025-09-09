using Game.Shared;

using Microsoft.Extensions.DependencyInjection;

using Source.Common.Bitbuffers;
using Source.Common.Client;
using Source.Common.Engine;
using Source.Common.Input;
using Source.Common.Mathematics;
using Source.Common.Networking;
using Source.Engine.Client;

using System.Runtime.CompilerServices;

namespace Game.Client;

public class HLInput(IServiceProvider provider, IClientMode ClientMode) : Input(provider, ClientMode)
{
	
}
