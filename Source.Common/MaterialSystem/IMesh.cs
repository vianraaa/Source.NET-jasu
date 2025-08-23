using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Source.Common.MaterialSystem;

public unsafe struct VertexDesc
{
	public int PositionSize;
	public int BoneWeightSize;
	public int BoneMatrixIndexSize;
	public int NormalSize;
	public int ColorSize;
	public int SpecularSize;
	fixed int texcoordSize[8];
	public Span<int> TexcoordSize {
		get {
			fixed (int* i = texcoordSize)
				return new(i, 8);
		}
	}

	public int TangentSSize;
	public int TangentTSize;
	public int WrinkleSize;

	public int UserDataSize;

	public int ActualVertexSize; 

	public VertexCompressionType CompressionType;

	public int NumBoneWeights;

	public float* Position;
	public float* BoneWeight;
	public byte* BoneMatrixIndex;
	public float* Normal;
	public byte* Color;
	public byte* Specular;
	public float* TexCoord0;
	public float* TexCoord1;
	public float* TexCoord2;
	public float* TexCoord3;
	public float* TexCoord4;
	public float* TexCoord5;
	public float* TexCoord6;
	public float* TexCoord7;
	public float* TangentS;
	public float* TangentT;
	public float* Wrinkle;
	public float* UserData;
	public int FirstVertex;
	public uint OffsetVertex;
}

public unsafe struct IndexDesc
{
	public ushort* Indices;
	public uint OffsetIndex;
	public uint FirstIndex;
	public uint IndexSize;
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

public struct VertexBuilder
{
	private VertexDesc Desc;
}

public struct IndexBuilder
{
	private IndexDesc Desc;
}

public struct MeshBuilder : IDisposable
{
	public MeshDesc Desc;

	public MeshBuilder() { }
	public void Dispose() => Assert(Mesh == null);  // if this fires you did a Begin() without an End()


	// This must be called before Begin, if a vertex buffer with a compressed format is to be used
	public void SetCompressionType(VertexCompressionType compressionType) => throw new NotImplementedException();

	// Locks the vertex buffer
	// (*cannot* use the Index() call below)
	public void Begin(IMesh pMesh, MaterialPrimitiveType type, int numPrimitives) => throw new NotImplementedException();

	// Locks the vertex buffer, can specify arbitrary index lists
	// (must use the Index() call below)
	public void Begin(IMesh pMesh, MaterialPrimitiveType type, int nVertexCount, int nIndexCount, ref int nFirstVertex) => throw new NotImplementedException();
	public void Begin(IMesh pMesh, MaterialPrimitiveType type, int nVertexCount, int nIndexCount) => throw new NotImplementedException();

	// forward compat
	public void Begin(IVertexBuffer pVertexBuffer, MaterialPrimitiveType type, int numPrimitives) => throw new NotImplementedException();
	public void Begin(IVertexBuffer pVertexBuffer, IIndexBuffer pIndexBuffer, MaterialPrimitiveType type, int nVertexCount, int nIndexCount, ref int nFirstVertex) => throw new NotImplementedException();
	public void Begin(IVertexBuffer pVertexBuffer, IIndexBuffer pIndexBuffer, MaterialPrimitiveType type, int nVertexCount, int nIndexCount) => throw new NotImplementedException();

	// Use this when you're done writing
	// Set bDraw to true to call m_pMesh->Draw automatically.
	public void End(bool bSpewData = false, bool bDraw = false) => throw new NotImplementedException();

	// Locks the vertex buffer to modify existing data
	// Passing nVertexCount == -1 says to lock all the vertices for modification.
	// Pass 0 for nIndexCount to not lock the index buffer.
	public void BeginModify(IMesh pMesh, int nFirstVertex = 0, int nVertexCount = -1, int nFirstIndex = 0, int nIndexCount = 0) => throw new NotImplementedException();
	public void EndModify(bool bSpewData = false) => throw new NotImplementedException();

	// A helper method since this seems to be done a whole bunch.
	public void DrawQuad(IMesh pMesh, ReadOnlySpan<float> v1, ReadOnlySpan<float> v2, ReadOnlySpan<float> v3, ReadOnlySpan<float> v4, ReadOnlySpan<byte> pColor, bool wireframe = false) => throw new NotImplementedException();

	// returns the number of indices and vertices
	public int VertexCount() => throw new NotImplementedException();
	public int IndexCount() => throw new NotImplementedException();

	// Resets the mesh builder so it points to the start of everything again
	public void Reset() => throw new NotImplementedException();

	// Returns the size of the vertex
	public int VertexSize() { return Desc.Vertex.ActualVertexSize; }

	// returns the data size of a given texture coordinate
	public int TextureCoordinateSize(int nTexCoordNumber) { return Desc.Vertex.TexcoordSize[nTexCoordNumber]; }

	// Selects the nth Vertex and Index 
	public void SelectVertex(int idx) => throw new NotImplementedException();
	public void SelectIndex(int idx) => throw new NotImplementedException();

	// Given an index, point to the associated vertex
	public void SelectVertexFromIndex(int idx) => throw new NotImplementedException();
	// Advances the current vertex and index by one
	public void AdvanceVertex() => throw new NotImplementedException();
	public void AdvanceVertices(int nVerts) => throw new NotImplementedException();
	public void AdvanceIndex() => throw new NotImplementedException();
	public void AdvanceIndices(int nIndices) => throw new NotImplementedException();
	public int GetCurrentVertex() => throw new NotImplementedException();
	public int GetCurrentIndex() => throw new NotImplementedException();

