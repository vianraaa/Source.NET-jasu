using Microsoft.Extensions.DependencyInjection;

using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Common.Input;
using Source.Common.Launcher;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Source.Engine;

public delegate void GameMessageFn(in InputEvent ev);
public struct GameMessageHandler
{
	public InputEventType EventType;
	public GameMessageFn Function;
	public GameMessageHandler(InputEventType eventType, GameMessageFn function) {
		EventType = eventType;
		Function = function;
	}
}

public class Game(ILauncherManager? launcherManager, Sys Sys, IFileSystem fileSystem, IInputSystem inputSystem, IMatSystemSurface surface, IEngine eng, IServiceProvider services) : IGame
{
	GameMessageHandler[] GameMessageHandlers;

	public void HandleMsg_ActivateApp(in InputEvent ev) { }
	public void HandleMsg_WindowMove(in InputEvent ev) { }
	public void HandleMsg_Close(in InputEvent ev) {
		if (eng.GetState() == IEngine.State.Active)
			eng.SetQuitting(IEngine.Quit.ToDesktop);
	}

	public bool CreateGameWindow(int width, int height, bool windowed) {
		GameMessageHandlers = [
			new(InputEventType.App_AppActivated, HandleMsg_ActivateApp),
			new(InputEventType.App_WindowMove, HandleMsg_WindowMove),
			new(InputEventType.App_Close, HandleMsg_Close),
			new(InputEventType.System_Quit, HandleMsg_Close),
		];

		if (launcherManager == null) {
			Sys.Error("Tried to call Game.CreateGameWindow without a valid ILauncherManager implementation.");
			return false;
		}

		string windowName = "HALF-LIFE 2";
		{
			KeyValues modinfo = new();
			if (modinfo.LoadFromFile(fileSystem, "gameinfo.txt")) {
				string name = new string(modinfo.GetString("game"));
				if (!string.IsNullOrEmpty(name))
					windowName = name;
			}
		}

		Console.Title = windowName;
		if (!launcherManager.CreateGameWindow(windowName, windowed, width, height))
			return false;

		SetGameWindow(launcherManager.GetWindow());
		AttachToWindow();

		return true;
	}

	private void AttachToWindow() {
		inputSystem.AttachToWindow(window);
		inputSystem.EnableInput(true);
		inputSystem.EnableMessagePump(false);

		surface.AttachToWindow(window, true);
		surface.EnableWindowsMessages(true);
	}

	IWindow window;

	public void DestroyGameWindow() {
		throw new NotImplementedException();
	}

	public void DispatchAllStoredGameMessages() {
		foreach (var ev in inputSystem.GetEventData()) {
			DispatchInputEvent(in ev);
		}
	}

	IEngineVGui? engineVGui;
	[MemberNotNull(nameof(engineVGui))]
	public void FetchEngineVGui() => engineVGui ??= services.GetRequiredService<IEngineVGui>();

	private void DispatchInputEvent(in InputEvent ev) {
		switch (ev.Type) {
			case InputEventType.IE_ButtonPressed:
			case InputEventType.IE_ButtonDoubleClicked:
			case InputEventType.IE_ButtonReleased:
				Key_Event(in ev);
				break;
			default:
				if (surface?.HandleInputEvent(in ev) ?? false)
					break;

				foreach (GameMessageHandler h in GameMessageHandlers) {
					if (h.EventType == ev.Type) {
						h.Function(in ev);
						break;
					}
				}

				break;
		}
	}

	struct KeyInfo_t
	{
		public string KeyBinding;
		public KeyUpTarget KeyUpTarget;
		public bool KeyDown;
	}

	enum KeyUpTarget
	{
		AnyTarget,
		Engine,
		VGui,
		Tools,
		Client
	}

	KeyInfo_t[] KeyInfo = new KeyInfo_t[(int)ButtonCode.Last];
	bool TrapMode = false;
	bool DoneTrapping = false;
	ButtonCode TrapKeyUp = ButtonCode.Invalid;
	ButtonCode TrapKey = ButtonCode.Invalid;
	delegate bool KeyFilterDelegate(in InputEvent ev);


