
using Source.Common.Commands;

namespace Source.Engine;

public class Render (
	CommonHostState host_state
	
	)
{
	int framecount = 1;

	internal void FrameBegin() {

		framecount++;
	}

	internal void FrameEnd() {

	}
}
