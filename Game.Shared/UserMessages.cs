namespace Game.Shared;

using Source.Common.Bitbuffers;

using Source;

public delegate void UserMessageHook(bf_read msg);

public class UserMessage
{
	public int Size = 0;
	public string Name = "";
	public List<UserMessageHook> ClientHooks = [];
}

[EngineComponent]
public partial class UserMessages
{
	private Dictionary<string, int> nameToIntType = [];
	private Dictionary<int, UserMessage> intTypeToInstance = [];


	public int Count {
		get {
			int refCount = nameToIntType.Count;
			Dbg.Assert(refCount == intTypeToInstance.Count);
			return refCount;
		}
	}

	public int LookupUserMessage(string name) {
		if (nameToIntType.TryGetValue(name, out int idx))
			return idx;
		return -1;
	}

	public int GetUserMessageSize(int index) {
		if (index < 0 || index >= Count) {
			throw new Exception($"UserMessages.GetUserMessageSize({index}) out of range!!!");
		}

		UserMessage e = intTypeToInstance[index];
		return e.Size;
	}

	public string GetUserMessageName(int index) {
		if (index < 0 || index >= Count) {
			throw new Exception($"UserMessages.GetUserMessageSize({index}) out of range!!!");
		}

		UserMessage e = intTypeToInstance[index];
		return e.Name;
	}

	public bool IsValidIndex(int index) {
		return intTypeToInstance.ContainsKey(index);
	}

	public void Register(string name, int size) {
		if (nameToIntType.TryGetValue(name, out int idx))
			throw new Exception($"UserMessages.Register '{name}' already registered");

		UserMessage entry = new UserMessage();
		entry.Size = size;
		entry.Name = name;
		idx = Count;

		intTypeToInstance[idx] = entry;
		nameToIntType[name] = idx;
	}

	public void HookMessage(string name, UserMessageHook hook) {
		Dbg.Assert(name != null);
		Dbg.Assert(hook != null);

		if (!nameToIntType.TryGetValue(name, out int idx)) {
			Dbg.Assert(false, $"UserMessages.HookMessage: no such message {name}");
			return;
		}

		UserMessage msg = intTypeToInstance[idx];
		msg.ClientHooks.Add(hook);
	}

	public bool DispatchUserMessage(int msgType, bf_read msgData) {
		if (msgType < 0 || msgType >= Count) {
			Dbg.Assert(false, $"UserMessages.DispatchUserMessage: Bogus msg type {msgType} (max == {Count})");
			return false;
		}

		if (!intTypeToInstance.TryGetValue(msgType, out UserMessage? entry)) {
			Dbg.Assert(false, $"UserMessages.DispatchUserMessage: Missing client entry for msg type {msgType}");
			return false;
		}

		if (entry.ClientHooks.Count == 0) {
			Dbg.Warning($"UserMessages.DispatchUserMessage: missing client hook for {entry.Name}\n");
			return false;
		}

		foreach (UserMessageHook hook in entry.ClientHooks) {
			bf_read msgCopy = msgData.Copy();
			hook(msgCopy);
		}

		return true;
	}
}