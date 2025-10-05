using Source;
using Source.Common;
using Source.Common.Commands;
using Source.Common.Hashing;
using Source.Common.Mathematics;

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Game.Client;

public enum LatchFlags {
	LatchAnimationVar = 1 << 0,
	LatchSimulationVar = 1 << 1,
	ExcludeAutoLatch = 1 << 2,
	ExcludeAutoInterpolate = 1 << 3,
	InterpolateLinearOnly = 1 << 4,
	InteroplateOmitUpdateLastNetworked = 1 << 5,
}

public class InterpolationContext
{
	static bool AllowExtrapolation;
	static TimeUnit_t LastTimeStamp;
	internal static void EnableExtrapolation(bool value) => AllowExtrapolation = value;
	internal static bool IsExtrapolationAllowed() => AllowExtrapolation;
	internal static void SetLastTimeStamp(TimeUnit_t value) => LastTimeStamp = value;
	internal static TimeUnit_t GetLastTimeStamp() => LastTimeStamp;
}

public interface IInterpolatedVar {
	public const double EXTRA_INTERPOLATION_HISTORY_STORED = 0.05;

	void Setup(object instance, DynamicAccessor accessor, LatchFlags type);
	void SetInterpolationAmount(TimeUnit_t seconds);
	void NoteLastNetworkedValue();
	void NoteChanged(TimeUnit_t changeTime, TimeUnit_t interpolationAmount, bool updateLastNetworkedValue);
	void Reset();
	int Interpolate(TimeUnit_t seconds, TimeUnit_t amount);
	LatchFlags GetVarType();
	void RestoreToLastNetworked();
	void Copy(IInterpolatedVar src);
	ReadOnlySpan<char> GetDebugName();
	void SetDebugName(ReadOnlySpan<char> name);
}

public class SimpleRingBuffer<T> where T : new()
{
	private T[]? elements;
	private ushort maxElement;
	private ushort firstElement;
	private ushort count;
	private ushort growSize;

	public SimpleRingBuffer(int startSize = 4) {
		elements = null;
		maxElement = 0;
		firstElement = 0;
		count = 0;
		growSize = 16;
		EnsureCapacity(startSize);
	}

	public int Count() => count;

	public int Head() => (count > 0) ? 0 : InvalidIndex();

	public bool IsIdxValid(int i) => (i >= 0 && i < count);

	public bool IsValidIndex(int i) => IsIdxValid(i);

	public static int InvalidIndex() => -1;

	public ref T this[int i] {
		get {
			Assert(IsIdxValid(i));
			i += firstElement;
			i = WrapRange(i);
			return ref elements![i];
		}
	}

	public void EnsureCapacity(int capSize) {
		if (capSize > maxElement) {
			int newMax = maxElement + ((capSize + growSize - 1) / growSize) * growSize;
			T[] pNew = new T[newMax];

			for (int i = 0; i < count; i++) {
				pNew[i] = elements![WrapRange(i + firstElement)];
			}

			firstElement = 0;
			maxElement = (ushort)newMax;
			elements = pNew;
		}
	}

	public int AddToHead() {
		EnsureCapacity(count + 1);
		int i = firstElement + maxElement - 1;
		count++;
		i = WrapRange(i);
		firstElement = (ushort)i;
		elements![firstElement] = new T();
		return 0;
	}

	public int AddToHead(T elem) {
		AddToHead();
		elements![firstElement] = elem;
		return 0;
	}

	public int AddToTail() {
		EnsureCapacity(count + 1);
		count++;
		int index = WrapRange(firstElement + count - 1);
		elements![index] = new T();
		return index;
	}

	public void RemoveAll() {
		count = 0;
		firstElement = 0;
	}

	public void RemoveAtHead() {
		if (count > 0) {
			firstElement = (ushort)WrapRange(firstElement + 1);
			count--;
		}
	}

	public void Truncate(int newLength) {
		if (newLength < count) {
			System.Diagnostics.Debug.Assert(newLength >= 0);
			count = (ushort)newLength;
		}
	}

	private int WrapRange(int i) {
		return (i >= maxElement) ? (i - maxElement) : i;
	}
}

