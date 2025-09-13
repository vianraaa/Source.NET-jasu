
using Source.Common;

using System.Drawing;

namespace Source.Engine;

public class FrameSnapshotEntry
{
	public ServerClass Class;
	public int SerialNumber;
	public PackedEntityHandle_t PackedData;
}

public class FrameSnapshot(FrameSnapshotManager frameSnapshotManager) : IDisposable
{
	public void AddReference() {
		Interlocked.Increment(ref References);
	}
	public void ReleaseReference() {
		Interlocked.Decrement(ref References);
		if (References == 0)
			frameSnapshotManager.DeleteFrameSnapshot(this);
	}

	public FrameSnapshot? NextSnapshot() {
		return frameSnapshotManager.NextSnapshot(this);
	}

	public volatile int ListIndex;
	public int TickCount;
	public FrameSnapshotEntry[]? Entities;
	public ushort[]? ValidEntities;
	public EventInfo[]? TempEntities;
	public readonly List<int> ExplicitDeleteSlots = [];

	volatile int References;

	public void Dispose() {
		ValidEntities = null;
		Entities = null;
		TempEntities = null;
		Assert(References == 0);

		GC.SuppressFinalize(this);
	}
}

public struct UnpackedDataCache
{
	public PackedEntity Entity;
	public int Counter;
	public int Bits;
	public InlineArrayMaxPackedEntityData<byte> Data;
}

[EngineComponent]
public class FrameSnapshotManager
{
	public const int INVALID_PACKED_ENTITY_HANDLE = 0;
	public virtual void LevelChanged() {

	}

	public FrameSnapshot CreateEmptySnapshot(int ticknumber, int maxEntities) {
		throw new NotImplementedException();
	}

	public FrameSnapshot TakeTickSnapshot(int ticknumber) {
		throw new NotImplementedException();
	}

	public FrameSnapshot NextSnapshot(FrameSnapshot snapshot) {
		throw new NotImplementedException();
	}

	public PackedEntity CreatePackedEntity(FrameSnapshot snapshot, int entity) {
		throw new NotImplementedException();
	}
	public PackedEntity GetPackedEntity(FrameSnapshot snapshot, int entity) {
		throw new NotImplementedException();
	}
	public void AddEntityReference(PackedEntityHandle_t handle) {
		throw new NotImplementedException();
	}
	public void RemoveEntityReference(PackedEntityHandle_t handle) {
		throw new NotImplementedException();
	}
	public bool UsePreviouslySentPacket(FrameSnapshot snapshot, int entity, int entSerialNumber) {
		throw new NotImplementedException();
	}
	public bool ShouldForceRepack(FrameSnapshot snapshot, int entity, PackedEntityHandle_t handle) {
		throw new NotImplementedException();
	}
	public PackedEntity GetPreviouslySentPacket(int iEntity, int iSerialNumber) {
		throw new NotImplementedException();
	}
	public UnpackedDataCache GetCachedUncompressedEntity(PackedEntity packedEntity) {
		throw new NotImplementedException();
	}

	public Mutex GetMutex() => WriteMutex;

	public void AddExplicitDelete(int slot) {
		if (!ExplicitDeleteSlots.Contains(slot))
			ExplicitDeleteSlots.Add(slot);
	}

	public void DeleteFrameSnapshot(FrameSnapshot snapshot) {
		for (int i = 0; i < (snapshot.Entities?.Length ?? 0); ++i) {
			if (snapshot.Entities![i].PackedData != INVALID_PACKED_ENTITY_HANDLE) {
				RemoveEntityReference(snapshot.Entities[i].PackedData);
			}
		}

		FrameSnapshots.Remove(snapshot);
		snapshot.Dispose();
	}

	readonly LinkedList<FrameSnapshot> FrameSnapshots = [];
	readonly ClassMemoryPool<PackedEntity> PackedEntitiesPool = new();

	int PackedEntityCacheCounter;
	readonly List<UnpackedDataCache> PackedEntityCache = [];

	InlineArrayMaxEdicts<PackedEntityHandle_t> PackedData;
	InlineArrayMaxEdicts<int> SerialNumber;

	readonly Mutex WriteMutex = new();

	readonly List<int> ExplicitDeleteSlots = [];
}