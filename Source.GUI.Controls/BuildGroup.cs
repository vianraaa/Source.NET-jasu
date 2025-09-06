using Source.Common.Commands;
using Source.Common.Engine;
using Source.Common.Filesystem;
using Source.Common.Formats.Keyvalues;

using System.Linq;

namespace Source.GUI.Controls;

public class BuildGroup
{
	readonly public IFileSystem fileSystem = Singleton<IFileSystem>();
	readonly public ICommandLine CommandLine = Singleton<ICommandLine>();

	public string? ResourceName;
	public string? ResourcePathID;
	public Panel? BuildContext;
	public Panel? ParentPanel;

	public BuildGroup(Panel? parentPanel, Panel? contextPanel) {
		this.ParentPanel = parentPanel;
		this.BuildContext = contextPanel;	
	}

	readonly Dictionary<ulong, KeyValues?> DictCachedResFiles = [];
	static ConVar vgui_cache_res_files = new("1", 0);
	internal void LoadControlSettings(ReadOnlySpan<char> controlResourceName, ReadOnlySpan<char> pathID, KeyValues? preloadedKeyValues, KeyValues? conditions) {
		RegisterControlSettingsFile(controlResourceName, pathID);

		KeyValues? dat = preloadedKeyValues;

		bool usePrecaching = vgui_cache_res_files.GetBool();
		bool usingPrecachedSourceKeys = false;
		bool shouldCacheKeys = true;
		bool deleteKeys = false;

		if (dat != null && usePrecaching) {
			if (DictCachedResFiles.TryGetValue(controlResourceName.Hash(), out KeyValues? cachedData)) {
				dat = cachedData;
				usingPrecachedSourceKeys = true;
				deleteKeys = false;
				shouldCacheKeys = false;
			}
		}

		while (dat == null) {
			if (usePrecaching) {
				if (DictCachedResFiles.TryGetValue(controlResourceName.Hash(), out KeyValues? cachedData)) {
					dat = cachedData;
					usingPrecachedSourceKeys = true;
					deleteKeys = false;
					shouldCacheKeys = false;
					break;
				}
			}

			dat = new KeyValues(controlResourceName);

			bool bSuccess = false;
			if (pathID == null)
				bSuccess = dat.LoadFromFile(fileSystem, controlResourceName, "SKIN");

			if (!bSuccess) 
				bSuccess = dat.LoadFromFile(fileSystem, controlResourceName, pathID);

			if (bSuccess) {
				deleteKeys = true;
				shouldCacheKeys = true;
			}
			else 
				Warning($"Failed to load {controlResourceName}\n");
			
			break;
		}

		if (conditions != null && conditions.GetFirstSubKey() != null) {
			if (usingPrecachedSourceKeys) {
				dat = dat.MakeCopy();
				deleteKeys = true;
			}

			ProcessConditionalKeys(dat, conditions);
			shouldCacheKeys = false;
		}


		ResourceName = new(controlResourceName);

		if (pathID != null) 
			ResourcePathID = new(pathID);

		DeleteAllControlsCreatedByControlSettingsFile();

		ApplySettings(dat);

		if (ParentPanel != null) {
			ParentPanel.InvalidateLayout();
			ParentPanel.Repaint();
		}

		if (shouldCacheKeys && usePrecaching) {
			Assert(!DictCachedResFiles.ContainsKey(controlResourceName.Hash()));
			DictCachedResFiles[controlResourceName.Hash()] = dat;
		}
		else if (deleteKeys) {
			Assert(DictCachedResFiles.ContainsKey(controlResourceName.Hash()) || conditions != null || preloadedKeyValues != null);
			DictCachedResFiles[controlResourceName.Hash()] = null;
		}
	}

	private void ApplySettings(KeyValues? dat) {

	}

	private void DeleteAllControlsCreatedByControlSettingsFile() {

	}

	private void ProcessConditionalKeys(KeyValues? dat, KeyValues conditions) {

	}

	private void RegisterControlSettingsFile(ReadOnlySpan<char> controlResourceName, ReadOnlySpan<char> pathID) {

	}
}
