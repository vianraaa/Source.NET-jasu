using Source.Common.Engine;
using Source.Common.Networking;

namespace Source.Engine;

public enum LevelLoadingProgress
{
	None,
	ChangeLevel,
	SpawnServer,
	LoadWorldModel,
	CrcMap,
	CrcClientDll,
	CreateNetworkStringTables,
	PrecacheWorld,
	ClearWorld,
	LevelInit,
	Precache,
	ActivateServer,

	BeginConnect,
	SignOnChallenge,
	SignOnConnect,
	SignOnConnected,
	ProcessServerInfo,
	ProcessStringTable,
	SignOnNew,
	SendClientInfo,
	SendSignonData,
	SignOnSpawn,
	FullyConnected,
	ReadyToPlay
}

public struct LoadingProgressDescription
{
	public LevelLoadingProgress Progress;
	public int Percent;
	public int Repeat;
	public string Description;

	public LoadingProgressDescription(LevelLoadingProgress progress, int percent, int repeat, ReadOnlySpan<char> desc) {
		Progress = progress;
		Percent = percent;
		Repeat = repeat;
		Description = new(desc);
	}
}

public interface IEngineVGuiInternal : IEngineVGui
{
	// Level loading
	public void OnLevelLoadingStarted();
	public void OnLevelLoadingFinished();
	public void NotifyOfServerConnect(ReadOnlySpan<char> game, int IP, int connectionPort, int queryPort);
	public void NotifyOfServerDisconnect();
	public void EnabledProgressBarForNextLoad();
	public void UpdateProgressBar(LevelLoadingProgress progress);
	public void UpdateCustomProgressBar(float progress, ReadOnlySpan<char> desc);
	public void StartCustomProgress();
	public void FinishCustomProgress();
	public void ShowErrorMessage();
	void Simulate();
}


public class EngineVGui(Sys Sys, Net Net) : IEngineVGuiInternal
{
	public static LoadingProgressDescription[] ListenServerLoadingProgressDescriptions = [

		new(LevelLoadingProgress.None,                          0,      0,      null),
		new(LevelLoadingProgress.SpawnServer,                   2,      0,      "#LoadingProgress_SpawningServer"),
		new(LevelLoadingProgress.LoadWorldModel,                4,      7,      "#LoadingProgress_LoadMap"),
		new(LevelLoadingProgress.CreateNetworkStringTables,     23,     0,      null),
		new(LevelLoadingProgress.PrecacheWorld,                 23,     0,      "#LoadingProgress_PrecacheWorld"),
		new(LevelLoadingProgress.ClearWorld,                    23,     0,      null),
		new(LevelLoadingProgress.LevelInit,                     34,     0,      "#LoadingProgress_LoadResources"),
		new(LevelLoadingProgress.Precache,                      35,     239,    null),
		new(LevelLoadingProgress.ActivateServer,                68,     0,      null),
		new(LevelLoadingProgress.SignOnChallenge,               68,     0,      null),
		new(LevelLoadingProgress.SignOnConnect,                 70,     0,      null),
		new(LevelLoadingProgress.SignOnConnected,               73,     0,      "#LoadingProgress_SignonLocal"),
		new(LevelLoadingProgress.ProcessServerInfo,             75,     0,      null),
		new(LevelLoadingProgress.ProcessStringTable,            77,     12,     null),	// 16
		new(LevelLoadingProgress.SignOnNew,                     84,     0,      null),
		new(LevelLoadingProgress.SendClientInfo,                88,     0,      null),
		new(LevelLoadingProgress.SendSignonData,                91,     0,      "#LoadingProgress_SignonDataLocal"),
		new(LevelLoadingProgress.SignOnSpawn,                   94,     0,      null),
		new(LevelLoadingProgress.FullyConnected,                97,     0,      null),
		new(LevelLoadingProgress.ReadyToPlay,                   99,     0,      null),
	];
	public static LoadingProgressDescription[] RemoteConnectLoadingProgressDescriptions = [
		new(LevelLoadingProgress.None,                          0,      0,      null),
		new(LevelLoadingProgress.ChangeLevel,                   1,      0,      "#LoadingProgress_Changelevel"),
		new(LevelLoadingProgress.BeginConnect,                  5,      0,      "#LoadingProgress_BeginConnect"),
		new(LevelLoadingProgress.SignOnChallenge,               10,     0,      "#LoadingProgress_Connecting"),
		new(LevelLoadingProgress.SignOnConnected,               15,     0,      null),
		new(LevelLoadingProgress.ProcessServerInfo,             20,     0,      "#LoadingProgress_ProcessServerInfo"),
		new(LevelLoadingProgress.CreateNetworkStringTables,     25,     11,     null),
		new(LevelLoadingProgress.LoadWorldModel,                45,     7,      "#LoadingProgress_LoadMap"),
		new(LevelLoadingProgress.SignOnNew,                     75,     0,      null),
		new(LevelLoadingProgress.SendClientInfo,                80,     0,      "#LoadingProgress_SendClientInfo"),
		new(LevelLoadingProgress.SendSignonData,                85,     0,      "#LoadingProgress_SignonData"),
		new(LevelLoadingProgress.SignOnSpawn,                   90,     0,      null),
		new(LevelLoadingProgress.FullyConnected,                95,     0,      null),
		new(LevelLoadingProgress.ReadyToPlay,                   99,     0,      null),
	];

