using Source.Common.Input;

namespace Source.GUI.Controls;

public class ConsoleDialog : Frame
{
	public ConsoleDialog(Panel? parent, string? name, bool statusVersion) : base(parent, name) {

	}

	public override void PerformLayout() {
		base.PerformLayout();
	}
	public void Clear() { }
	public void Hide() { }

	public void Print(ReadOnlySpan<char> msg) { }
	public void DPrint(ReadOnlySpan<char> msg) { }
	public void ColorPrint(in Color clr, ReadOnlySpan<char> msg) { }
	public void DumpConsoleTextToFile() { }

	public override void OnKeyCodePressed(ButtonCode code) {
		base.OnKeyCodePressed(code);
	}
}
