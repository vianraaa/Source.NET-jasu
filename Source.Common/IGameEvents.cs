using Source.Common.Formats.Keyvalues;

namespace Source.Common;

public interface IGameEvent
{
	ReadOnlySpan<char> GetName(); 
	bool IsReliable();
	bool IsLocal();
	bool IsEmpty(ReadOnlySpan<char> keyName = default);
	bool GetBool(ReadOnlySpan<char> keyName = default, bool defaultValue = false);
	int GetInt(ReadOnlySpan<char> keyName = default, int defaultValue = 0);
	float GetFloat(ReadOnlySpan<char> keyName = default, float defaultValue = 0.0f);
	ReadOnlySpan<char> GetString(ReadOnlySpan<char> keyName = default, ReadOnlySpan<char> defaultValue = default);
	void SetBool(ReadOnlySpan<char> keyName, bool value);
	void SetInt(ReadOnlySpan<char> keyName, int value);
	void SetFloat(ReadOnlySpan<char> keyName, float value);
	void SetString(ReadOnlySpan<char> keyName, ReadOnlySpan<char> value);
}

public interface IGameEventListener2
{
	void FireGameEvent(IGameEvent ev);
}

public interface IGameEventManager2
{

}

public interface IGameEventListener
{
	void FireGameEvent(KeyValues ev);
}