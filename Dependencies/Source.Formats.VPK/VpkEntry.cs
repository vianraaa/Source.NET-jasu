using System.IO;
using System.Linq;

namespace Source.Formats.VPK
{
	public class VpkEntry
	{
		public string Extension { get; set; }
		public string Path { get; set; }
		public string Filename { get; set; }
		public byte[] PreloadData { get { return ReadPreloadData(); } }
		public byte[] Data { get { return ReadData(); } }
		public bool HasPreloadData { get; set; }

		internal uint CRC;
		internal ushort PreloadBytes;
		internal uint PreloadDataOffset;
		internal ushort ArchiveIndex;
		internal uint EntryOffset;
		internal uint EntryLength;
		internal VpkArchive ParentArchive;

		public override string ToString() => $"VpkEntry '{Path}/{Filename}.{Extension}' [crc {CRC}, entry<{EntryOffset}-{EntryLength}>]";

		internal VpkEntry(VpkArchive parentArchive, uint crc, ushort preloadBytes, uint preloadDataOffset, ushort archiveIndex, uint entryOffset,
			uint entryLength, string extension, string path, string filename) {
			ParentArchive = parentArchive;
			CRC = crc;
			PreloadBytes = preloadBytes;
			PreloadDataOffset = preloadDataOffset;
			ArchiveIndex = archiveIndex;
			EntryOffset = entryOffset;
			EntryLength = entryLength;
			Extension = extension;
			Path = path;
			Filename = filename;
			HasPreloadData = preloadBytes > 0;
		}

		private byte[] ReadPreloadData() {
			if (PreloadBytes > 0) {
				var buff = new byte[PreloadBytes];
				using (var fs = new FileStream(ParentArchive.ArchivePath, FileMode.Open, FileAccess.Read)) {
					buff = new byte[PreloadBytes];
					fs.Seek(PreloadDataOffset, SeekOrigin.Begin);
					fs.Read(buff, 0, buff.Length);
				}
				return buff;
			}
			return null;
		}

		private byte[]? dataCache;
		private byte[] ReadData() {
			if (dataCache != null)
				return dataCache;

			var partFile = ParentArchive.Parts.FirstOrDefault(part => part.Index == ArchiveIndex);
			if (partFile == null)
				return null;

			if (HasPreloadData) {
				dataCache = new byte[PreloadBytes + EntryLength];
				using (var fs = new FileStream(ParentArchive.ArchivePath, FileMode.Open, FileAccess.Read)) {
					fs.Seek(PreloadDataOffset, SeekOrigin.Begin);
					fs.Read(dataCache, 0, PreloadBytes);
				}

				using (var fs = new FileStream(partFile.Filename, FileMode.Open, FileAccess.Read)) {
					fs.Seek(EntryOffset, SeekOrigin.Begin);
					fs.Read(dataCache, PreloadBytes, (int)EntryLength);
				}
			}
			else {
				dataCache = new byte[EntryLength];
				using (var fs = new FileStream(partFile.Filename, FileMode.Open, FileAccess.Read)) {
					fs.Seek(EntryOffset, SeekOrigin.Begin);
					fs.Read(dataCache, 0, dataCache.Length);
				}
			}

			return dataCache;
		}

		public byte[] AnyData { get { if (this.HasPreloadData) return this.ReadPreloadData(); else return this.ReadData(); } }
	}
}
