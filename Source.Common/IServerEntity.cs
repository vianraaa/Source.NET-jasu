using Source.Common.Engine;
using Source.Common.Server;

namespace Source.Common;

public interface IServerEntity : IServerUnknown
{
	int GetModelIndex();
	ReadOnlySpan<char> GetModelName();
	void SetModelIndex(int index);
}
public interface IServerUnknown : IHandleEntity {
	ICollideable? GetCollideable();
	IServerNetworkable? GetNetworkable();
}
