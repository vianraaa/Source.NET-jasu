using Source.Common.Hashing;
using Source.Common.ShaderAPI;
using Source.Common.Utilities;

using System.Runtime.CompilerServices;

namespace Source.MaterialSystem;



public struct SnapshotShaderState
{
	public ShadowShaderState ShaderState;
	public ShadowStateId_t ShadowStateId;
	public ushort Reserved;
	public uint Reserved2;
}

public class TransitionTable(ShaderShadowGl46 ShaderShadow)
{
	RefStack<ShadowState> ShadowStateList = [];
	RefStack<SnapshotShaderState> SnapshotList = [];
	public ref ShadowState GetSnapshot(StateSnapshot_t snapshotID) {
		Assert(snapshotID >= 0 && snapshotID < SnapshotList.Count);
		return ref ShadowStateList[SnapshotList[snapshotID].ShadowStateId];
	}
	public ref ShadowShaderState GetSnapshotShader(StateSnapshot_t snapshotID) {
		Assert(snapshotID >= 0 && snapshotID < SnapshotList.Count);
		return ref SnapshotList[snapshotID].ShaderState;
	}

	internal StateSnapshot_t TakeSnapshot() {
		ShaderShadow.ComputeAggregateShadowState();

		ref ShadowState currentState = ref ShaderShadow.GetShadowState();
		ShadowStateId_t shadowStateId = FindShadowState(ref currentState);

		if (shadowStateId == -1) {
			shadowStateId = CreateShadowState(ref currentState);

			for (short to = 0; to < shadowStateId; to++) {
				CreateTransitionTableEntry(to, shadowStateId);
			}

			for (short from = 0; from < shadowStateId; from++) {
				CreateTransitionTableEntry(shadowStateId, from);
			}
		}

		ref ShadowShaderState currentShaderState = ref ShaderShadow.GetShadowShaderState();
		StateSnapshot_t snapshotId = FindStateSnapshot(shadowStateId, ref currentShaderState);
		if (snapshotId == -1) {
			snapshotId = CreateStateSnapshot(shadowStateId, ref currentShaderState);
		}

		return snapshotId;
	}

	private unsafe StateSnapshot_t CreateStateSnapshot(ShadowStateId_t shadowStateId, ref ShadowShaderState currentShaderState) {
		SnapshotList.Push();
		StateSnapshot_t snapshotId = (StateSnapshot_t)SnapshotList.Count;
		ref SnapshotShaderState shaderState = ref SnapshotList[snapshotId];
		shaderState.ShadowStateId = shadowStateId;
		shaderState.ShaderState = currentShaderState;

		CRC32_t checksum = new();
		fixed (SnapshotShaderState* pTemp = &shaderState) {
			CRC32.Init(ref checksum);
			CRC32.ProcessBuffer(ref checksum, pTemp, sizeof(SnapshotShaderState));
			CRC32.Final(ref checksum);
		}

		SnapshotDict[checksum] = snapshotId;
		return snapshotId;
	}

	SortedList<CRC32_t, ShadowStateId_t> ShadowStateDict = [];
	SortedList<CRC32_t, ShadowStateId_t> SnapshotDict = [];

	private unsafe StateSnapshot_t FindStateSnapshot(ShadowStateId_t id, ref ShadowShaderState currentShaderState) {
		SnapshotShaderState* temp = stackalloc SnapshotShaderState[1];
		temp->ShadowStateId = id;
		temp->ShaderState = currentShaderState;
		CRC32_t checksum = new();
		CRC32.Init(ref checksum);
		CRC32.ProcessBuffer(ref checksum, temp, sizeof(SnapshotShaderState));
		CRC32.Final(ref checksum);

		if (!SnapshotDict.TryGetValue(checksum, out StateSnapshot_t snapshot))
			return -1;

		return snapshot;
	}

