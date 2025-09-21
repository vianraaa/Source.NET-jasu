using SharpCompress.Common;

using System.Reflection;
using System.Runtime.CompilerServices;

namespace Source.Common;

public class GModTable {
	/// <summary>
	/// Never used but can freely be edited.
	/// </summary>
	public static DVariant Empty;

	// This sucks. But I need this function, and its internal to a Dictionary<TKey, TValue>...
	delegate ref DVariant GetRefDVariant(int key);
	readonly Dictionary<int, DVariant> Values;
	readonly GetRefDVariant FindValue;

	public GModTable() {
		Values = [];
		FindValue = typeof(Dictionary<int, DVariant>).GetMethod("FindValue", (BindingFlags)~0)!.CreateDelegate<GetRefDVariant>(Values);
	}

	public const int ENTRIES_BITS = 12;
	public const int MAX_ENTRIES= 1 << ENTRIES_BITS;

	public const int ENTRY_KEY_BITS = 12;
	public const int MAX_ENTRY_KEYS = 1 << ENTRY_KEY_BITS;

	public const int ENTRY_VALUE_TYPE_BITS = 3;
	public const int MAX_VALUE_TYPE = 1 << ENTRY_VALUE_TYPE_BITS;

	public static readonly string WARNING_UNDERFLOW = "Attempted to index below 0 on a GModTable!\n";
	public static readonly string WARNING_OVERFLOW = "Attempted to index above " + (MAX_ENTRY_KEYS - 1) + " on a GModTable!\n";

	public ref DVariant this[int index] {
		get {
			if(index < 0) {
				DevWarning(WARNING_UNDERFLOW);
				return ref Empty;
			}

			if(index >= MAX_ENTRY_KEYS) {
				DevWarning(WARNING_OVERFLOW);
				return ref Empty;
			}

			ref DVariant valRef = ref FindValue(index);
			if (!Unsafe.IsNullRef(ref valRef)) 
				return ref valRef;
			else {
				Values[index] = new();
				return ref FindValue(index);
			}
		}
	}
	public void Clear() {
		Values.Clear();
	}
}