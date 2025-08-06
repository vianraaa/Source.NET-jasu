namespace Source.Common.Filesystem;

[Flags]
public enum FileOpenOptions
{
	// The first two bits are the overall operation occuring on the file
	Invalid = 0b00,
	Read = 0b01,
	Write = 0b10,
	Append = 0b11,

	// The following bits are bitflag fields.
	Extended = 1 << 2,
	Binary = 1 << 3,
	Text = 1 << 4,

	ReadEx = Read + Extended,
	WriteEx = Write + Extended,
	AppendEx = Append + Extended,
}
