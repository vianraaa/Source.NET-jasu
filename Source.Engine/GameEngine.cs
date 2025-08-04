using Source.Common;
using Source.Engine.Server;

using static Source.Dbg;

namespace Source.Engine;

public class GameEngine : IEngine
{
	readonly Sys Sys;
	readonly GameServer sv;
	readonly IHostState HostState;
	bool FilterTime(double t) {
		return false;
	}

	IEngine.Quit Quitting;

	IEngine.State State;
	IEngine.State NextState;

	double CurrentTime;
	double FrameTime;
	double PreviousTime;
	double FilteredTime;
	double MinFrameTime;
	double LastRemainder;
	bool CatchupTime;

	public GameEngine(Sys Sys, IHostState HostState, GameServer sv) {
		this.Sys = Sys;
		this.HostState = HostState;
		this.sv = sv;

		State = IEngine.State.Inactive;
		NextState = IEngine.State.Inactive;
		CurrentTime = 0;
		FrameTime = 0;
		PreviousTime = 0;
		FilteredTime = 0;
		MinFrameTime = 0;
		LastRemainder = 0;
		CatchupTime = false;
		Quitting = IEngine.Quit.NotQuitting;
	}

	public bool Load(bool dedicated, string rootDirectory) {
		bool success = false;

		State = NextState = IEngine.State.Active;
		if(Sys.InitGame(dedicated, rootDirectory)) {
			success = true;
		}

		return success;
	}

	public void Unload() {
		Sys.ShutdownGame();
		State = IEngine.State.Inactive;
		NextState = IEngine.State.Inactive;
	}

	public void SetNextState(IEngine.State nextState) {
		throw new NotImplementedException();
	}

	public IEngine.State GetState() {
		throw new NotImplementedException();
	}

	public void Frame() {
		if (PreviousTime == 0) {
			FilterTime(0.0);
			PreviousTime = Sys.Time - MinFrameTime;
		}

		for (; ; ) {
			CurrentTime = Sys.Time;
			FrameTime = CurrentTime - PreviousTime;
			Assert(FrameTime >= 0);
			// TODO: handle ^^^

			if (FilterTime(FrameTime))
				break;

			double busyWaitMS = 2.25; // windows exclusive change later?

			int sleepMS = (int)((MinFrameTime - FrameTime) * 1000 - busyWaitMS);
			if (sleepMS > 0)
				Thread.Sleep(sleepMS);
			else {
				for (int i = 2000; i >= 0; i--) ;
			}
		}
		FilteredTime = 0;
		if (!sv.IsDedicated()) { }

		switch (State) {
			case IEngine.State.Paused:
			case IEngine.State.Inactive:
				break;
			case IEngine.State.Active:
			case IEngine.State.Close:
			case IEngine.State.Restart:
				HostState.Frame(FrameTime);
				break;
		}

		if (NextState != State) {
			State = NextState;
			switch (State) {
				case IEngine.State.Close: SetQuitting(IEngine.Quit.ToDesktop); break;
				case IEngine.State.Restart: SetQuitting(IEngine.Quit.Restart); break;
			}
		}

		PreviousTime = CurrentTime;
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
		Quitting = quitType;
	}
}
