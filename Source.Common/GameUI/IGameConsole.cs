using Source.Common.GUI;

namespace Source.Common.GameUI;

public interface IGameConsole {
	void Activate();
	void Initialize();
	void Hide();
	void Clear();
	bool IsConsoleVisible();
	void SetParent(IPanel? parent);
}
