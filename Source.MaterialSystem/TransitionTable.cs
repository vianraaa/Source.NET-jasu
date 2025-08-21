using Source.Common.Hashing;
using Source.Common.ShaderAPI;
using Source.Common.Utilities;

namespace Source.MaterialSystem;



public struct SnapshotShaderState {
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

		if(shadowStateId == -1) {
			shadowStateId = CreateShadowState(currentState);

			for (short to = 0; to < shadowStateId; to++) {
				CreateTransitionTableEntry(to, shadowStateId);
			}

			for (short from = 0; from < shadowStateId; from++) {
				CreateTransitionTableEntry(shadowStateId, from);
			}
		}

		ref ShadowShaderState currentShaderState = ref ShaderShadow.GetShadowShaderState();
		StateSnapshot_t snapshotId = FindStateSnapshot(shadowStateId, ref currentShaderState);
		if(snapshotId == -1) {
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

		SnapshotDictEntry_t insert = new();
		fixed (SnapshotShaderState* pTemp = &shaderState) {
			CRC32.Init(ref insert.Checksum);
			CRC32.ProcessBuffer(ref insert.Checksum, pTemp, sizeof(SnapshotShaderState));
			CRC32.Final(ref insert.Checksum);
		}

		SnapshotDict[insert.Checksum] = snapshotId;
		return snapshotId;
	}

	public struct SnapshotDictEntry_t
	{
		public CRC32_t Checksum;
		public StateSnapshot_t Snapshot;
	}

	public struct ShadowStateDictEntry_t
	{
		public CRC32_t Checksum;
		public ShadowStateId_t ShadowStateId;
	}

	Dictionary<CRC32_t, StateSnapshot_t> SnapshotDict = [];

	private unsafe StateSnapshot_t FindStateSnapshot(ShadowStateId_t id, ref ShadowShaderState currentShaderState) {
		SnapshotShaderState* temp = stackalloc SnapshotShaderState[1];
		temp->ShadowStateId = id;
		temp->ShaderState = currentShaderState;
		SnapshotDictEntry_t find = new();
		CRC32.Init(ref find.Checksum);
		CRC32.ProcessBuffer(ref find.Checksum, temp, sizeof(SnapshotShaderState));
		CRC32.Final(ref find.Checksum);

		if (!SnapshotDict.TryGetValue(find.Checksum, out StateSnapshot_t snapshot))
			return -1;

		return snapshot;
	}

	private void CreateTransitionTableEntry(StateSnapshot_t shadowStateId, StateSnapshot_t from) {

	}

	private ShadowStateId_t CreateShadowState(ShadowState currentState) {
		nint newShaderState = ShadowStateList.AddToTail();
		ShadowStateList[(int)newShaderState] = currentState;

		nint i;
		for (i = 0; i < newShaderState; i++) {
			nint newElem = TransitionTable.AddToTail();
		}
	}

	private short FindShadowState(ref ShadowState currentState) {

	}
}
