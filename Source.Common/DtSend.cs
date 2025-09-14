namespace Source.Common;

public abstract class SendProp {
	public RecvProp? MatchingRecvProp;
	public SendPropType Type;
	public int Bits;
	public float LowValue;
	public float HighValue;
	public SendProp? ArrayProp;
	public int Elements;
	public int ElementStride;
	public string? ExcludeDTName;
	public string? VarName;

	PropFlags Flags;

	public PropFlags GetFlags() => Flags;
}

public class SendTable : List<SendProp>
{
	public string? NetTableName;

	protected bool Initialized;
	protected bool HasBeenWritten;
	protected bool HasPropsEncodedAgainstCurrentTickCount;

	public ReadOnlySpan<char> GetName() => NetTableName;
}