namespace Game.Client;

internal static class CdllBoundedCVars
{
	public static double GetClientInterpAmount() {
		return 0.1; // MASSIVE TODO: REQUIRES ConVar_ServerBounded and a lot of other annoying things!!!!
		// For now, just hardcoding it at 0.1
	}
}
