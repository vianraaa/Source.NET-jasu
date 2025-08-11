using Microsoft.Extensions.DependencyInjection;
using Source.Common.Compression;
using Source.Common.Engine;
using Source.Common.Filesystem;

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static Source.Dbg;

namespace Source.Engine;

/// <summary>
/// Common functionality
/// </summary>
/// <param name="providers"></param>
public class Common(IServiceProvider providers)
{
	readonly static CharacterSet BreakSet = new("{}()");
	readonly static CharacterSet BreakSetIncludingColons = new("{}()':");

	public void InitFilesystem(ReadOnlySpan<char> fullModPath) {
		CFSSearchPathsInit initInfo = new();
		IEngineAPI engineAPI = providers.GetRequiredService<IEngineAPI>();
		Host Host = providers.GetRequiredService<Host>();
		FileSystem FileSystem = providers.GetRequiredService<FileSystem>();

		initInfo.FileSystem = engineAPI.GetRequiredService<IFileSystem>();
		initInfo.DirectoryName = new(fullModPath);
		if(initInfo.DirectoryName == null) 
			initInfo.DirectoryName = Host.GetCurrentGame();

		Host.CheckGore();

		initInfo.LowViolence = Host.LowViolence;
		initInfo.MountHDContent = false; // Study this further

		FileSystem.LoadSearchPaths(in initInfo);
	}

	public bool Initialized { get; private set; }
	public void Init() {
		Initialized = true;
	}

	public static bool IsValidPath(ReadOnlySpan<char> filename) {
		if (filename == null)
			return false;

		if (filename.Length == 0
			|| filename.Contains("\\\\", StringComparison.OrdinalIgnoreCase) // To protect network paths
			|| filename.Contains(":", StringComparison.OrdinalIgnoreCase) // To protect absolute paths
			|| filename.Contains("..", StringComparison.OrdinalIgnoreCase) // To protect relative paths
			|| filename.Contains("\n", StringComparison.OrdinalIgnoreCase)
			|| filename.Contains("\r", StringComparison.OrdinalIgnoreCase)
		)
			return false;

		return true;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LzssHeader
	{
		public uint id;
		public uint actualSize;
	}
	public const uint LZSS_ID = 0x4C5A5353;   // 'L''Z''S''S' in big-endian

	public static int GetUncompressedSize(byte[] compressed, int compressedLen)
	{
		if (compressedLen < Marshal.SizeOf<LzssHeader>())
			return -1;

		var id = BinaryPrimitives.ReadUInt32BigEndian(compressed.AsSpan(0, 4));
		if (id == LZSS_ID)
		{
			uint actualSize = BinaryPrimitives.ReadUInt32LittleEndian(compressed.AsSpan(4, 4));
			return (int)actualSize;
		}

		// Newer source games use Snappy though GMod does not.

		return -1;
	}

	public static bool BufferToBufferDecompress(byte[] dest, ref int destLen, byte[] source, int sourceLen)
	{
		if (source == null || dest == null)
		{
			Warning("NET_BufferToBufferDecompress: null buffer(s)");
			return false;
		}

		if (sourceLen > source.Length || destLen > dest.Length)
		{
			Warning("NET_BufferToBufferDecompress: invalid length(s)");
			return false;
		}

		Span<byte> input = new Span<byte>(source, 0, sourceLen);
		Span<byte> output = new Span<byte>(dest, 0, destLen);

		uint id = BitConverter.ToUInt32(source, 0);
		uint uncompressedSize = CLZSS.GetActualSize(input);
		if (uncompressedSize > 0)
		{
			if (uncompressedSize > destLen)
			{
				Warning("NET_BufferToBufferDecompress with improperly sized dest buffer (%i in, %i needed)", destLen, uncompressedSize);
				return false;
			}

			if (id == CLZSS.LZSS_ID)
			{
				uint actualSize = CLZSS.Uncompress(input, output);

				if (actualSize != uncompressedSize)
				{
					Warning("NET_BufferToBufferDecompress: header said %i bytes would be decompressed, but we LZSS decompressed %i", uncompressedSize, actualSize);
					return false;
				}

				destLen = (int)actualSize;
				return true;
			} else if (id == CLZSS.SNAPPY_ID) {
				Warning("NET_BufferToBufferDecompress: Snappy decompression not implemented");
				return false;
			}

			Warning("NET_BufferToBufferDecompress: Unknown compression type 0x%X", id);
			return false;
		} else {
			if (sourceLen > destLen)
			{
				Warning("NET_BufferToBufferDecompress with improperly sized dest buffer (%i in, %i needed)", destLen, sourceLen);
				return false;
			}

			Buffer.BlockCopy(source, 0, dest, 0, sourceLen);
			destLen = sourceLen;
			return true;
		}
	}
}