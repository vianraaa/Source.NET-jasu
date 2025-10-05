using Source;
using Source.Common;
using Source.Common.Mathematics;

using System.Numerics;

namespace Game.Client;

public enum LatchFlags {
	LatchAnimationVar = 1 << 0,
	LatchSimulationVar = 1 << 1,
	ExcludeAutoLatch = 1 << 2,
	ExcludeAutoInterpolate = 1 << 3,
	InterpolateLinearOnly = 1 << 4,
	InteroplateOmitUpdateLastNetworked = 1 << 5,
}

public interface IInterpolatedVar {
	public const double EXTRA_INTERPOLATION_HISTORY_STORED = 0.05;

	void Setup(DynamicAccessor accessor, LatchFlags type);
	void SetInterpolationAmount(TimeUnit_t seconds);
	void NoteLastNetworkedValue();
	void NoteChanged(TimeUnit_t changeTime, bool updateLastNetworkedValue);
	void Reset();
	void Interpolate(TimeUnit_t seconds);
	LatchFlags GetVarType();
	void RestoreToLastNetworked();
	void Copy(IInterpolatedVar src);
	ReadOnlySpan<char> GetDebugName();
	void SetDebugName(ReadOnlySpan<char> name);
}

public class InterpolatedVarArrayBase<T>(bool isArray) : IInterpolatedVar
{
	public void Copy(IInterpolatedVar src) {
		throw new NotImplementedException();
	}

	public ReadOnlySpan<char> GetDebugName() {
		throw new NotImplementedException();
	}

	public LatchFlags GetVarType() {
		throw new NotImplementedException();
	}

	public void Interpolate(double seconds) {
		throw new NotImplementedException();
	}

	public void NoteChanged(double changeTime, bool updateLastNetworkedValue) {
		throw new NotImplementedException();
	}

	public void NoteLastNetworkedValue() {
		throw new NotImplementedException();
	}

	public void Reset() {
		throw new NotImplementedException();
	}

	public void RestoreToLastNetworked() {
		throw new NotImplementedException();
	}

	public void SetDebugName(ReadOnlySpan<char> name) {
		throw new NotImplementedException();
	}

	public void SetInterpolationAmount(double seconds) {
		throw new NotImplementedException();
	}

	public void Setup(DynamicAccessor accessor, LatchFlags type) {
		throw new NotImplementedException();
	}
}

public class InterpolatedVar<T>() : InterpolatedVarArrayBase<T>(false)
{

}


[EngineComponent]
public class Interpolation
{
	public static TimeUnit_t LastPacketTimestamp;
	internal void SetLastPacketTimeStamp(TimeUnit_t timestamp) {
		LastPacketTimestamp = timestamp;
	}

	public static Vector3 ExtrapolateInterpolatedVarType(in Vector3 oldVal, in Vector3 newVal, double divisor, double extrapolationAmount)
		=> Vector3.Lerp(oldVal, newVal, (float)(1.0 + extrapolationAmount * divisor));
	public static QAngle ExtrapolateInterpolatedVarType(in QAngle oldVal, in QAngle newVal, double divisor, double extrapolationAmount)
		=> QAngle.Lerp(oldVal, newVal, (float)(1.0 + extrapolationAmount * divisor));
	public static float ExtrapolateInterpolatedVarType(float oldVal, float newVal, double divisor, double extrapolationAmount)
		=> float.Lerp(oldVal, newVal, (float)(1.0 + extrapolationAmount * divisor));
	public static double ExtrapolateInterpolatedVarType(double oldVal, double newVal, double divisor, double extrapolationAmount)
		=> double.Lerp(oldVal, newVal, 1.0 + extrapolationAmount * divisor);
}