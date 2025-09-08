using Game.Client.HUD;
using Game.Shared;

using Microsoft.Extensions.DependencyInjection;

using Source.Common.Bitbuffers;
using Source.Common.Client;
using Source.Common.Engine;

namespace Game.Client;

public class HLClient(ViewRender view, IInput input, Hud HUD, UserMessages usermessages) : IBaseClientDLL
{
	public static void DLLInit(IServiceCollection services) {
		services.AddSingleton<IInput, HLInput>();
		services.AddSingleton<ViewRender>();
		services.AddSingleton<Hud>();
		services.AddSingleton<HudElementHelper>();
		services.AddSingleton<IViewRender>(x => x.GetRequiredService<ViewRender>());
	}

	public void IN_SetSampleTime(double frameTime) {

	}

	public void PostInit() {

	}

	public void CreateMove(int sequenceNumber, double inputSampleFrametime, bool active) {
		input.CreateMove(sequenceNumber, inputSampleFrametime, active);
	}

	public bool WriteUsercmdDeltaToBuffer(bf_write buf, int from, int to, bool isNewCommand) {
		return input.WriteUsercmdDeltaToBuffer(buf, from, to, isNewCommand);
	}
	public bool DisconnectAttempt() => false;

	public void HudText(ReadOnlySpan<char> text) {

	}

	public bool DispatchUserMessage(int msgType, bf_read msgData) {
		return usermessages.DispatchUserMessage(msgType, msgData);
	}

	public bool Init() {
		HUD.Init();
		view.Init();
		input.Init();
		return true;
	}

	public void EncodeUserCmdToBuffer(bf_write buf, int slot) {
		input.EncodeUserCmdToBuffer(buf, slot);
	}

	public void DecodeUserCmdFromBuffer(bf_read buf, int slot) {
		input.DecodeUserCmdFromBuffer(buf, slot);
	}

	public bool HandleUiToggle() {
		return false;
	}

	public void IN_DeactivateMouse() {

	}

	public void IN_ActivateMouse() {

	}

	public void View_Render(ViewRects rects) {
		ref ViewRect rect = ref rects[0];
		if (rect.Width == 0 || rect.Height == 0)
			return;
		view.Render(rects);
	}
}
