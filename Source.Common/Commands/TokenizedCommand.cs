namespace Source.Common.Commands;

public unsafe ref struct TokenizedCommand {
	ReadOnlySpan<char> commandBuffer;
	int argCount;

	const int MAX_ARGC = 64;
	/// <summary>
	/// How many arguments are in the tokenized command
	/// </summary>
	/// <returns></returns>
	public int ArgC() => argCount;
	/// <summary>
	/// The argument buffer past the first argument
	/// </summary>
	/// <returns></returns>
	public ReadOnlySpan<char> ArgS() {
		if (argCount <= 1)
			return [];

		return commandBuffer[argSizes[0]..];
	}
	public ReadOnlySpan<char> GetCommandString() => commandBuffer;

	fixed int argPositions[MAX_ARGC];
	fixed int argSizes[MAX_ARGC];

	public void Reset() {
		commandBuffer = null;
		argCount = 0;
		// zero out argV
		for (int i = 0; i < MAX_ARGC; i++) {
			argPositions[i] = -1;
			argSizes[i] = -1;
		}
	}

	public ReadOnlySpan<char> this[int index] {
		get => Arg(index);
	}


	public bool Tokenize(ReadOnlySpan<char> command) {
		Reset();
		
		if (command == null)
			return false;
		
		commandBuffer = command;

		return true;
	}

	public ReadOnlySpan<char> Arg(int index) {
		int baseAddr = argPositions[index];
		int endAddr = baseAddr + argSizes[index];
		return commandBuffer[baseAddr..endAddr];
	}

	public ReadOnlySpan<char> FindArg(ReadOnlySpan<char> name) {
		for (int i = 1; i < argCount; i++) {
			if (Arg(i).Equals(name, StringComparison.OrdinalIgnoreCase))
				return (i + 1) < argCount ? Arg(i + 1) : "";
		}
		return null;
	}
}
