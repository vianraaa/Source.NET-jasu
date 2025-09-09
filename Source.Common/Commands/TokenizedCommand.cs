using System.Diagnostics.CodeAnalysis;

namespace Source.Common.Commands;

public unsafe struct TokenizedCommand
{

	const int COMMAND_MAX_ARGC = 64;
	const int COMMAND_MAX_LENGTH = 512;
	public static int MaxCommandLength => COMMAND_MAX_LENGTH - 1;
	public static readonly CharacterSet DefaultBreakSet = new("{}()':");

	int argCount;
	int strlen;
	char[]? argSBuffer;
	Range[] ppArgs;

	/// <summary>
	/// How many arguments are in the tokenized command?
	/// </summary>
	/// <returns></returns>
	public readonly int ArgC() => argCount;
	/// <summary>
	/// The argument buffer past the provided argument.
	/// </summary>
	/// <returns>All text, as a <see cref="ReadOnlySpan{char}"/> slice of the internal command buffer, after the provided arguments starting position (0 returning all text, 1 returning all after the initial command, etc..)</returns>
	public readonly ReadOnlySpan<char> ArgS(int startingArg = 1) 
		=> argCount > startingArg ? argSBuffer!.AsSpan()[ppArgs[startingArg].Start..ppArgs[argCount - 1].End] : [];
	public readonly ReadOnlySpan<char> Arg(int index) {
		if (index < 0 || index >= argCount)
			return [];

		return new ReadOnlySpan<char>(argSBuffer!)[ppArgs![index]];
	}

	public readonly ReadOnlySpan<char> GetCommandString() => argSBuffer;
	public readonly void CopyTo(Span<char> target) {
		ArgS(0).CopyTo(target);
	}

	[MemberNotNull(nameof(argSBuffer))]
	[MemberNotNull(nameof(ppArgs))]
	public void Reset() {
		argCount = 0;
		strlen = 0;
		argSBuffer ??= new char[COMMAND_MAX_LENGTH];
		ppArgs ??= new Range[COMMAND_MAX_ARGC];
		for (int i = 0; i < COMMAND_MAX_LENGTH; i++) {
			argSBuffer[i] = '\0';
		}
		for (int i = 0; i < ppArgs.Length; i++) {
			ppArgs[i] = new Range(0, 0);
		}
	}

	public readonly ReadOnlySpan<char> this[int index] {
		get => Arg(index);
	}


	public bool Tokenize(ReadOnlySpan<char> command, CharacterSet? breakSet = null) {
		Reset();

		breakSet ??= DefaultBreakSet;

		fixed (char* pArgSBuffer = argSBuffer){
			command.CopyTo(new Span<char>(pArgSBuffer, command.Length));
			strlen = command.Length;

			char* readPos = pArgSBuffer;
			nint readOffset = 0;

			StringReader bufParse = new StringReader(new(argSBuffer, 0, command.Length));
			int argvbuffersize = 0;
			Span<char> argvBuf = stackalloc char[COMMAND_MAX_LENGTH];

			while (bufParse.IsValid() && (argCount < COMMAND_MAX_ARGC) && (readOffset < COMMAND_MAX_LENGTH)) {
				int maxLen = COMMAND_MAX_LENGTH - argvbuffersize;
				bufParse.EatWhiteSpace();
				int start = bufParse.TellGet();
				int size = bufParse.ParseToken(breakSet, argvBuf[..maxLen]);
				if (size < 0)
					break;

				if (maxLen == size) {
					Reset();
					return false;
				}

				while (size > 0 && argvBuf[size - 1] == '\0')
					size--;

				ppArgs[argCount++] = new(start, start + size);

				if (argCount >= COMMAND_MAX_ARGC)
					Dbg.Warning("CCommand::Tokenize: Encountered command which overflows the argument buffer.. Clamped!\n");

				argvbuffersize += size + 1;
				Dbg.Assert(argvbuffersize <= COMMAND_MAX_LENGTH);
			}

			return true;
		}
	}
	public readonly ReadOnlySpan<char> FindArg(ReadOnlySpan<char> name) {
		for (int i = 1; i < argCount; i++) {
			if (Arg(i).Equals(name, StringComparison.OrdinalIgnoreCase))
				return (i + 1) < argCount ? Arg(i + 1) : "";
		}
		return null;
	}
}
