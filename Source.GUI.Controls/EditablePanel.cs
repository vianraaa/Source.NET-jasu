using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;

namespace Source.GUI.Controls;

public class EditablePanel : Panel {
	[Imported] public IFileSystem fileSystem;

	public EditablePanel(Panel? parent, string? panelName, bool showTaskbarIcon = true) : base(parent, panelName, showTaskbarIcon) {
	}

	public virtual void ApplySettings(KeyValues resourceData) {

	}
	public virtual void LoadControlSettings(ReadOnlySpan<char> resourceName, ReadOnlySpan<char> pathID, KeyValues keyValues, KeyValues conditions) {
		// todo
	}
}
