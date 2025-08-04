namespace Source.Common;

/// <summary>
/// Interface exposed from the client DLL back to the engine
/// </summary>
public interface IBaseClientDLL {
	void IN_SetSampleTime(double frameTime);
	void PostInit();
}
