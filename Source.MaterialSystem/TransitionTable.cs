using Source.Common.Utilities;

namespace Source.MaterialSystem;



public struct SnapshotShaderState {
	public ShadowShaderState ShaderState;
	public ShadowStateId_t ShadwStateId;
	public ushort Reserved;
	public uint Reserved2;
}

public class TransitionTable
{
	RefStack<ShadowState> ShadowStateList = [];
	RefStack<SnapshotShaderState> SnapshotList = [];
	public ref ShadowShaderState GetSnapshotShader(StateSnapshot_t snapshotID) {
		Assert(snapshotID >= 0 && snapshotID < SnapshotList.Count);
		return ref SnapshotList[snapshotID].ShaderState;
	}
}
