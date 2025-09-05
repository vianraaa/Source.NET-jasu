using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;
using Source.Common.GUI;

namespace Source.GUI.Controls;

public class EditablePanel : Panel {
	[Imported] public IFileSystem fileSystem;

	public EditablePanel(Panel? parent, string? panelName, bool showTaskbarIcon = true) : base(parent, panelName, showTaskbarIcon) {
		NavGroup = EngineAPI.New<FocusNavGroup>();
	}

	public override void ApplySettings(KeyValues resourceData) {

	}
	public virtual void LoadControlSettings(ReadOnlySpan<char> resourceName, ReadOnlySpan<char> pathID, KeyValues keyValues, KeyValues conditions) {
		// todo
	}
	public override IPanel? GetCurrentKeyFocus() {
		Panel focus = NavGroup.GetCurrentFocus();
		if (focus == this)
			return null;

		if(focus != null) {
			if (focus.IsPopup())
				return base.GetCurrentKeyFocus();

			IPanel? subFocus = focus.GetCurrentKeyFocus();
			if (subFocus != null)
				return subFocus;

			return focus;
		}

		return base.GetCurrentKeyFocus();
	}
	public override void OnRequestFocus(Panel subFocus, Panel? defaultPanel) {
		if (!subFocus.IsPopup())
			defaultPanel = NavGroup.SetCurrentFocus(subFocus, defaultPanel);

		base.OnRequestFocus(subFocus, defaultPanel);
	}

	public FocusNavGroup GetFocusNavGroup() => NavGroup;

	readonly FocusNavGroup NavGroup;
}
public class FocusNavGroup
{
	[Imported] public IVGui VGui;
	readonly WeakReference<Panel?> DefaultButton = new(null);
	readonly WeakReference<Panel?> CurrentDefaultButton = new(null);
	readonly WeakReference<Panel?> CurrentFocus = new(null);

	internal Panel? SetCurrentFocus(Panel focus, Panel? defaultPanel) {
		CurrentFocus.SetTarget(focus);
		if(defaultPanel == null) {
			if (CanButtonBeDefault(focus))
				defaultPanel = focus;
			else if (DefaultButton.TryGetTarget(out Panel? def))
				defaultPanel = def;
		}

		SetCurrentDefaultButton(defaultPanel);
		return defaultPanel;
	}


	private bool CanButtonBeDefault(Panel panel) {
		if (panel == null)
			return false;

		KeyValues data = new("CanBeDefaultButton");
		bool result = false;
		if (panel.RequestInfo(data)) 
			result = (data.GetInt("result") == 1);
		
		return result;
	}

	private void SetCurrentDefaultButton(Panel? panel, bool sendCurrentDefaultButtonMessage = true) {
		CurrentDefaultButton.TryGetTarget(out Panel? currentDefaultButton);

		if (panel == currentDefaultButton)
			return;

		if (sendCurrentDefaultButtonMessage && currentDefaultButton != null) 
			VGui.PostMessage(currentDefaultButton, new KeyValues("SetAsCurrentDefaultButton", "state", 0), null);
		
		CurrentDefaultButton.SetTarget(panel);

		if (sendCurrentDefaultButtonMessage && currentDefaultButton != null) 
			VGui.PostMessage(currentDefaultButton, new KeyValues("SetAsCurrentDefaultButton", "state", 1), null);
	}

	public Panel? GetCurrentFocus() => CurrentFocus.TryGetTarget(out Panel? t) ? t : null;
}
