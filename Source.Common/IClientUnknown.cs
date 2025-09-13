namespace Source.Common;

public interface IClientUnknown {
	IClientNetworkable GetClientNetworkable();
	IClientRenderable GetClientRenderable();
	IClientEntity GetIClientEntity();
	IClientThinkable GetClientThinkable();
}
