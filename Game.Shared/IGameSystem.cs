namespace Game.Shared;


public interface IGameSystem
{
	ReadOnlySpan<char> Name();

	bool Init();
	void PostInit();
	void Shutdown();

	void LevelInitPreEntity();
	void LevelInitPostEntity();

	void LevelShutdownPreClearSteamAPIContext();
	void LevelShutdownPreEntity();
	void LevelShutdownPostEntity();
	void OnSave();
	void OnRestore();
	void SafeRemoveIfDesired();

	bool IsPerFrame();

#if CLIENT_DLL
	void PreRender();
	void Update(double frametime);
	void PostRender();
#else
	void FrameUpdatePreEntityThink();
	void FrameUpdatePostEntityThink();
	void PreClientUpdate();
#endif

	static string? currentMapName;

	static ReadOnlySpan<char> MapName() => currentMapName;

	static readonly List<IGameSystem> SystemList = [];

	static void Add(IGameSystem sys) {
		SystemList.Add(sys);
	}
	static void Remove(IGameSystem sys) {
		SystemList.Remove(sys);
	}
	static void RemoveAll() {
		SystemList.Clear();
	}

	static bool InitAllSystems() {
		foreach (var sys in SystemList) {
			if (!sys.Init())
				return false;
		}
		return true;
	}

	static void PostInitAllSystems() {
		foreach (var sys in SystemList)
			sys.PostInit();
	}

	static void ShutdownAllSystems() {
		foreach (var sys in ((IEnumerable<IGameSystem>)SystemList).Reverse())
			sys.PostInit();
	}

	static void LevelInitPreEntityAllSystems(ReadOnlySpan<char> mapName) {
		currentMapName = new(mapName);
		foreach (var sys in SystemList)
			sys.LevelInitPreEntity();
	}

	static void LevelInitPostEntityAllSystems() {
		foreach (var sys in SystemList)
			sys.LevelInitPostEntity();
	}

	static void LevelShutdownPreClearSteamAPIContextAllSystems() {
		foreach (var sys in ((IEnumerable<IGameSystem>)SystemList).Reverse())
			sys.LevelShutdownPreClearSteamAPIContext();
	}

	static void LevelShutdownPreEntityAllSystems() {
		foreach (var sys in ((IEnumerable<IGameSystem>)SystemList).Reverse())
			sys.LevelShutdownPreEntity();
	}

	static void LevelShutdownPostEntityAllSystems() {
		foreach (var sys in ((IEnumerable<IGameSystem>)SystemList).Reverse())
			sys.LevelShutdownPostEntity();
	}

	static void OnSaveAllSystems() {
		foreach (var sys in ((IEnumerable<IGameSystem>)SystemList).Reverse())
			sys.OnSave();
	}

	static void OnRestoreAllSystems() {
		foreach (var sys in ((IEnumerable<IGameSystem>)SystemList).Reverse())
			sys.OnRestore();
	}

	static void SafeRemoveIfDesiredAllSystems() {
		foreach (var sys in ((IEnumerable<IGameSystem>)SystemList).Reverse())
			sys.SafeRemoveIfDesired();
	}

#if CLIENT_DLL
	static void PreRenderAllSystems() {
		// todo
	}
	static void UpdateAllSystems(double frametime) {
		// todo
	}
	static void PostRenderAllSystems() {
		// todo
	}
#else
	static void FrameUpdatePreEntityThinkAllSystems() { 
		// todo
	}
	static void FrameUpdatePostEntityThinkAllSystems() { 
		// todo
	}
	static void PreClientUpdateAllSystems() {
		// todo
	}
#endif
};