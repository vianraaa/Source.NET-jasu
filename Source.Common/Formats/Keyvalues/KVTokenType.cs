namespace Source.Common.Formats.Keyvalues
{
	enum KVTokenType
	{
		ObjectStart,
		ObjectEnd,
		String,
		EndOfFile,
		Comment,
		Condition,
		IncludeAndAppend,
		IncludeAndMerge
	}
}
