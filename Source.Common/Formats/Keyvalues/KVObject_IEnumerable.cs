using System.Collections;
using System.Collections.Generic;

namespace Source.Common.Formats.Keyvalues
{
    /// <summary>
    /// Represents a dynamic KeyValue object.
    /// </summary>
    public partial class KeyValues : IEnumerable<KeyValues>
    {
        /// <inheritdoc/>
        public IEnumerator<KeyValues> GetEnumerator()
            => Children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Children.GetEnumerator();
    }
}