	LoadingProgressDescription[]? activeDescriptions = null;

	bool ShowProgressDialog;
	LevelLoadingProgress LastProgressPoint;
	int LastProgressPointRepeatCount;
	double LoadingStartTime;
	struct LoadingProgressEntry
	{
		public double Time;
		public LevelLoadingProgress Progress;
	}
	List<LoadingProgressEntry> LoadingProgress = [];
	bool SaveProgress;
	bool NoShaderAPI;
	double ProgressBias;

	LoadingProgressDescription def = new(LevelLoadingProgress.None, 0, 0, null);
	public ref LoadingProgressDescription GetProgressDescription(LevelLoadingProgress progress) {
		if (activeDescriptions == null) {
			return ref def;
		}

		int i = 0;
		while (i < activeDescriptions.Length) {
			ref LoadingProgressDescription desc = ref activeDescriptions[i];
			if (desc.Progress >= progress)
				return ref desc;

			i++;
		}

		return ref activeDescriptions[0];
	}

	public void OnLevelLoadingStarted() {
		LoadingStartTime = Sys.Time;
		LoadingProgress.Clear();
		LastProgressPoint = LevelLoadingProgress.None;
		LastProgressPointRepeatCount = 0;
		ProgressBias = 0;

		if (Net.IsMultiplayer())
			activeDescriptions = RemoteConnectLoadingProgressDescriptions;
		else
			activeDescriptions = ListenServerLoadingProgressDescriptions;

		ShowProgressDialog = false;
	}
	public void OnLevelLoadingFinished() {
		LastProgressPoint = LevelLoadingProgress.None;
		Sys.ExtendedError = false;
		Sys.DisconnectReason = null;
		Sys.ExtendedDisconnectReason = null;
	}
	public void NotifyOfServerConnect(ReadOnlySpan<char> game, int IP, int connectionPort, int queryPort) { }
	public void NotifyOfServerDisconnect() { }
	public void EnabledProgressBarForNextLoad() { }
	public void UpdateProgressBar(LevelLoadingProgress progress) {
		if (!Sys.InMainThread())
			return;

		if (activeDescriptions == null)
			return;

		if (progress < LastProgressPoint)
			return;

		if (progress == LastProgressPoint)
			++LastProgressPointRepeatCount;
		else
			LastProgressPointRepeatCount = 0;

		ref LoadingProgressDescription desc = ref GetProgressDescription(progress);
		double perc = desc.Percent / 100d;
		if(desc.Repeat > 1 && LastProgressPointRepeatCount > 0) {
			LastProgressPointRepeatCount = Math.Min(LastProgressPointRepeatCount, desc.Repeat);
			double nextPerc = GetProgressDescription(progress + 1).Percent / 100d;
			perc += (nextPerc - perc) * ((float)LastProgressPointRepeatCount / desc.Repeat);
		}

		perc = perc * (1.0f - ProgressBias) + ProgressBias;

		if (Sys.TextMode) {
			Console.Title = ($"Progress: {desc.Description} [{new string('=', Convert.ToInt32(perc * 30)),-30}]\n");
		}
		else {

		}

		LastProgressPoint = progress;
	}
	public void UpdateCustomProgressBar(float progress, ReadOnlySpan<char> desc) { }
	public void StartCustomProgress() { }
	public void FinishCustomProgress() { }
	public void ShowErrorMessage() { }

	public object? GetPanel(VGuiPanelType type) {
		throw new NotImplementedException();
	}
	public bool IsGameUIVisible() {
		throw new NotImplementedException();
	}

	public void Simulate() {
		throw new NotImplementedException();
	}
}