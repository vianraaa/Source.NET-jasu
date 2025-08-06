namespace Source.Common.Formats.Keyvalues.Deserialization.KeyValues1
{
	enum KV1TextReaderState
	{
		InObjectBeforeKey,
		InObjectBetweenKeyAndValue,
		InObjectAfterValue
	}
}
