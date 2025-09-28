namespace Game.Server;

using Game.Client;

using Source.Common;

using FIELD = Source.FIELD<C_EnvTonemapController>;

public class C_EnvTonemapController : C_BaseEntity
{
	public static readonly RecvTable DT_EnvTonemapController = new(DT_BaseEntity, [
		RecvPropBool(FIELD.OF(nameof(UseCustomAutoExposureMin))),
		RecvPropBool(FIELD.OF(nameof(UseCustomAutoExposureMax))),
		RecvPropBool(FIELD.OF(nameof(UseCustomBloomScale))),
		RecvPropFloat(FIELD.OF(nameof(CustomAutoExposureMin))),
		RecvPropFloat(FIELD.OF(nameof(CustomAutoExposureMax))),
		RecvPropFloat(FIELD.OF(nameof(CustomBloomScale))),
		RecvPropFloat(FIELD.OF(nameof(CustomBloomScaleMinimum))),
	]); public static readonly new ClientClass ClientClass = new ClientClass("EnvTonemapController", null, null, DT_EnvTonemapController);

	public bool UseCustomAutoExposureMin;
	public bool UseCustomAutoExposureMax;
	public bool UseCustomBloomScale;
	public float CustomAutoExposureMin;
	public float CustomAutoExposureMax;
	public float CustomBloomScale;
	public float CustomBloomScaleMinimum;
}