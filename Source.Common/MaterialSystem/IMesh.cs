using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.MaterialSystem;

public unsafe struct VertexDesc
{
	int PositionSize;
	int BoneWeightSize;
	int BoneMatrixIndexSize;
	int NormalSize;
	int ColorSize;
	int SpecularSize;
	int Texcoord0Size;
	int Texcoord1Size;
	int Texcoord2Size;
	int Texcoord3Size;
	int Texcoord4Size;
	int Texcoord5Size;
	int Texcoord6Size;
	int Texcoord7Size;
	int TangentSSize;
	int TangentTSize;
	int WrinkleSize;

	int UserDataSize;

	int ActualVertexSize;

	VertexCompressionType CompressionType;

	int NumBoneWeights;

	float* Position;
	float* BoneWeight;
	byte* BoneMatrixIndex;
	float* Normal;
	byte* Color;
	byte* Specular;
	float* TexCoord0;
	float* TexCoord1;
	float* TexCoord2;
	float* TexCoord3;
	float* TexCoord4;
	float* TexCoord5;
	float* TexCoord6;
	float* TexCoord7;
	float* TangentS;
	float* TangentT;
	float* Wrinkle;
	float* UserData;
	int FirstVertex;
	uint OffsetVertex;
}

public unsafe struct IndexDesc
{
	ushort* Indices;
	uint OffsetIndex;
	uint FirstIndex;
	uint IndexSize;
}

public struct MeshDesc
{
	public VertexDesc Vertex;
	public IndexDesc Index;
}

public struct ModelVertex
{
	Vector3 Position;
	Vector2 BoneWeights;
	uint BoneIndices;
	Vector3 Normal;
	uint Color;
	Vector2 TexCoord;
	Vector4 UserData;
}

public interface IVertexBuffer
{
	int VertexCount();
	VertexFormat GetVertexFormat();
	bool IsDynamic();
	void BeginCastBuffer(VertexFormat format);
	void EndCastBuffer();
	int GetRoomRemaining();
	bool Lock(int vertexCount, bool append, ref VertexDesc desc);
	bool Unlock(int vertexCount, ref VertexDesc desc);
}

public interface IIndexBuffer
{
	int IndexCount();
	MaterialIndexFormat IndexFormat();
	bool IsDynamic();
	void BeginCastBuffer(MaterialIndexFormat format);
	void EndCastBuffer();
	bool Lock(int maxIndexCount, bool append, ref IndexDesc desc);
	bool Unlock(int writtenIndexCount, ref IndexDesc desc);
}

public interface IMesh : IVertexBuffer, IIndexBuffer
{
	public const int VERTEX_MAX_TEXTURE_COORDINATES = 8;
	public const int BONE_MATRIX_INDEX_INVALID = 255;
	public const int INDEX_BUFFER_SIZE = 32768;
	public const int DYNAMIC_VERTEX_BUFFER_MEMORY = (1024 + 512) * 1024;
	public const int DYNAMIC_VERTEX_BUFFER_MEMORY_SMALL = 384 * 1024; // Only allocate this much during map transitions
	void SetPrimitiveType(MaterialPrimitiveType type);
	void Draw(int firstIndex = -1, int indexCount = 0);
	void SetColorMesh(IMesh colorMesh, int vertexOffset);
	void LockMesh(int vertexCount, int indexCount, ref MeshDesc desc);
	void ModifyBegin(int firstVertex, int vertexCount, int firstIndex, int indexCount, ref MeshDesc desc);
	void ModifyEnd(ref MeshDesc desc);
	void UnlockMesh(int vertexCount, int indexCount, ref MeshDesc desc);
	void MarkAsDrawn();
}