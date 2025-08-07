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
	int argV0Size;
	char[]? argSBuffer;
	char[]? argVBuffer;
	char[][]? ppArgv;

	/// <summary>
	/// How many arguments are in the tokenized command
	/// </summary>
	/// <returns></returns>
	public readonly int ArgC() => argCount;
	/// <summary>
	/// The argument buffer past the first argument
	/// </summary>
	/// <returns></returns>
	public readonly ReadOnlySpan<char> ArgS() => argV0Size > 0 ? argSBuffer!.AsSpan()[..argV0Size] : [];
	public readonly ReadOnlySpan<char> Arg(int index) {
		if (index < 0 || index >= argCount)
			return [];

		return ppArgv![index];
	}

	public readonly ReadOnlySpan<char> GetCommandString() => argSBuffer;

	[MemberNotNull(nameof(argSBuffer))]
	[MemberNotNull(nameof(argVBuffer))]
	[MemberNotNull(nameof(ppArgv))]
	public void Reset() {
		argCount = 0;
		strlen = 0;
		argSBuffer ??= new char[COMMAND_MAX_LENGTH];
		argVBuffer ??= new char[COMMAND_MAX_LENGTH];
		ppArgv ??= new char[COMMAND_MAX_ARGC][];
	}

	public readonly ReadOnlySpan<char> this[int index] {
		get => Arg(index);
	}


	public bool Tokenize(ReadOnlySpan<char> command, CharacterSet? breakSet = null) {
		Reset();

		breakSet ??= DefaultBreakSet;

		fixed (char* pArgSBuffer = argSBuffer)
		fixed (char* pArgVBuffer = argVBuffer) {
			command.CopyTo(new Span<char>(pArgSBuffer, command.Length));
			strlen = command.Length;

			StringReader bufParse = new StringReader(new(argSBuffer, 0, command.Length));
			int argvbuffersize = 0;
			while (bufParse.IsValid() && (argCount < COMMAND_MAX_ARGC)) {
				Span<char> argvBuf = new Span<char>(pArgVBuffer, COMMAND_MAX_LENGTH)[argvbuffersize..];
				int maxLen = COMMAND_MAX_LENGTH - argvbuffersize;
				int start = bufParse.TellGet();
				int size = bufParse.ParseToken(breakSet, argvBuf[..maxLen]);
				if (size < 0)
					break;

				if (maxLen == size) {
					Reset();
					return false;
				}

				argvBuf = argvBuf[..size];

				if (argCount == 1) {
					argV0Size = bufParse.TellGet();
					bool foundEndQuote = pArgSBuffer[argV0Size - 1] == '\"';
					if (foundEndQuote)
						--argV0Size;

					argV0Size -= size;
					Dbg.Assert(argV0Size != 0);

					bool foundStartQuote = (argV0Size > start) && (pArgSBuffer[argV0Size - 1] == '\"');
					Dbg.Assert(foundStartQuote == foundEndQuote);
					if (foundStartQuote)
						--argV0Size;
				}

				// WOW - this sucks! WOW!!!
				ppArgv[argCount++] = new string(argvBuf[..size].ToArray()).Trim('\0').ToCharArray();
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
