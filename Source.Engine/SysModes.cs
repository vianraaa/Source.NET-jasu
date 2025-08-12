using Microsoft.Extensions.DependencyInjection;

using Source.Common.Engine;
using Source.Common.Launcher;
using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;


public class VideoMode_Common(IServiceProvider services) : IVideoMode
{
	VMode mode = new();
	bool Windowed;
	bool ClientViewRectDirty;
	int ModeWidth;
	int ModeHeight;
	int ModeBPP;
	int UIWidth;
	int UIHeight;
	int StereoWidth;
	int StereoHeight;

	public ref VMode RequestedWindowVideoMode() {
		return ref mode;
	}

	public void ResetCurrentModeForNewResolution(int width, int height, bool windowed) {
		ref VMode mode = ref RequestedWindowVideoMode();
		ModeWidth = mode.Width;
		ModeHeight = mode.Height;
		UIWidth = mode.Width;
		UIHeight = mode.Height;
		StereoWidth = mode.Width;
		StereoHeight = mode.Height;
	}

	public bool IsWindowedMode() => Windowed;
	public int GetModeWidth() => ModeWidth;
	public int GetModeHeight() => ModeHeight;
	public int GetModeBPP() => ModeBPP;
	public int GetModeStereoWidth() =>  StereoWidth;
	public int GetModeStereoHeight() => StereoHeight;
	public int GetModeUIWidth() =>  UIWidth;
	public int GetModeUIHeight() => UIHeight;

	public void AdjustWindow(int width, int height, int bpp, bool windowed) {
		IGame game = services.GetRequiredService<IGame>();
		ILauncherManager launcherMgr = services.GetRequiredService<ILauncherManager>();

		Rectangle windowRect = Rectangle.FromLTRB(
			0,
			0,
			width,
			height
			);

		game.SetWindowSize(width, height);
		launcherMgr.CenterWindow(windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top);
	}

	public void MarkClientViewRectDirty() => ClientViewRectDirty = true;

	public bool CreateGameWindow(int width, int height, bool windowed) {
		if (width != 0 && height != 0 && windowed) {
			ref VMode requested = ref RequestedWindowVideoMode();
			requested.Width = width;
			requested.Height = height;
		}

		if (true) { // InEditMode(), we aren't doing edit mode right now, so just true
			ResetCurrentModeForNewResolution(width, height, windowed);

			IGame game = services.GetRequiredService<IGame>();
			if (!game.CreateGameWindow(width, height, windowed))
				return false;

			AdjustWindow(GetModeWidth(), GetModeHeight(), GetModeBPP(), IsWindowedMode());
			if (!SetMode(GetModeWidth(), GetModeHeight(), IsWindowedMode()))
				return false;

			DrawStartupGraphic();
		}

		return true;
	}

	public void DrawStartupGraphic() {

	}

	public void SetGameWindow(nint window) {
		throw new NotImplementedException();
	}

	public virtual bool SetMode(int width, int height, bool windowed) {
		return false;
	}
}
public class VideoMode_MaterialSystem(IMaterialSystem materials, IGame game, IServiceProvider services) : VideoMode_Common(services)
{
	bool SetModeOnce;
	public override bool SetMode(int width, int height, bool windowed) {
		ref VMode mode = ref RequestedWindowVideoMode();
		MaterialSystemConfig config = services.GetRequiredService<MaterialSystemConfig>();
		config.Width = mode.Width;
		config.Height = mode.Height;
		config.RefreshRate = mode.RefreshRate;

		if (!SetModeOnce) {
			if (!materials.SetMode(game.GetMainDeviceWindow(), config)) {

			}
			SetModeOnce = true;

			InitStartupScreen();
			return true;
		}

		return true;
	}

	private void InitStartupScreen() {

	}
}