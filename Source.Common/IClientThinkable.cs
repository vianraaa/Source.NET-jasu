namespace Source.Common;

public interface IClientThinkable {
	IClientUnknown GetIClientUnknown();
	void ClientThink();
	void Release();
}
