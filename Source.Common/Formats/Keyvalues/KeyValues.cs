using Source.Common.Formats.Keyvalues;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Source.Common.Formats.Keyvalues
{
	/// <summary>
	/// Represents a dynamic KeyValue object.
	/// </summary>
	[DebuggerDisplay("{DebuggerDescription}")]
    public partial class KeyValues
    {
		public int Count => (Value as KVCollectionValue).Count;

		internal KeyValues Parent;
		internal int Pointer;
		public KeyValues GetFirstSubKey() {
			KVCollectionValue collection = (Value as KVCollectionValue)!;

			KeyValues subkey = collection.FirstOrDefault();
			if (subkey == null) return null;

			// Sets up for iteration
			subkey.Parent = this;
			subkey.Pointer = 0;

			return subkey;
		}
		public KeyValues GetNextKey() {
			if (Parent == null) 
				throw new NullReferenceException("No parent to iterate over");

			KVCollectionValue collection = (Parent.Value as KVCollectionValue)!;

			int nextPointer = Pointer + 1;
			if (nextPointer >= collection.Count)
				return null;

			KeyValues subkey = collection[nextPointer];
			subkey.Parent = Parent;
			subkey.Pointer = nextPointer;

			return subkey;
		}

		public KeyValues FindKey(string key, bool create = false) {
			if (string.IsNullOrEmpty(key))
				return this;

			const int workingKeyMaxSz = 256;
			Span<char> workingKey = stackalloc char[workingKeyMaxSz];
			int writePtr = 0;
			KVCollectionValue workingItem = GetCollectionValue();
			string lastName = Name;

			for (int i = 0; i <= key.Length; i++) {
				if(i >= key.Length || key[i] == '/') {
					// Process one working key.
					var keyF = new string(workingKey[..writePtr]);
					var item = workingItem[key];

					// The end
					if (i >= key.Length) {
						if (item == null || item.ValueType == KVValueType.Null) {
							if (create)
								item = new KVCollectionValue();
							else
								return null;
						}
						KeyValues ret = new(keyF, item);
						ret.Parent = new KeyValues(lastName, (KVValue)workingItem);
						return ret;
					}
					// Start working on a new collection if applicable
					else if (item is null)
						return null;
					else if (item is KVCollectionValue kvo) {
						lastName = keyF;
						workingItem = kvo;
					}
					// Force return since we can't continue
					else {
						KeyValues ret = new(keyF, item);
						ret.Parent = new KeyValues(lastName, (KVValue)workingItem);
						return ret;
					}
				}
				else {
					// Write via writePtr to workingKey
					if (writePtr > workingKeyMaxSz)
						throw new OverflowException($"Key is too large (writePtr greater than workingKeyMaxSz: {workingKeyMaxSz})");

					workingKey[writePtr++] = key[i];
				}
			}

			return null;
		}

		public KeyValues(string name) {
			Require.NotNull(name, nameof(name));

			Name = name;
		}

		
		/// <summary>
		/// Initializes a new instance of the <see cref="KeyValues"/> class.
		/// </summary>
		/// <param name="name">Name of this object.</param>
		/// <param name="value">Value of this object.</param>
		public KeyValues(string name, KVValue value)
        {
            Require.NotNull(name, nameof(name));

            Name = name;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValues"/> class.
        /// </summary>
        /// <param name="name">Name of this object.</param>
        /// <param name="items">Child items of this object.</param>
        public KeyValues(string name, IEnumerable<KeyValues> items)
        {
            Require.NotNull(name, nameof(name));
            Require.NotNull(items, nameof(items));

            Name = name;
            var value = new KVCollectionValue();
            value.AddRange(items);

            Value = value;
        }

        /// <summary>
        /// Gets the name of this object.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the value of this object.
        /// </summary>
        public KVValue Value { get; protected set; }

        /// <summary>
        /// Indexer to find a child item by name.
        /// </summary>
        /// <param name="key">Key of the child object to find</param>
        /// <returns>A <see cref="KeyValues"/> if the child item exists, otherwise <c>null</c>.</returns>
        public KVValue this[string key]
        {
            get
            {
                Require.NotNull(key, nameof(key));

                var children = GetCollectionValue();
                return children[key];
            }

            set
            {
                Require.NotNull(key, nameof(key));

                var children = GetCollectionValue();
                children.Set(key, value);
            }
        }

        /// <summary>
        /// Adds a <see cref="KeyValues" /> as a child of the current object.
        /// </summary>
        /// <param name="value">The child to add.</param>
        public void Add(KeyValues value)
        {
            Require.NotNull(value, nameof(value));
            GetCollectionValue().Add(value);
        }

        /// <summary>
        /// Gets the children of this <see cref="KeyValues"/>.
        /// </summary>
        public IEnumerable<KeyValues> Children => (Value as KVCollectionValue) ?? Enumerable.Empty<KeyValues>();

        KVCollectionValue GetCollectionValue()
        {
            if (Value is not KVCollectionValue collection)
            {
                throw new InvalidOperationException($"This operation on a {nameof(KeyValues)} can only be used when the value has children.");
            }

            return collection;
        }

        string DebuggerDescription => $"{Name}: {Value}";
    }
}
