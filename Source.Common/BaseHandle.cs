namespace Source.Common;

public class BaseHandle {
	public uint Index;

	public int GetEntryIndex() => (int)(Index & Constants.ENT_ENTRY_MASK);
	public int GetSerialNumber() => (int)(Index >> Constants.NUM_ENT_ENTRY_BITS);

	public static bool operator ==(BaseHandle a, BaseHandle b) => a.Index == b.Index;
	public static bool operator !=(BaseHandle a, BaseHandle b) => a.Index != b.Index;
	public static bool operator <(BaseHandle a, BaseHandle b) => a.Index < b.Index;
	public static bool operator >(BaseHandle a, BaseHandle b) => a.Index > b.Index;
	public override bool Equals(object? obj) {
		return obj switch {
			BaseHandle b => Index == b.Index,
			_ => false
		};
	}
}