	public void UseSnapshot(StateSnapshot_t snapshotId) {
		ShadowStateId_t id = SnapshotList[snapshotId].ShadowStateId;
		if (CurrentSnapshotId != snapshotId) {
			// First apply things that are in the transition table
			if (CurrentShadowId != id) {
				TransitionList transition = transitionTable[id][CurrentShadowId];
				ApplyTransition(transition, id);
			}

			// NOTE: There is an opportunity here to set non-dynamic state that we don't
			// store in the transition list if we ever need it.

			CurrentSnapshotId = snapshotId;
		}

		// NOTE: This occurs regardless of whether the snapshot changed because it depends
		// on dynamic state (namely, the dynamic vertex + pixel shader index)
		// Followed by things that are not
		ApplyShaderState(ShadowStateList[id], SnapshotList[snapshotId].ShaderState);
	}

	private void ApplyShaderState(in ShadowState shadowState, in ShadowShaderState shaderState) {
		if (!shadowState.UsingFixedFunction) {

		}
		else {
			
		}
	}

	private void ApplyTransition(TransitionList transition, short id) {
		throw new NotImplementedException();
	}

	StateSnapshot_t CurrentSnapshotId;
	ShadowStateId_t CurrentShadowId;
	public int CurrentSnapshot() => (int)CurrentSnapshotId;

	private void CreateTransitionTableEntry(StateSnapshot_t shadowStateId, StateSnapshot_t from) {

	}

	struct TransitionList
	{
		public uint FirstOperation;
		public uint NumOperations;
	}

	const int INVALID_TRANSITION_OP = 0xFFFFFF;
	List<RefStack<TransitionList>> transitionTable = [];

	private ShadowStateId_t CreateShadowState(ref ShadowState currentState) {
		nint newShaderState = ShadowStateList.AddToTail();
		ShadowStateList[(int)newShaderState] = currentState;

		// all existing states must transition to the new state
		nint i;
		for (i = 0; i < newShaderState; ++i) {
			// Add a new transition to all existing states
			nint newElem = transitionTable[(int)i].AddToTail();
			transitionTable[(int)i][(int)newElem].FirstOperation = INVALID_TRANSITION_OP;
			transitionTable[(int)i][(int)newElem].NumOperations = 0;
		}

		// Add a new vector for this transition
		transitionTable.Add(new());
		nint newTransitionElem = transitionTable.Count - 1;
		transitionTable[(int)newTransitionElem].EnsureCapacity(32);
		Assert(newShaderState == newTransitionElem);

		for (i = 0; i <= newShaderState; ++i) {
			// Add a new transition from all existing states
			nint newElem = transitionTable[(int)newShaderState].AddToTail();
			transitionTable[(int)newShaderState][(int)newElem].FirstOperation = INVALID_TRANSITION_OP;
			transitionTable[(int)newShaderState][(int)newElem].NumOperations = 0;
		}

		CRC32_t checksum = new();
		CRC32.Init(ref checksum);
		unsafe {
			CRC32.ProcessBuffer(ref checksum, Unsafe.AsPointer(ref ShadowStateList[(int)newShaderState]), Unsafe.SizeOf<ShadowState>());
		}
		CRC32.Final(ref checksum);

		SnapshotDict[checksum] = (ShadowStateId_t)newShaderState;

		return (ShadowStateId_t)newShaderState;
	}

	private unsafe ShadowStateId_t FindShadowState(ref ShadowState currentState) {
		CRC32_t checksum = new();
		CRC32.Init(ref checksum);
		CRC32.ProcessBuffer(ref checksum, Unsafe.AsPointer(ref currentState), Unsafe.SizeOf<ShadowState>());
		CRC32.Final(ref checksum);

		int nDictCount = ShadowStateDict.Count;
		if (ShadowStateDict.TryGetValue(checksum, out ShadowStateId_t v))
			return v;

		return (ShadowStateId_t)(-1);
	}

	public unsafe ref ShadowState CurrentShadowState() {
		if(CurrentShadowId == -1)
			return ref Unsafe.NullRef<ShadowState>();

		Assert(CurrentShadowId >= 0 && (CurrentShadowId < ShadowStateList.Count));
		return ref ShadowStateList[CurrentShadowId];
	}
}
