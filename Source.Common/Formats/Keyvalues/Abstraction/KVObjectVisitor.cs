using Source.Common.Formats.Keyvalues;
using Source.Common.Formats.Keyvalues;

using System;
using System.Collections.Generic;

namespace Source.Common.Formats.Keyvalues.Abstraction
{
	sealed class KVObjectVisitor
	{
		public KVObjectVisitor(IVisitationListener listener) {
			Require.NotNull(listener, nameof(listener));

			this.listener = listener;
		}

		readonly IVisitationListener listener;

		public void Visit(KeyValues @object) {
			VisitObject(@object.Name, @object.Value);
		}

		void VisitObject(string name, KVValue value) {
			switch (value.ValueType) {
				case KVValueType.Collection:
					listener.OnObjectStart(name);
					VisitValue((IEnumerable<KeyValues>)value);
					listener.OnObjectEnd();
					break;

				case KVValueType.FloatingPoint:
				case KVValueType.Int32:
				case KVValueType.Pointer:
				case KVValueType.String:
				case KVValueType.UInt64:
				case KVValueType.Int64:
					listener.OnKeyValuePair(name, value);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(value.ValueType), value.ValueType, "Unhandled value type.");
			}
		}

		void VisitValue(IEnumerable<KeyValues> collection) {
			foreach (var item in collection) {
				VisitObject(item.Name, item.Value);
			}
		}
	}
}
