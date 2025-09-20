using Source.Common;

namespace Game.Client;

public interface IHasLocalToGlobalFlexSettings;
public partial class C_BaseFlex : C_BaseAnimatingOverlay, IHasLocalToGlobalFlexSettings {
	public static readonly RecvTable DT_BaseFlex = new(DT_BaseAnimatingOverlay, [

	]);
	public static readonly new ClientClass ClientClass = new ClientClass("BaseFlex", null, null, DT_BaseFlex);
}