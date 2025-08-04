using Microsoft.Extensions.DependencyInjection;

using Source.Common;

namespace Source.Engine;

public class EngineAPI(IServiceProvider provider) : IEngineAPI, IDisposable
{
	public void Dispose() => ((IDisposable)provider).Dispose();

	public IEngineAPI.RunResult Run() {

		return IEngineAPI.RunResult.OK;
	}
}

public class EngineBuilder : ServiceCollection {

	public EngineAPI Build() {
		this.AddSingleton<EngineAPI>();
		ServiceProvider provider = this.BuildServiceProvider();
		EngineAPI api = provider.GetRequiredService<EngineAPI>();
		return api;
	}
}