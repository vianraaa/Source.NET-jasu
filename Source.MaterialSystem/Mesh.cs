using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Source.MaterialSystem;

public unsafe class VertexBuffer
{
	internal VertexFormat VertexBufferFormat;
	internal int VertexCount;
	internal int VertexSize;
	internal void* SysmemBuffer;
	internal int SysmemBufferStartBytes;
	internal int BufferSize;

	internal uint LockCount;
	internal bool Dynamic;
	internal bool Locked;
	internal bool Flush;
	internal bool ExternalMemory;
	internal bool SoftwareVertexProcessing;
	internal bool LateCreateShouldDiscard;

	int vbo = -1;

	public VertexBuffer(int vertexSize, int vertexCount, bool dynamic) {
		Assert(Dynamic && !Locked && vertexSize > 0);
		VertexSize = vertexSize;
		VertexCount = BufferSize / vertexSize;
		BufferSize = VertexSize * VertexCount;
	}

	public VertexBuffer(VertexFormat format, int vertexSize, int vertexCount, bool dynamic) {
		VertexBufferFormat = format;
		VertexSize = vertexSize;
		VertexCount = vertexCount;
		BufferSize = VertexSize * VertexCount;
		Dynamic = dynamic;
		Locked = false;
		Flush = true;
		ExternalMemory = false;
	}

	public static int VertexFormatSize(VertexFormat vertexFormat) {
		VertexCompressionType compression = vertexFormat.CompressionType();

		int offset = 0;
		Assert((((int)vertexFormat & VertexFormatFlags.VertexFormatWrinkle) == 0) || ((vertexFormat & VertexFormat.Position) != 0));
		if ((vertexFormat & VertexFormat.Position) > 0) {
			offset += IMaterialExts.GetVertexElementSize(VertexElement.Position, compression);
			if (((int)vertexFormat & VertexFormatFlags.VertexFormatWrinkle) > 0) 
				offset += IMaterialExts.GetVertexElementSize(VertexElement.Wrinkle, compression);
		}
		
		int numBoneWeights = vertexFormat.NumBoneWeights();

		if (((int)vertexFormat & VertexFormatFlags.VertexFormatBoneIndex) > 0)
			if (numBoneWeights > 0) 
				offset += IMaterialExts.GetVertexElementSize(VertexElement.BoneWeights2, compression);
			offset += IMaterialExts.GetVertexElementSize(VertexElement.BoneIndex, compression);


		if (((int) vertexFormat & VertexFormatFlags.VertexFormatNormal) > 0) 
			offset += IMaterialExts.GetVertexElementSize(VertexElement.Normal, compression);

		if (((int)vertexFormat & VertexFormatFlags.VertexFormatColor) > 0)
			offset += IMaterialExts.GetVertexElementSize(VertexElement.Color, compression);

		if (((int)vertexFormat & VertexFormatFlags.VertexFormatSpecular) > 0)
			offset += IMaterialExts.GetVertexElementSize(VertexElement.Specular, compression);

		// Set up texture coordinates
			Span<VertexElement> texCoordElements = [VertexElement.TexCoord1D_0, VertexElement.TexCoord2D_0, VertexElement.TexCoord3D_0, VertexElement.TexCoord4D_0];
		int i;
		for (i = 0; i < IMesh.VERTEX_MAX_TEXTURE_COORDINATES; ++i) {
			// FIXME: compress texcoords to SHORT2N/SHORT4N, with a scale rolled into the texture transform
			int nSize = vertexFormat.TexCoordSize(i);
			if (nSize != 0) {
				VertexElement texCoordElement = texCoordElements[nSize - 1] + i;
				offset += IMaterialExts.GetVertexElementSize(texCoordElement, compression);
			}
		}

		// Binormal + tangent...
		// Note we have to put these at the end so the vertex is FVF + stuff at end
		if (((int)vertexFormat & VertexFormatFlags.VertexFormatTangentS) > 0)
			offset += IMaterialExts.GetVertexElementSize(VertexElement.TangentS, compression);

		if(((int)vertexFormat & VertexFormatFlags.VertexFormatTangentT) > 0)
			offset += IMaterialExts.GetVertexElementSize(VertexElement.TangentT, compression);

		// User data..
		int userDataSize = vertexFormat.UserDataSize();
		if (userDataSize > 0) {
			VertexElement userDataElement = VertexElement.UserData1 + (userDataSize - 1);
			offset += IMaterialExts.GetVertexElementSize(userDataElement, compression);
		}

		bool bCacheAlign = ((int)vertexFormat & VertexFormatFlags.VertexFormatUseExactFormat) == 0;
		if (bCacheAlign && (offset > 16) && IsPC()) {
			offset = (offset + 0xF) & (~0xF);
		}

		return offset;
	}

	internal void ChangeConfiguration(int vertexSize, int totalSize) {
		VertexSize = vertexSize;
		VertexCount = BufferSize / vertexSize;
	}

	private unsafe void SetupSysmem() {
		if (vbo != -1) {
			Assert(SysmemBuffer != null);
			fixed (int* ugh = &vbo)
				glDeleteBuffers(1, (uint*)vbo);
			vbo = -1;
			SysmemBuffer = null;
		}
		vbo = (int)glCreateBuffer();
		glNamedBufferStorage((uint)vbo, BufferSize, null, GL_MAP_WRITE_BIT | GL_MAP_PERSISTENT_BIT | GL_MAP_COHERENT_BIT);
		SysmemBuffer = glMapNamedBufferRange((uint)vbo, 0, BufferSize, GL_MAP_WRITE_BIT | GL_MAP_PERSISTENT_BIT | GL_MAP_COHERENT_BIT);
	}

