using Source.Common.Formats.Keyvalues;

namespace Source.Common.GUI;

public interface IVGui
{
	void Quit();
	void RunFrame();
	bool DispatchMessages();
	void PostMessage(IPanel? to, KeyValues message, IPanel? from, double delay = 0);
	void Stop();
	IVGuiInput GetInput();
}