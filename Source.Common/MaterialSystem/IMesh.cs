namespace Source.Common.MaterialSystem;

public unsafe struct VertexDesc {
	public const int VERTEX_MAX_TEXTURE_COORDINATES = 8;
	public const int BONE_MATRIX_INDEX_INVALID = 255;

	public int VertexSize_Position;
	public int VertexSize_BoneWeight;
	public int VertexSize_BoneMatrixIndex;
	public int VertexSize_Normal;
	public int VertexSize_Color;
	public int VertexSize_Specular;
	public fixed int VertexSize_TexCoord[VERTEX_MAX_TEXTURE_COORDINATES];
	public int VertexSize_TangentS;
	public int VertexSize_TangentT;
	public int VertexSize_Wrinkle;

	public int VertexSize_UserData;
	public int ActualVertexSize;

	public VertexCompressionType CompressionType;
	public int NumBoneWeights;

	public float* Position;
	public float* BoneWeight;
	public byte* BoneMatrixIndex;
	public float* Normal;
	public byte* Color;
	public byte* Specular;
	public float** TexCoord;
	public float* TangentS;
	public float* TangentT;
	public float* Wrinkle;
	public float* UserData;

	public uint FirstIndex;
	public uint Offset;
}
public unsafe struct IndexDesc {
	public const int INDEX_BUFFER_SIZE = 32768;
	public const int DYNAMIC_VERTEX_BUFFER_MEMORY = (1024 + 512) * 1024;
	public const int DYNAMIC_VERTEX_BUFFER_MEMORY_SMALL = 384 * 1024;

	public ushort* Indices;
	public uint FirstIndex;
	public uint Offset;
	public byte IndexSize;
}
public struct MeshDesc {
	public VertexDesc Vertices;
	public IndexDesc Indices;
}

public interface IVertexBuffer {
	int VertexCount();
	VertexFormat GetVertexFormat();
	bool IsDynamic();
	void BeginCastBuffer(VertexFormat format);
	void EndCastBuffer();
	int GetRoomRemaining();
	bool Lock(int vetexCount, bool append, in VertexDesc desc);
	bool Unlock(int vetexCount, in VertexDesc desc);
	void Spew(int vertexCount, out VertexDesc desc);
	void ValidateData(int vertexCount, out VertexDesc desc);
}
public interface IIndexBuffer {
	int IndexCount();
	MaterialIndexFormat GetIndexFormat();
	bool IsDynamic();
	void BeginCastBuffer(MaterialIndexFormat format);
	void EndCastBuffer();
	int GetRoomRemaining();
	bool Lock(int vetexCount, bool append, in IndexDesc desc);
	bool Unlock(int vetexCount, in IndexDesc desc);
	void Spew(int vertexCount, out IndexDesc desc);
	void ValidateData(int vertexCount, out IndexDesc desc);
}

public interface IMesh {
	void SetPrimitiveType(MaterialPrimitiveType type);
	void Draw(int firstIndex = -1, int indexCount = 0);
	void SetColorMesh(IMesh colorMesh, int vertexOffset);
}
[Flags]
public enum VertexFormat : ulong {
	Position = 0x0001,
	Normal = 0x0002,
	Color = 0x0004,
	Specular = 0x0008,
	TangentS = 0x0010,
	TangentT = 0x0020,
	TangentSpace = TangentS | TangentT,
	Wrinkle = 0x0040,
	BoneIndex = 0x0080,
	VertexShader = 0x0100,
	UseExactFormat = 0x0200,
	Compressed = 0x0400,

	LastBit = 10,
	BoneWeightBit = LastBit + 1,
	UserDataSizeBit = LastBit + 4,
	TexCoordSizeBit = LastBit + 7,

	BoneWeightMask = 0x7L << (int)BoneWeightBit,
	UserDataSizeMask = 0x7L << (int)UserDataSizeBit,
	FormatFieldMask = 0x0FF,

	Unknown = 0,

	BoneWeight1 = 1L << (int)BoneWeightBit,
	BoneWeight2 = 2L << (int)BoneWeightBit,
	BoneWeight3 = 3L << (int)BoneWeightBit,
	BoneWeight4 = 4L << (int)BoneWeightBit,

