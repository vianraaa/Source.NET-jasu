
using SharpCompress.Common;

namespace Source.Common;

public class BaseHandle {
	public uint Index;

	public BaseHandle() => Index = (uint)Constants.INVALID_EHANDLE_INDEX;
	public BaseHandle(BaseHandle handle) => Index = handle.Index;
	public BaseHandle(int entry, int serial) => Init(entry, serial);

	public void Init(int entry, int serial) => Index = (uint)(entry | (serial << Constants.NUM_ENT_ENTRY_BITS));

	public int GetEntryIndex() => (int)(Index & Constants.ENT_ENTRY_MASK);
	public int GetSerialNumber() => (int)(Index >> Constants.NUM_ENT_ENTRY_BITS);

	public static bool operator ==(BaseHandle? a, BaseHandle? b) => (a?.Index ?? 0) == (b?.Index ?? 0);
	public static bool operator !=(BaseHandle? a, BaseHandle? b) => (a?.Index ?? 0) != (b?.Index ?? 0);
	public static bool operator <(BaseHandle? a, BaseHandle? b) => (a?.Index ?? 0) < (b?.Index ?? 0);
	public static bool operator >(BaseHandle? a, BaseHandle? b) => (a?.Index ?? 0) > (b?.Index ?? 0);
	public override bool Equals(object? obj) {
		return obj switch {
			BaseHandle b => Index == b.Index,
			_ => false
		};
	}
	public bool IsValid() => Index != Constants.INVALID_EHANDLE_INDEX;
}