public struct InterpolatedVarEntryBase<T>
{
	public readonly bool IsArray;

	public TimeUnit_t ChangeTime;
	public T[]? Value;

	public InterpolatedVarEntryBase(bool isArray) {
		IsArray = isArray;
	}

	public T[]? GetValue() => Value;
	public void Init(int maxCount) {
		if(maxCount == 0) {
			DeleteEntry();
		}
		else {
			if (maxCount != Value?.Length)
				DeleteEntry();

			if(Value == null) {
				Value = new T[maxCount];
			}
		}
	}

	public Span<T> NewEntry(object instance, DynamicAccessor accessor, int maxCount, TimeUnit_t time) {
		ChangeTime = time;
		Init(accessor.Length);
		if (Value != null && maxCount != 0)
			accessor.CopyTo(instance, Value.AsSpan());
		return Value;
	}

	public void DeleteEntry() {
		Value = null;
	}

	internal void FastTransferFrom<T>(InterpolatedVarEntryBase<T> interpolatedVarEntryBase) {
		throw new NotImplementedException();
	}
}

public class InterpolatedVarArrayBase<T>(bool isArray) : IInterpolatedVar
{
	SimpleRingBuffer<InterpolatedVarEntryBase<T>> VarHistory = new();
	public void Copy(IInterpolatedVar src) {
		throw new NotImplementedException();
	}

	public virtual ReadOnlySpan<char> GetDebugName() {
		throw new NotImplementedException();
	}

	public LatchFlags GetVarType() => mType;

	public struct InterpolationInfo
	{
		public bool Hermite;
		public int Oldest;
		public int Older;
		public int Newer;
		public double Fraction;
	}


	public int Interpolate(TimeUnit_t currentTime, TimeUnit_t interpolationAmount) {
		int noMoreChanges = 0;
		if (GetInterpolationInfo(out InterpolationInfo info, currentTime, interpolationAmount, out noMoreChanges))
			return noMoreChanges;

		var history = VarHistory;

		if (info.Hermite)
			_Interpolate_Hermite(Instance, Accessor, info.Fraction, ref history[info.Oldest], ref history[info.Older], ref history[info.Newer]);
		else if (info.Newer == info.Older) {
			int realOlder = info.Newer + 1;
			if (InterpolationContext.IsExtrapolationAllowed() && IsValidIndex(realOlder) && history[realOlder].ChangeTime != 0.0 && interpolationAmount > 0.000001 && InterpolationContext.GetLastTimeStamp() <= LastNetworkedTime) 
				_Extrapolate(Instance, Accessor, ref history[realOlder], ref history[info.Newer], currentTime - interpolationAmount, Interpolation.cl_extrapolate_amount.GetFloat());
			else 
				_Interpolate(Instance, Accessor, info.Fraction, ref history[info.Older], ref history[info.Newer]);
		}
		else {
			_Interpolate(Instance, Accessor, info.Fraction, ref history[info.Older], ref history[info.Newer]);
		}

		RemoveEntriesPreviousTo(currentTime - interpolationAmount - IInterpolatedVar.EXTRA_INTERPOLATION_HISTORY_STORED);
		return noMoreChanges;
	}

	private void _Interpolate(object instance, DynamicAccessor accessor, double fraction, ref InterpolatedVarEntryBase<T> start, ref InterpolatedVarEntryBase<T> end) {
		if (Unsafe.AreSame(ref start, ref end)) {
			accessor.CopyFrom(instance, end.GetValue()!.AsSpan()[..MaxCount]);
			return;
		}

		Assert(fraction >= 0.0 && fraction <= 1.0);
		// todo
		throw new NotImplementedException();

	}

	private void _Extrapolate(object instance, DynamicAccessor accessor, ref InterpolatedVarEntryBase<T> interpolatedVarEntryBase1, ref InterpolatedVarEntryBase<T> interpolatedVarEntryBase2, double v1, float v2) {
		throw new NotImplementedException();
	}

	private void _Interpolate_Hermite(object instance, DynamicAccessor accessor, double fraction, ref InterpolatedVarEntryBase<T> interpolatedVarEntryBase1, ref InterpolatedVarEntryBase<T> interpolatedVarEntryBase2, ref InterpolatedVarEntryBase<T> interpolatedVarEntryBase3) {
		throw new NotImplementedException();
	}