	UserDataSize1 = 1L << (int)UserDataSizeBit,
	UserDataSize2 = 2L << (int)UserDataSizeBit,
	UserDataSize3 = 3L << (int)UserDataSizeBit,
	UserDataSize4 = 4L << (int)UserDataSizeBit,

	TexCoordMask1 = (0x7ul) << (int)(TexCoordSizeBit + 3 * 1),
	TexCoordMask2 = (0x7ul) << (int)(TexCoordSizeBit + 3 * 2),
	TexCoordMask3 = (0x7ul) << (int)(TexCoordSizeBit + 3 * 3),
	TexCoordMask4 = (0x7ul) << (int)(TexCoordSizeBit + 3 * 4),
	TexCoordMask5 = (0x7ul) << (int)(TexCoordSizeBit + 3 * 5),
	TexCoordMask6 = (0x7ul) << (int)(TexCoordSizeBit + 3 * 6),
}

public static class VertexFormatExts {
	public static int VertexFlags(this VertexFormat vertexFormat) => (int)(vertexFormat & (VertexFormat)(1 << ((int)VertexFormat.LastBit + 1) - 1));
	public static int NumBoneWeights(this VertexFormat vertexFormat) => ((int)vertexFormat >> (int)VertexFormat.BoneWeightBit) & 0x7;
	public static int UserDataSize(this VertexFormat vertexFormat) => ((int)vertexFormat >> (int)VertexFormat.UserDataSizeBit) & 0x7;
	public static int TexCoordSize(this VertexFormat vertexFormat, int texCoordIndex) => ((int)vertexFormat >> (int)VertexFormat.TexCoordSizeBit + 3 * texCoordIndex) & 0x7;
	public static bool UsesVertexShader(this VertexFormat vertexFormat) => (vertexFormat & VertexFormat.VertexShader) != 0;

	public static VertexFormat VertexTexcoordSize(int index, int coords)
		=> (VertexFormat)(coords << ((int)VertexFormat.TexCoordSizeBit + (3 * index)));

