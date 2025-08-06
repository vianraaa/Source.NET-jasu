using Source.Common.Formats.Keyvalues.Abstraction;
using Source.Common.Formats.Keyvalues;

using System.Collections.Generic;

namespace Source.Common.Formats.Keyvalues.Deserialization
{
	class KVObjectBuilder : IParsingVisitationListener
	{
		readonly IList<KVObjectBuilder> associatedBuilders = new List<KVObjectBuilder>();

		public KeyValues GetObject() {
			if (stateStack.Count != 1) {
				throw new KeyValueException($"Builder is not in a fully completed state (stack count is {stateStack.Count}).");
			}

			foreach (var associatedBuilder in associatedBuilders) {
				associatedBuilder.FinalizeState();
			}

			var state = stateStack.Peek();
			return MakeObject(state);
		}

		readonly Stack<KVPartialState> stateStack = new();

		public void OnKeyValuePair(string name, KVValue value) {
			if (StateStack.Count > 0) {
				var state = StateStack.Peek();
				state.Items.Add(new KeyValues(name, value));
			}
			else {
				var state = new KVPartialState {
					Key = name,
					Value = value
				};

				StateStack.Push(state);
			}
		}

		public void OnObjectEnd() {
			if (StateStack.Count <= 1) {
				return;
			}

			var state = StateStack.Pop();

			var completedObject = MakeObject(state);

			var parentState = StateStack.Peek();
			parentState.Items.Add(completedObject);
		}

		public void DiscardCurrentObject() {
			var state = StateStack.Peek();
			if (state.Items?.Count > 0) {
				state.Items.RemoveAt(state.Items.Count - 1);
			}
			else {
				StateStack.Pop();
			}
		}

		public void OnObjectStart(string name) {
			var state = new KVPartialState {
				Key = name
			};
			StateStack.Push(state);
		}

		public IParsingVisitationListener GetMergeListener() {
			var builder = new KVMergingObjectBuilder(this);
			associatedBuilders.Add(builder);
			return builder;
		}

		public IParsingVisitationListener GetAppendListener() {
			var builder = new KVAppendingObjectBuilder(this);
			associatedBuilders.Add(builder);
			return builder;
		}

		public void Dispose() {
		}

		internal Stack<KVPartialState> StateStack => stateStack;

		protected virtual void FinalizeState() {
			foreach (var associatedBuilder in associatedBuilders) {
				associatedBuilder.FinalizeState();
			}
		}

		KeyValues MakeObject(KVPartialState state) {
			if (state.Discard) {
				return null;
			}

			KeyValues @object;

			if (state.Value != null) {
				@object = new KeyValues(state.Key, state.Value);
			}
			else {
				@object = new KeyValues(state.Key, state.Items);
			}

			return @object;
		}
	}
}