	private bool IsValidIndex(int i) => VarHistory.IsValidIndex(i); 

	private bool GetInterpolationInfo(out InterpolationInfo info, double currentTime, double interpolationAmount, out int noMoreChanges) {
		throw new NotImplementedException();
	}

	public void NoteChanged(TimeUnit_t changeTime, TimeUnit_t interpolationAmount, bool updateLastNetworkedValue) {
		bool ret = true;

		AddToHead(changeTime, Accessor, true);

		if (updateLastNetworkedValue) {
			NoteLastNetworkedValue();
		}
		RemoveEntriesPreviousTo(gpGlobals.CurTime - interpolationAmount - IInterpolatedVar.EXTRA_INTERPOLATION_HISTORY_STORED);
	}

	private void RemoveEntriesPreviousTo(double v) {
		throw new NotImplementedException();
	}

	private void AddToHead(double changeTime, DynamicAccessor values, bool flushNewer) {
		int newslot;

		if (flushNewer) {
			while (VarHistory.Count() != 0) {
				if ((VarHistory[0].ChangeTime + 0.0001) > changeTime) 
					VarHistory.RemoveAtHead();
				else 
					break;
			}

			newslot = VarHistory.AddToHead();
		}
		else {
			newslot = VarHistory.AddToHead();
			for (int i = 1; i < VarHistory.Count(); i++) {
				if (VarHistory[i].ChangeTime <= changeTime)
					break;
				VarHistory[newslot].FastTransferFrom(VarHistory[i]);
				newslot = i;
			}
		}

		ref InterpolatedVarEntryBase<T> e = ref VarHistory[newslot];
		e.NewEntry(Instance, values, MaxCount, changeTime);
	}

	public void NoteLastNetworkedValue() {
		LastNetworkedValue![0] = Accessor.GetValue<T>(Instance);
		LastNetworkedTime = Interpolation.LastPacketTimestamp;
	}

	public void Reset() {
		ClearHistory();

		if (Accessor != null) {
			AddToHead(gpGlobals.CurTime, Accessor, false);
			AddToHead(gpGlobals.CurTime, Accessor, false);
			AddToHead(gpGlobals.CurTime, Accessor, false);

			Accessor.CopyTo(Instance, LastNetworkedValue.AsSpan());
		}
	}

	public void SetMaxCount(int newmax) {
		bool changed = newmax != MaxCount ? true : false;
		newmax = Math.Max(1, newmax);
		if (changed) {
			Looping = null;
			LastNetworkedValue = null;
			Looping = new bool[newmax];
			LastNetworkedValue = new T[newmax];
			Reset();
		}
	}

	private void ClearHistory() {
		for (int i = 0; i < VarHistory.Count(); i++) 
			VarHistory[i].DeleteEntry();
		
		VarHistory.RemoveAll();
	}

	public void RestoreToLastNetworked() {
		throw new NotImplementedException();
	}

	public void SetDebugName(ReadOnlySpan<char> name) {
		throw new NotImplementedException();
	}

	public void SetInterpolationAmount(TimeUnit_t seconds) {
		InterpolationAmount = seconds;
	}

	public void Setup(object instance, DynamicAccessor accessor, LatchFlags type) {
		this.Instance = instance;
		this.Accessor = accessor;
		this.mType = type;
	}
	object Instance;
	DynamicAccessor Accessor;
	LatchFlags mType;
	TimeUnit_t InterpolationAmount;
	TimeUnit_t LastNetworkedTime;
	T[]? LastNetworkedValue;
	int Type;
	int MaxCount => LastNetworkedValue?.Length ?? 0;
	bool[]? Looping;
}

public class InterpolatedVar<T>(string name) : InterpolatedVarArrayBase<T>(false)
{
	public override ReadOnlySpan<char> GetDebugName() {
		return name;
	}
}


[EngineComponent]
public class Interpolation
{
	internal static ConVar cl_extrapolate_amount = new("0.25", FCvar.Cheat, "Set how many seconds the client will extrapolate entities for.");

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