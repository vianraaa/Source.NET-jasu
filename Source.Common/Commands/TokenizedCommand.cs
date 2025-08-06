namespace Source.Common.Commands;

public unsafe struct TokenizedCommand {
	char[]? buffer;
	int sliceOffset;
	int sliceLength;

	int argCount;

	const int MAX_ARGC = 64;
	public static readonly CharacterSet DefaultBreakSet = new("{}()':");

	/// <summary>
	/// How many arguments are in the tokenized command
	/// </summary>
	/// <returns></returns>
	public readonly int ArgC() => argCount;
	/// <summary>
	/// The argument buffer past the first argument
	/// </summary>
	/// <returns></returns>
	public readonly ReadOnlySpan<char> ArgS() {
		if (argCount <= 1)
			return [];

		return GetCommandString()[argSizes[0]..];
	}
	public readonly ReadOnlySpan<char> GetCommandString() => buffer == null 
		? throw new NullReferenceException("Untokenized command.") 
		: new ReadOnlySpan<char>(buffer)[sliceOffset..(sliceOffset + sliceLength)];

	fixed int argPositions[MAX_ARGC];
	fixed int argSizes[MAX_ARGC];

	public void Reset() {
		argCount = 0;
		// zero out argV
		for (int i = 0; i < MAX_ARGC; i++) {
			argPositions[i] = -1;
			argSizes[i] = -1;
		}

		this.buffer = null;
		this.sliceOffset = 0;
		this.sliceLength = 0;
	}

	public readonly ReadOnlySpan<char> this[int index] {
		get => Arg(index);
	}


	public bool Tokenize(char[] buffer, int sliceOffset, int sliceLength) {
		Reset();

		this.buffer = buffer;
		this.sliceOffset = sliceOffset;
		this.sliceLength = sliceLength;
		
		return true;
	}

	public readonly ReadOnlySpan<char> Arg(int index) {
		int baseAddr = argPositions[index];
		int endAddr = baseAddr + argSizes[index];
		return GetCommandString()[baseAddr..endAddr];
	}

	public readonly ReadOnlySpan<char> FindArg(ReadOnlySpan<char> name) {
		for (int i = 1; i < argCount; i++) {
			if (Arg(i).Equals(name, StringComparison.OrdinalIgnoreCase))
				return (i + 1) < argCount ? Arg(i + 1) : "";
		}
		return null;
	}
}