	public static ushort GetVertexElementSize(this VertexElement element, VertexCompressionType compressionType = VertexCompressionType.None) {
		if(compressionType == VertexCompressionType.On) {
			switch (element) {
				case VertexElement.Normal:
					return 4 * sizeof(byte);
				case VertexElement.UserData4:
					return 0;

				case VertexElement.BoneWeights1:
				case VertexElement.BoneWeights2:
					return (2 * sizeof(short));

				default:
					break;
			}
		}

		switch (element) {
			case VertexElement.Position: return (3 * sizeof(float));
			case VertexElement.Normal: return (3 * sizeof(float));
			case VertexElement.Color: return (4 * sizeof(byte) );
			case VertexElement.Specular: return (4 * sizeof(byte) );
			case VertexElement.TangentS: return (3 * sizeof(float));
			case VertexElement.TangentT: return (3 * sizeof(float));
			case VertexElement.Wrinkle: return (1 * sizeof(float)); // Packed into Position.W
			case VertexElement.BoneIndex: return (4 * sizeof(byte) );
			case VertexElement.BoneWeights1: return (1 * sizeof(float));
			case VertexElement.BoneWeights2: return (2 * sizeof(float));
			case VertexElement.BoneWeights3: return (3 * sizeof(float));
			case VertexElement.BoneWeights4: return (4 * sizeof(float));
			case VertexElement.UserData1: return (1 * sizeof(float));
			case VertexElement.UserData2: return (2 * sizeof(float));
			case VertexElement.UserData3: return (3 * sizeof(float));
			case VertexElement.UserData4: return (4 * sizeof(float));
			case VertexElement.TexCoord1D_0: return (1 * sizeof(float));
			case VertexElement.TexCoord1D_1: return (1 * sizeof(float));
			case VertexElement.TexCoord1D_2: return (1 * sizeof(float));
			case VertexElement.TexCoord1D_3: return (1 * sizeof(float));
			case VertexElement.TexCoord1D_4: return (1 * sizeof(float));
			case VertexElement.TexCoord1D_5: return (1 * sizeof(float));
			case VertexElement.TexCoord1D_6: return (1 * sizeof(float));
			case VertexElement.TexCoord1D_7: return (1 * sizeof(float));
			case VertexElement.TexCoord2D_0: return (2 * sizeof(float));
			case VertexElement.TexCoord2D_1: return (2 * sizeof(float));
			case VertexElement.TexCoord2D_2: return (2 * sizeof(float));
			case VertexElement.TexCoord2D_3: return (2 * sizeof(float));
			case VertexElement.TexCoord2D_4: return (2 * sizeof(float));
			case VertexElement.TexCoord2D_5: return (2 * sizeof(float));
			case VertexElement.TexCoord2D_6: return (2 * sizeof(float));
			case VertexElement.TexCoord2D_7: return (2 * sizeof(float));
			case VertexElement.TexCoord3D_0: return (3 * sizeof(float));
			case VertexElement.TexCoord3D_1: return (3 * sizeof(float));
			case VertexElement.TexCoord3D_2: return (3 * sizeof(float));
			case VertexElement.TexCoord3D_3: return (3 * sizeof(float));
			case VertexElement.TexCoord3D_4: return (3 * sizeof(float));
			case VertexElement.TexCoord3D_5: return (3 * sizeof(float));
			case VertexElement.TexCoord3D_6: return (3 * sizeof(float));
			case VertexElement.TexCoord3D_7: return (3 * sizeof(float));
			case VertexElement.TexCoord4D_0: return (4 * sizeof(float));
			case VertexElement.TexCoord4D_1: return (4 * sizeof(float));
			case VertexElement.TexCoord4D_2: return (4 * sizeof(float));
			case VertexElement.TexCoord4D_3: return (4 * sizeof(float));
			case VertexElement.TexCoord4D_4: return (4 * sizeof(float));
			case VertexElement.TexCoord4D_5: return (4 * sizeof(float));
			case VertexElement.TexCoord4D_6: return (4 * sizeof(float));
			case VertexElement.TexCoord4D_7: return (4 * sizeof(float));
		}

		Dbg.Assert(false);
		return 0;
	}
}

public enum VertexCompressionType : uint {
	Invalid = 0xFFFFFFFF,
	None = 0,
	On = 1
}

public enum VertexElement {
	None = -1,
	Position = 0,
	Normal = 1, 
	Color = 2,
	Specular = 3,
	TangentS = 4,
	TangentT = 5,
	Wrinkle = 6,
	BoneIndex = 7,

	BoneWeights1 = 8,
	BoneWeights2 = 9,
	BoneWeights3 = 10,
	BoneWeights4 = 11,

	UserData1 = 12,
	UserData2 = 13,
	UserData3 = 14,
	UserData4 = 15,

	TexCoord1D_0 = 16,
	TexCoord1D_1 = 17,
	TexCoord1D_2 = 18,
	TexCoord1D_3 = 19,
	TexCoord1D_4 = 20,
	TexCoord1D_5 = 21,
	TexCoord1D_6 = 22,
	TexCoord1D_7 = 23,

	TexCoord2D_0 = 24,
	TexCoord2D_1 = 25,
	TexCoord2D_2 = 26,
	TexCoord2D_3 = 27,
	TexCoord2D_4 = 28,
	TexCoord2D_5 = 29,
	TexCoord2D_6 = 30,
	TexCoord2D_7 = 31,

	TexCoord3D_0 = 32,
	TexCoord3D_1 = 33,
	TexCoord3D_2 = 34,
	TexCoord3D_3 = 35,
	TexCoord3D_4 = 36,
	TexCoord3D_5 = 37,
	TexCoord3D_6 = 38,
	TexCoord3D_7 = 39,

	TexCoord4D_0 = 40,
	TexCoord4D_1 = 41,
	TexCoord4D_2 = 42,
	TexCoord4D_3 = 43,
	TexCoord4D_4 = 44,
	TexCoord4D_5 = 45,
	TexCoord4D_6 = 46,
	TexCoord4D_7 = 47,
	NumElements = 48
}