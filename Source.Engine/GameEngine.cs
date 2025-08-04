using Source.Common;

namespace Source.Engine;

public class GameEngine : IEngine
{
	readonly Sys Sys;
	bool FilterTime(float t) {
		return false;
	}

	IEngine.Quit quitting;

	IEngine.State state;
	IEngine.State nextState;

	double currentTime;
	double frameTime;
	double previousTime;
	double filteredTime;
	double minFrameTime;
	double lastRemainder;
	bool catchupTime;

	public GameEngine(Sys Sys) {
		this.Sys = Sys;

		state = IEngine.State.Inactive;
		nextState = IEngine.State.Inactive;
		currentTime = 0;
		frameTime = 0;
		previousTime = 0;
		filteredTime = 0;
		minFrameTime = 0;
		lastRemainder = 0;
		catchupTime = false;
		quitting = IEngine.Quit.NotQuitting;
	}

	public bool Load(bool dedicated, string rootDirectory) {
		bool success = false;

		state = nextState = IEngine.State.Active;
		if(Sys.InitGame(dedicated, rootDirectory)) {
			success = true;
		}

		return success;
	}

	public void Unload() {
		Sys.ShutdownGame();
		state = IEngine.State.Inactive;
		nextState = IEngine.State.Inactive;
	}

	public void SetNextState(IEngine.State nextState) {
		throw new NotImplementedException();
	}

	public IEngine.State GetState() {
		throw new NotImplementedException();
	}

	public void Frame() {
		throw new NotImplementedException();
	}

	public double GetFrameTime() {
		throw new NotImplementedException();
	}

	public double GetCurTime() {
		throw new NotImplementedException();
	}

	public IEngine.Quit GetQuitting() {
		throw new NotImplementedException();
	}

	public void SetQuitting(IEngine.Quit quitType) {
		throw new NotImplementedException();
	}
}
