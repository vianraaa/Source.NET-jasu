namespace Source.Common;

public interface IEngine {
	public enum Quit {
		NotQuitting,
		ToDesktop,
		Restart
	}
	public enum State {
		Inactive,
		Active,
		Close,
		Restart,
		Paused
	}

	public bool Load(bool dedicated, string rootDirectory);
	public void Unload();
	public void SetNextState(State nextState);
	public State GetState();
	public void Frame();
	public double GetFrameTime();
	public double GetCurTime();
	public Quit GetQuitting();
	public void SetQuitting(Quit quitType);
}
