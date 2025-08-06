using System.Collections.Generic;

namespace Source.Common.Formats.Keyvalues.Deserialization
{
	class KVPartialState<TState> : KVPartialState
	{
		public Stack<TState> States { get; } = new Stack<TState>();
	}
}
