namespace Source.Common;

public class SendProp {

}

public class SendTable : List<SendProp>
{
	public string? NetTableName;

	protected bool Initialized;
	protected bool HasBeenWritten;
	protected bool HasPropsEncodedAgainstCurrentTickCount;

	public ReadOnlySpan<char> GetName() => NetTableName;
}