	private void Key_Event(in InputEvent ev) {
#if SWDS
		return;
#endif

		bool down = ev.Type != InputEventType.IE_ButtonReleased;
		ButtonCode code = (ButtonCode)ev.Data;

		if (KeyInfo[(int)code].KeyDown == down)
			return;

		if (FilterTrappedKey(code, down))
			return;

		FetchEngineVGui();
		engineVGui.UpdateButtonState(in ev);

		if (FilterKey(in ev, KeyUpTarget.Tools, HandleToolKey))
			return;

		if (FilterKey(in ev, KeyUpTarget.Tools, HandleVGuiKey))
			return;

		if (FilterKey(in ev, KeyUpTarget.Tools, HandleClientKey))
			return;

		FilterKey(in ev, KeyUpTarget.Engine, HandleEngineKey);
	}

	private bool HandleToolKey(in InputEvent ev) {
		// TODO: Tools
		return false;
	}

	private bool HandleVGuiKey(in InputEvent ev) {
		bool down = ev.Type != InputEventType.IE_ButtonReleased;
		ButtonCode code = (ButtonCode)ev.Data;

		FetchEngineVGui();
		return engineVGui.Key_Event(in ev);
	}

	private bool HandleClientKey(in InputEvent ev) {
		// TODO: Client
		return false;
	}

	private bool HandleEngineKey(in InputEvent ev) {
		// TODO: Engine
		return false;
	}

	private bool FilterKey(in InputEvent ev, KeyUpTarget target, KeyFilterDelegate func) {
		bool down = ev.Type != InputEventType.IE_ButtonReleased;
		ButtonCode code = (ButtonCode)ev.Data;

		if (!down && !ShouldPassKeyUpToTarget(code, target))
			return false;

		bool filtered = func(in ev);

		if (down) {
			if (filtered) {
				Assert(KeyInfo[(int)code].KeyUpTarget == KeyUpTarget.AnyTarget);
				KeyInfo[(int)code].KeyUpTarget = target;
			}
		}
		else {
			if (KeyInfo[(int)code].KeyUpTarget == target) {
				KeyInfo[(int)code].KeyUpTarget = KeyUpTarget.AnyTarget;
				filtered = true;
			}
			else {
				Assert(!filtered);
			}
		}

		return filtered;
	}

	private bool ShouldPassKeyUpToTarget(ButtonCode code, KeyUpTarget target)
		=> (KeyInfo[(int)code].KeyUpTarget == target) || (KeyInfo[(int)code].KeyUpTarget == KeyUpTarget.AnyTarget);

	private bool FilterTrappedKey(ButtonCode code, bool down) {
		if (TrapKeyUp == code && !down) {
			TrapKeyUp = ButtonCode.Invalid;
			return true;
		}

		if (TrapMode && down) {
			TrapKey = code;
			TrapMode = false;
			DoneTrapping = true;
			TrapKeyUp = code;
			return true;
		}

		return false;
	}

	public void GetDesktopInfo(out int width, out int height, out int refreshrate) {
		throw new NotImplementedException();
	}

	public IWindow GetMainDeviceWindow() {
		return window;
	}

	public nint GetMainWindow() {
		throw new NotImplementedException();
	}

	public nint GetMainWindowAddress() {
		throw new NotImplementedException();
	}

	public void GetWindowRect(out int x, out int y, out int w, out int h) {
		throw new NotImplementedException();
	}
	bool ExternallySuppliedWindow = false;
	public bool InputAttachToGameWindow() {
		if (!ExternallySuppliedWindow)
			return true;

		AttachToWindow();
		return true;
	}

	public void InputDetachFromGameWindow() {
		throw new NotImplementedException();
	}

	public bool IsActiveApp() {
		throw new NotImplementedException();
	}

	public void PlayStartupVideos() {
		throw new NotImplementedException();
	}

	public void SetGameWindow(IWindow window) {
		this.window = window;
	}

	public void SetWindowSize(int w, int h) {

	}

	public void SetWindowXY(int x, int y) {

	}
}
