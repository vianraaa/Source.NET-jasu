using Source.Common.Formats.Keyvalues;

using System.Collections.Generic;

namespace Source.Common.Formats.Keyvalues.Deserialization
{
	class KVPartialState
	{
		public string Key { get; set; }

		public KVValue Value { get; set; }

		public IList<KeyValues> Items { get; } = new List<KeyValues>();

		public bool Discard { get; set; }
	}
}
