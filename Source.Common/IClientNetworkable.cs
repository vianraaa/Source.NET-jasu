using Source.Common.Bitbuffers;

namespace Source.Common;

public enum ShouldTransmiteState {
	Start,
	End
}

public enum DataUpdateType {
	Created,
	DataTableChanged
}

public interface IClientNetworkable {
	IClientUnknown GetIClientUnknown();
	void Release();
	ClientClass GetClientClass();
	void NotifyShouldTransmit(ShouldTransmiteState state);
	void OnPreDataChanged(DataUpdateType updateType);
	void OnDataChanged(DataUpdateType updateType);
	void PreDataUpdate(DataUpdateType updateType);
	void PostDataUpdate(DataUpdateType updateType);
	bool IsDormant();
	int EntIndex();
	Span<byte> GetDataTableBasePtr();
	void ReceiveMessage(int classID, bf_read msg);
	void SetDestroyedOnRecreateEntities();
}