using Source.Common.Engine;
using Source.Common.Formats.Keyvalues;

using static System.Net.Mime.MediaTypeNames;

namespace Source.GUI.Controls;

public class MessageBox : Frame
{
	protected Button OkButton;
	protected Button CancelButton;
	protected Label MessageLabel;

	protected KeyValues? OkCommand;
	protected KeyValues? CancelCommand;
	Frame? FrameOver;
	bool NoAutoClose;
	bool ShowMessageBoxOverCursor;

	public MessageBox(string title, string text, Panel? parent = null) : base(parent, null, false) {
		SetTitle(title, true);
		MessageLabel = EngineAPI.New<Label>(this, null, text);
		Init();
	}

	public MessageBox(ReadOnlySpan<char> title, ReadOnlySpan<char> text, Panel? parent = null) : base(parent, null, false) {
		SetTitle(title, true);
		MessageLabel = EngineAPI.New<Label>(this, null, new string(text));
		Init();
	}

	void Init() {
		SetDeleteSelfOnClose(true);

		FrameOver = null;
		ShowMessageBoxOverCursor = false;

		SetMenuButtonResponsive(false);
		SetMinimizeButtonVisible(false);
		SetCloseButtonVisible(false);
		SetSizeable(false);

		OkButton = EngineAPI.New<Button>(this, null, "#MessageBox_OK");
		OkButton.SetCommand("OnOk");
		OkButton.AddActionSignalTarget(this);

		CancelButton = EngineAPI.New<Button>(this, null, "#MessageBox_Cancel");
		CancelButton.SetCommand("OnCancel");
		CancelButton.AddActionSignalTarget(this);
		CancelButton.SetVisible(false);

		OkCommand = CancelCommand = null;
		NoAutoClose = false;
	}

	public void SetOKButtonVisible(bool state) => OkButton.SetVisible(state);
	public void SetCancelButtonVisible(bool state) => CancelButton.SetVisible(state);
	public void SetOKButtonText(ReadOnlySpan<char> buttonText) {
		OkButton.SetText(buttonText);
		InvalidateLayout();
	}
	public void SetCancelButtonText(ReadOnlySpan<char> buttonText) {
		CancelButton.SetText(buttonText);
		InvalidateLayout();
	}
	public void SetOKCommand(KeyValues? command) => OkCommand = command;
	public void SetCancelCommand(KeyValues? command) => CancelCommand = command;
	public void DoModal(Frame? frameOver = null) {
		ShowWindow(frameOver);
		Input.SetAppModalSurface(this);
	}

	public void ShowWindow(Frame? frameOver) {
		FrameOver = frameOver;

		SetVisible(true);
		SetEnabled(true);
		MoveToFront();

		if (OkButton.IsVisible())
			OkButton.RequestFocus();
		else
			RequestFocus();

		InvalidateLayout();
	}
}
