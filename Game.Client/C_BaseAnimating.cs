using Source.Common;
using Source.Common.Engine;

namespace Game.Client;

public partial class C_BaseAnimating : C_BaseEntity, IModelLoadCallback
{
	public void OnModelLoadComplete(Model model) {
		throw new NotImplementedException();
	}
	public override void PostDataUpdate(DataUpdateType updateType) {
		base.PostDataUpdate(updateType);
	}
}
