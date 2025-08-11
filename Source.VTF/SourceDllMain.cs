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
		IVTFTexture.OnRequestSize += VTFFileHeaderSize;

		bootstrapped = true;
	}

	private static IVTFTexture CreateVTFTexture() {
		return new VTFTexture();
	}

	private static void DestroyVTFTexture(IVTFTexture texture) {
		if (texture is VTFTexture vtf)
			vtf.Dispose();
	}

	private static unsafe ushort VTFFileHeaderSize(int majorVersion, int minorVersion) {
		if (majorVersion == -1)
			majorVersion = IVTFTexture.VTF_MAJOR_VERSION;
		if(minorVersion == -1)
			minorVersion = IVTFTexture.VTF_MINOR_VERSION;

		switch (majorVersion) {
			case IVTFTexture.VTF_MAJOR_VERSION:
				switch (minorVersion) {
					case 0:
					case 1:
						return VTFFileHeaderV7_1.Size;
					case 2:
						return VTFFileHeaderV7_2.Size;
					case 3:
						return (ushort)(VTFFileHeaderV7_3.Size + sizeof(ResourceEntryInfo));
					case IVTFTexture.VTF_MINOR_VERSION:
					case 5:
						ushort size1 = VTFFileBaseHeader.Size;
						ushort size2 = (ushort)(sizeof(ResourceEntryInfo) * (short)HeaderDetails.MaxRSRCDictionaryEntries);
						return (ushort)(size1 + size2);
				}
				break;
		}

		return 0;
	}
}