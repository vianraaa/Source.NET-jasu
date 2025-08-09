namespace Source.Common.Commands;

public sealed class DefaultCvarQuery : ICvarQuery {
	public bool AreConVarsLinkable(ConVar child, ConVar parent) {
		return true;
	}
}