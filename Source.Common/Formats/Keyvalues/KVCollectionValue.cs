using Source.Common.Formats.Keyvalues;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Source.Common.Formats.Keyvalues
{
	public class KVCollectionValue : KVValue, IEnumerable<KeyValues>, IList<KeyValues>
	{
		public KVCollectionValue() {
			children = new List<KeyValues>();
		}

		readonly List<KeyValues> children;

		public override KVValueType ValueType => KVValueType.Collection;

		public int Count => ((ICollection<KeyValues>)children).Count;

		public bool IsReadOnly => ((ICollection<KeyValues>)children).IsReadOnly;

		public KeyValues this[int index] { get => ((IList<KeyValues>)children)[index]; set => ((IList<KeyValues>)children)[index] = value; }

		public override KVValue this[string key] {
			get {
				Require.NotNull(key, nameof(key));
				return Get(key)?.Value;
			}
		}

		public void Add(KeyValues value) {
			Require.NotNull(value, nameof(value));
			children.Add(value);
		}

		public void AddRange(IEnumerable<KeyValues> values) {
			Require.NotNull(values, nameof(values));
			children.AddRange(values);
		}

		public KeyValues Get(string name) {
			Require.NotNull(name, nameof(name));
			return children.FirstOrDefault(c => c.Name == name);
		}

		public void Set(string name, KVValue value) {
			Require.NotNull(name, nameof(name));
			Require.NotNull(value, nameof(value));

			children.RemoveAll(kv => kv.Name == name);
			children.Add(new KeyValues(name, value));
		}

		#region IEnumerable<KVObject>

		public IEnumerator<KeyValues> GetEnumerator() => children.GetEnumerator();

		#endregion

		#region IConvertible

		public override TypeCode GetTypeCode() {
			throw new NotSupportedException();
		}

		public override bool ToBoolean(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		public override byte ToByte(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		public override char ToChar(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		public override DateTime ToDateTime(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		public override decimal ToDecimal(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		public override double ToDouble(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		public override short ToInt16(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		public override int ToInt32(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		public override long ToInt64(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		public override sbyte ToSByte(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		public override float ToSingle(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		public override string ToString(IFormatProvider provider)
			 => ToString();

		public override object ToType(Type conversionType, IFormatProvider provider) {
			throw new NotSupportedException();
		}

		public override ushort ToUInt16(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		public override uint ToUInt32(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		public override ulong ToUInt64(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		#endregion

		#region IEnumerable

		IEnumerator IEnumerable.GetEnumerator() => children.GetEnumerator();

		#endregion

		public override string ToString() => "[Collection]";

		public int IndexOf(KeyValues item) {
			return ((IList<KeyValues>)children).IndexOf(item);
		}

		public void Insert(int index, KeyValues item) {
			((IList<KeyValues>)children).Insert(index, item);
		}

		public void RemoveAt(int index) {
			((IList<KeyValues>)children).RemoveAt(index);
		}

		public void Clear() {
			((ICollection<KeyValues>)children).Clear();
		}

		public bool Contains(KeyValues item) {
			return ((ICollection<KeyValues>)children).Contains(item);
		}

		public void CopyTo(KeyValues[] array, int arrayIndex) {
			((ICollection<KeyValues>)children).CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValues item) {
			return ((ICollection<KeyValues>)children).Remove(item);
		}
	}
}
