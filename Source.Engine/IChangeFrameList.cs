namespace Source.Engine;

public interface IChangeFrameList : IDisposable
{
	int GetNumProps();
	void SetChangeTick(ReadOnlySpan<int> propIndices, int tick);
	int GetPropsChangedAfterTick(int tick, Span<int> outProps);
	IChangeFrameList Copy();
}