	public byte* Lock(int numVerts, out int baseVertexIndex) {
		Assert(!Locked);
		baseVertexIndex = 0;
		if (SysmemBuffer == null) {
			SetupSysmem();
		}
		Locked = true;
		return (byte*)SysmemBuffer;
	}

	public void Unlock() {
		if (!Locked)
			return;

		Locked = false;
	}
}

public unsafe class IndexBuffer
{
	internal MaterialIndexFormat IndexFormat;
	internal int VertexCount;
	internal int VertexSize;
	internal void* SysmemBuffer;
	internal int SysmemBufferStartBytes;
	internal int BufferSize;

	internal uint LockCount;
	internal bool Dynamic;
	internal bool Locked;
	internal bool Flush;
	internal bool ExternalMemory;
	internal bool SoftwareVertexProcessing;
	internal bool LateCreateShouldDiscard;

	int ibo = -1;
	byte* mem;
	private unsafe void SetupSysmem() {
		if (ibo != -1) {
			Assert(SysmemBuffer != null);
			fixed (int* ugh = &ibo)
				glDeleteBuffers(1, (uint*)ibo);
			ibo = -1;
			SysmemBuffer = null;
		}
		ibo = (int)glCreateBuffer();
		glNamedBufferStorage((uint)ibo, BufferSize, null, GL_MAP_WRITE_BIT | GL_MAP_PERSISTENT_BIT | GL_MAP_COHERENT_BIT);
		SysmemBuffer = glMapNamedBufferRange((uint)ibo, 0, BufferSize, GL_MAP_WRITE_BIT | GL_MAP_PERSISTENT_BIT | GL_MAP_COHERENT_BIT);
	}

	public short* Lock(int numVerts, out int baseVertexIndex) {
		Assert(!Locked);
		baseVertexIndex = 0;
		if (SysmemBuffer == null) {
			SetupSysmem();
		}
		Locked = true;
		return (short*)SysmemBuffer;
	}

	public void Unlock() {
		if (!Locked)
			return;

		Locked = false;
	}
}

public unsafe class BufferedMesh : Mesh
{
	public void Flush() {

	}
}
public unsafe class DynamicMesh : Mesh { }

public unsafe class Mesh : IMesh
{
	public IShaderAPI ShaderAPI;
	public IShaderUtil ShaderUtil;
	public MeshMgr MeshMgr;
	public IShaderDevice ShaderDevice;

	public VertexBuffer GetVertexBuffer() => throw new Exception();
	public IndexBuffer GetIndexBuffer() => throw new Exception();

	public virtual void BeginCastBuffer(VertexFormat format) {
		throw new NotImplementedException();
	}

	public virtual void BeginCastBuffer(MaterialIndexFormat format) {
		throw new NotImplementedException();
	}

	public virtual void Draw(int firstIndex = -1, int indexCount = 0) {
		throw new NotImplementedException();
	}

	public virtual void EndCastBuffer() {
		throw new NotImplementedException();
	}

	public virtual int GetRoomRemaining() {
		throw new NotImplementedException();
	}

	public virtual VertexFormat GetVertexFormat() {
		throw new NotImplementedException();
	}

	public virtual int IndexCount() {
		throw new NotImplementedException();
	}

	public virtual MaterialIndexFormat IndexFormat() {
		throw new NotImplementedException();
	}

	public virtual bool IsDynamic() {
		throw new NotImplementedException();
	}

	public virtual bool Lock(int vertexCount, bool append, ref VertexDesc desc) {
		throw new NotImplementedException();
	}

	public virtual bool Lock(int maxIndexCount, bool append, ref IndexDesc desc) {
		throw new NotImplementedException();
	}

	public virtual void LockMesh(int vertexCount, int indexCount, ref MeshDesc desc) {
		throw new NotImplementedException();
	}

	public virtual void MarkAsDrawn() {
		throw new NotImplementedException();
	}

	public virtual void ModifyBegin(int firstVertex, int vertexCount, int firstIndex, int indexCount, ref MeshDesc desc) {
		throw new NotImplementedException();
	}

	public virtual void ModifyEnd(ref MeshDesc desc) {
		throw new NotImplementedException();
	}

	public virtual void SetColorMesh(IMesh colorMesh, int vertexOffset) {
		throw new NotImplementedException();
	}

	public virtual void SetPrimitiveType(MaterialPrimitiveType type) {
		throw new NotImplementedException();
	}

	public virtual bool Unlock(int vertexCount, ref VertexDesc desc) {
		throw new NotImplementedException();
	}

	public virtual bool Unlock(int writtenIndexCount, ref IndexDesc desc) {
		throw new NotImplementedException();
	}

	public virtual void UnlockMesh(int vertexCount, int indexCount, ref MeshDesc desc) {
		throw new NotImplementedException();
	}

	public virtual int VertexCount() {
		throw new NotImplementedException();
	}

	public virtual void SetMaterial(IMaterialInternal matInternal) {
		throw new NotImplementedException();
	}

	public virtual void SetVertexFormat(VertexFormat fmt) {
		throw new NotImplementedException();
	}
}
