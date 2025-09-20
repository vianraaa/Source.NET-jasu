using Source.Common;
using Source.Common.Engine;

namespace Game.Client;

public partial class C_BaseAnimating : C_BaseEntity, IModelLoadCallback
{
	public static readonly RecvTable DT_BaseAnimating = new(DT_BaseEntity, [

	]);
	public static readonly new ClientClass ClientClass = new ClientClass("BaseAnimating", null, null, DT_BaseAnimating);

	public void OnModelLoadComplete(Model model) {
		throw new NotImplementedException();
	}
	public override void PostDataUpdate(DataUpdateType updateType) {
		base.PostDataUpdate(updateType);
	}
}
