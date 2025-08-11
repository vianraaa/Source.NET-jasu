using Microsoft.Extensions.DependencyInjection;

using Source.Common;

namespace Source.VTF;


[EngineComponent]
public static class SourceDllMain
{
	static bool bootstrapped;
	public static void Link(IServiceCollection services) {
		// Don't do it more than once per engine startup.
		// ie. engine restarts should not trigger this linkage again
		if (bootstrapped)
			return;

		// Plug ourselves into the class factory for IVTFTexture
		IVTFTexture.OnCreate += CreateVTFTexture;
		IVTFTexture.OnDestroy += DestroyVTFTexture;

		bootstrapped = true;
	}

	private static IVTFTexture CreateVTFTexture() {
		return new VTFTexture();
	}

	private static void DestroyVTFTexture(IVTFTexture texture) {
		if (texture is not VTFTexture vtf)
			throw new InvalidCastException();

		vtf.Dispose();
	}
}