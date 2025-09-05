using Source.Common.Commands;
using Source.Common.Engine;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;
using Source.Common.Input;

using System.Diagnostics;

namespace Source.GUI.Controls;

public class TabCatchingTextEntry : TextEntry
{
	public TabCatchingTextEntry(Panel? parent, string? name, Panel comp) : base(parent, name) {

	}
	public override void OnKeyCodeTyped(ButtonCode code) {
		if (code == ButtonCode.KeyTab)
			GetParent()!.OnKeyCodeTyped(code);
		else if(code != ButtonCode.KeyEnter)
			base.OnKeyCodeTyped(code);
	}
}
public class ConsolePanel : EditablePanel, IConsoleDisplayFunc
{
	[Imported] public ICvar Cvar;

	internal RichText History;
	internal TextEntry Entry;
	internal Button Submit;
	internal Menu CompletionList;

	protected Color PrintColor, DPrintColor;

	protected int NextCompletion;
	protected char[] PartialText = new char[256];
	protected char[] PreviousPartialText = new char[256];
	protected bool AutoCompleteMode;
	protected bool WasBackspacing;
	protected bool StatusVersion;

	public override void OnTextChanged(Panel p) { 
	
	}

	public override void OnCommand(ReadOnlySpan<char> command) {
		if(command == "Submit") {
			Debugger.Break();
		}
	}

	public ConsolePanel(Panel? parent, string? panelName, bool statusVersion) : base(parent, panelName) {
		StatusVersion = statusVersion;

		SetKeyboardInputEnabled(true);

		if (!StatusVersion)
			SetMinimumSize(100, 100);

		History = EngineAPI.New<RichText>(this, "ConsoleHistory");
		History.SetAllowKeyBindingChainToParent(false);
		History.MakeReadyForUse();

		// History.SetVerticalScrollbar(!m_bStatusVersion);
		// if (StatusVersion)
		// History.SetDrawOffsets(3, 3);

		// History.GotoTextEnd();

		Submit = EngineAPI.New<Button>(this, "ConsoleSubmit", "#Console_Submit");
		Submit.SetCommand("submit");
		Submit.SetVisible(!StatusVersion);

		// var completionList = new NonFocusableMenu(this, "CompletionList");
		// completionList.SetVisible(false);

		Entry = EngineAPI.New<TabCatchingTextEntry>(this, "ConsoleEntry", CompletionList);
		Entry.AddActionSignalTarget(this);
		Entry.SendNewLine(true);
		// completionList.SetFocusPanel(Entry);

		PrintColor = new(216, 222, 211, 255);
		DPrintColor = new(196, 181, 80, 255);

		Entry.SetTabPosition(1);

		AutoCompleteMode = false;

		Cvar!.InstallConsoleDisplayFunc(this);
	}

	public override void OnKeyCodeTyped(ButtonCode code) {
		base.OnKeyCodeTyped(code);

		if (TextEntryHasFocus()) {
			if (code == ButtonCode.KeyTab) {
				bool reverse = false;
				if (Input.IsKeyDown(ButtonCode.KeyLShift) || Input.IsKeyDown(ButtonCode.KeyRShift)) 
					reverse = true;
				
				// OnAutoComplete(reverse);
				Entry.RequestFocus();
			}
			else if (code == ButtonCode.KeyDown) {
				// OnAutoComplete(false);

				Entry.RequestFocus();
			}
			else if (code == ButtonCode.KeyUp) {
				// OnAutoComplete(true);
				Entry.RequestFocus();
			}
		}
	}
	public void ColorPrint(in Color clr, string message) {
		throw new NotImplementedException();
	}

	public void DPrint(string message) {
		throw new NotImplementedException();
	}

	public void Print(string message) {
		throw new NotImplementedException();
	}

	public override void ApplySchemeSettings(IScheme scheme) {
		base.ApplySchemeSettings(scheme);

		PrintColor = GetSchemeColor("Console.TextColor", scheme);
		DPrintColor = GetSchemeColor("Console.DevTextColor", scheme);
		History.SetFont(scheme.GetFont("ConsoleText", IsProportional()));
		// CompletionList.SetFont(scheme.GetFont("DefaultSmall", IsProportional()));

		InvalidateLayout();
	}



	public override void PerformLayout() {
		base.PerformLayout();

		GetFocusNavGroup().SetDefaultButton(Submit);

		IScheme scheme = GetScheme()!;
		Entry.SetBorder(scheme.GetBorder("DepressedButtonBorder"));
		History.SetBorder(scheme.GetBorder("DepressedButtonBorder"));

		GetSize(out int wide, out int tall);

		if (!StatusVersion) {
			const int inset = 8;
			const int entryHeight = 24;
			const int topHeight = 4;
			const int entryInset = 4;
			const int submitWide = 64;
			const int submitInset = 7; 

			History.SetPos(inset, inset + topHeight);
			History.SetSize(wide - (inset * 2), tall - (entryInset * 2 + inset * 2 + topHeight + entryHeight));
			History.InvalidateLayout();

			int nSubmitXPos = wide - (inset + submitWide + submitInset);
			Submit.SetPos(nSubmitXPos, tall - (entryInset * 2 + entryHeight));
			Submit.SetSize(submitWide, entryHeight);

			Entry.SetPos(inset, tall - (entryInset * 2 + entryHeight));
			Entry.SetSize(nSubmitXPos - entryInset - 2 * inset, entryHeight);
		}
		else {
			const int inset = 2;

			int entryWidth = wide / 2;
			if (wide > 400) {
				entryWidth = 200;
			}

			Entry.SetBounds(inset, inset, entryWidth, tall - 2 * inset);

			History.SetBounds(inset + entryWidth + inset, inset, (wide - entryWidth) - inset, tall - 2 * inset);
		}

		UpdateCompletionListPosition();
	}

	private void UpdateCompletionListPosition() {

	}

	public bool TextEntryHasFocus() => Input.GetFocus() == Entry;

	public void TextEntryRequestFocus() => Entry.RequestFocus();

	const int MAX_HISTORY_ITEMS = 500;
	class CompletionItem
	{
		public ReadOnlySpan<char> GetItemText() => null;
		public ReadOnlySpan<char> GetCommand() => null;
		public ReadOnlySpan<char> GetName() => null;
		public bool IsCommand;
		public ConCommandBase? Command;
	}
}

public class ConsoleDialog : Frame
{
	protected ConsolePanel ConsolePanel;

	public ConsoleDialog(Panel? parent, string? name, bool statusVersion) : base(parent, name) {
		SetVisible(false);
		SetTitle("#Console_Title", true);

		ConsolePanel = EngineAPI.New<ConsolePanel>(this, "ConsolePage", statusVersion);
		ConsolePanel.AddActionSignalTarget(this);
	}

	public override void PerformLayout() {
		base.PerformLayout();

		GetClientArea(out int x, out int y, out int w, out int h);
		ConsolePanel.SetBounds(x, y, w, h);
	}

	public override void Activate() {
		base.Activate();
		ConsolePanel.Entry.RequestFocus();
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
