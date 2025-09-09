using Source.Common.Input;
using Source.GUI.Controls;

namespace Game.Client;

public enum MessageModeType {
	None,
	Say,
	SayTeam
}

public interface IClientMode
{
	void Init();
	int KeyInput(int down, ButtonCode keynum, ReadOnlySpan<char> currentBinding);
	void StartMessageMode(MessageModeType messageModeType);
}
