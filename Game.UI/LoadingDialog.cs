using Source.Common;
using Source.Common.Engine;
using Source.Common.Formats.Keyvalues;
using Source.Common.GameUI;
using Source.GUI.Controls;

namespace Game.UI;

public class LoadingDialog : Frame
{
	[Imported] public IGameUI GameUI;
	[Imported] public ModInfo ModInfo;

	ProgressBar Progress;
	ProgressBar Progress2;
	Label InfoLabel;
	Label TimeRemainingLabel;
	Button CancelButton;
	Panel? LoadingBackground;

	bool ShowingSecondaryProgress;
	float SecondaryProgress;
	double LastSecondaryProgressUpdateTime;
	double SecondaryProgressStartTime;
	bool ShowingVACInfo;
	bool Center;
	bool ConsoleStyle;
	float ProgressFraction;

	public LoadingDialog(Panel? parent = null) : base(parent, "LoadingDialog") {
		SetDeleteSelfOnClose(true);

		SetSize(416, 100);
		SetTitle("#GameUI_Loading", true);

		Center = GameUI!.HasLoadingBackgroundDialog();

		ShowingSecondaryProgress = false;
		SecondaryProgress = 0.0f;
		LastSecondaryProgressUpdateTime = 0.0f;
		SecondaryProgressStartTime = 0.0f;

		Progress = EngineAPI.New<ProgressBar>(this, "Progress");
		Progress2 = EngineAPI.New<ProgressBar>(this, "Progress2");
		InfoLabel = EngineAPI.New<Label>(this, "InfoLabel", "");
		CancelButton = EngineAPI.New<Button>(this, "CancelButton", "#GameUI_Cancel");
		TimeRemainingLabel = EngineAPI.New<Label>(this, "TimeRemainingLabel", "");
		CancelButton.SetCommand("Cancel");

		if (ModInfo!.IsSinglePlayerOnly() == false && ConsoleStyle == true)
			LoadingBackground = new Panel(this, "LoadingDialogBG");
		else
			LoadingBackground = null;

		SetMinimizeButtonVisible(false);
		SetMaximizeButtonVisible(false);
		SetCloseButtonVisible(false);
		SetSizeable(false);
		SetMoveable(false);

		InfoLabel.SetBounds(20, 32, 392, 24);
		Progress.SetBounds(20, 64, 300, 24);
		CancelButton.SetBounds(330, 64, 72, 24);
		Progress2.SetVisible(false);

		SetupControlSettings(false);
	}

	private void SetupControlSettings(bool forceShowProgressText) {
		ShowingVACInfo = false;

		if (ModInfo.IsSinglePlayerOnly() && !forceShowProgressText) 
			LoadControlSettings("Resource/LoadingDialogNoBannerSingle.res");
		else 
			LoadControlSettings("Resource/LoadingDialogNoBanner.res");
	}

	internal void DisplayGenericError(ReadOnlySpan<char> failureReason, ReadOnlySpan<char> extendedReason) {
		throw new NotImplementedException();
	}

	internal void Open() {
		throw new NotImplementedException();
	}

	internal bool SetProgressPoint(float progress) {
		throw new NotImplementedException();
	}

	internal void SetStatusText(ReadOnlySpan<char> statusText) {
		throw new NotImplementedException();
	}
}