	// Data retrieval...
	public ReadOnlySpan<float> Position() => throw new NotImplementedException();
	public ReadOnlySpan<float> Normal() => throw new NotImplementedException();
	public ReadOnlySpan<uint> Color() => throw new NotImplementedException();
	public ReadOnlySpan<byte> Specular() => throw new NotImplementedException();
	public ReadOnlySpan<float> TexCoord(int stage) => throw new NotImplementedException();
	public ReadOnlySpan<float> TangentS() => throw new NotImplementedException();
	public ReadOnlySpan<float> TangentT() => throw new NotImplementedException();
	public ReadOnlySpan<float> BoneWeight() => throw new NotImplementedException();
	public ReadOnlySpan<float> Wrinkle() => throw new NotImplementedException();

	public int NumBoneWeights() => throw new NotImplementedException();

	public ReadOnlySpan<byte> BoneMatrix() => throw new NotImplementedException();
	public ReadOnlySpan<ushort> Index() => throw new NotImplementedException();

	// position setting
	public void Position3f(float x, float y, float z) => throw new NotImplementedException();
	public void Position3fv(ReadOnlySpan<float> v) => throw new NotImplementedException();

	// normal setting
	public void Normal3f(float nx, float ny, float nz) => throw new NotImplementedException();
	public void Normal3fv(ReadOnlySpan<float> n) => throw new NotImplementedException();
	public void NormalDelta3fv(ReadOnlySpan<float> n) => throw new NotImplementedException();
	public void NormalDelta3f(float nx, float ny, float nz) => throw new NotImplementedException();

	// color setting
	public void Color3f(float r, float g, float b) => throw new NotImplementedException();
	public void Color3fv(ReadOnlySpan<float> rgb) => throw new NotImplementedException();
	public void Color4f(float r, float g, float b, float a) => throw new NotImplementedException();
	public void Color4fv(ReadOnlySpan<float> rgba) => throw new NotImplementedException();

	// Faster versions of color
	public void Color3ub(byte r, byte g, byte b) => throw new NotImplementedException();
	public void Color3ubv(ReadOnlySpan<float> rgb )=> throw new NotImplementedException();
	public void Color4ub(byte r, byte g, byte b, byte a) => throw new NotImplementedException();
	public void Color4ubv(ReadOnlySpan<float> rgba )=> throw new NotImplementedException();

	// specular color setting
	public void Specular3f(float r, float g, float b) => throw new NotImplementedException();
	public void Specular3fv(ReadOnlySpan<float> rgb)=> throw new NotImplementedException();
	public void Specular4f(float r, float g, float b, float a) => throw new NotImplementedException();
	public void Specular4fv(ReadOnlySpan<float> rgba)=> throw new NotImplementedException();

	// Faster version of specular
	public void Specular3ub(byte r, byte g, byte b) => throw new NotImplementedException();
	public void Specular3ubv(ReadOnlySpan<byte> c )=> throw new NotImplementedException();
	public void Specular4ub(byte r, byte g, byte b, byte a) => throw new NotImplementedException();
	public void Specular4ubv(ReadOnlySpan<byte> c )=> throw new NotImplementedException();

	// texture coordinate setting
	public void TexCoord1f(int stage, float s) => throw new NotImplementedException();
	public void TexCoord2f(int stage, float s, float t) => throw new NotImplementedException();
	public void TexCoord2fv(int stage, ReadOnlySpan<float> st)=> throw new NotImplementedException();
	public void TexCoord3f(int stage, float s, float t, float u) => throw new NotImplementedException();
	public void TexCoord3fv(int stage, ReadOnlySpan<float> stu)=> throw new NotImplementedException();
	public void TexCoord4f(int stage, float s, float t, float u, float w) => throw new NotImplementedException();
	public void TexCoord4fv(int stage, ReadOnlySpan<float> stuv)=> throw new NotImplementedException();

	public void TexCoordSubRect2f(int stage, float s, float t, float offsetS, float offsetT, float scaleS, float scaleT) => throw new NotImplementedException();
	public void TexCoordSubRect2fv(int stage, ReadOnlySpan<float> st, ReadOnlySpan<float> offset, ReadOnlySpan<float> scale)=> throw new NotImplementedException();

	// tangent space 
	public void TangentS3f(float sx, float sy, float sz) => throw new NotImplementedException();
	public void TangentS3fv(ReadOnlySpan<float> s)=> throw new NotImplementedException();

	public void TangentT3f(float tx, float ty, float tz) => throw new NotImplementedException();
	public void TangentT3fv(ReadOnlySpan<float> t)=> throw new NotImplementedException();

	// Wrinkle
	public void Wrinkle1f(float flWrinkle) => throw new NotImplementedException();

	// bone weights
	public void BoneWeight(int idx, float weight) => throw new NotImplementedException();
	// bone weights (templatized for code which needs to support compressed vertices)

	// bone matrix index
	public void BoneMatrix(int idx, int matrixIndex) => throw new NotImplementedException();

	// Generic per-vertex data
	public void UserData(ReadOnlySpan<float> pData)=> throw new NotImplementedException();

	// Used to define the indices (only used if you aren't using primitives)
	public void Index(ushort index) => throw new NotImplementedException();


	private void ComputeNumVertsAndIndices(ref int pMaxVertices, ref int pMaxIndices, MaterialPrimitiveType type, int nPrimitiveCount) => throw new NotImplementedException();
	private int IndicesFromVertices(MaterialPrimitiveType type, int nVertexCount) => throw new NotImplementedException();

	// The mesh we're modifying
	IMesh? Mesh;

	MaterialPrimitiveType Type;

	// Generate indices?
	bool GenerateIndices;

	IndexBuilder IndexBuilder;
	VertexBuilder VertexBuilder;
}