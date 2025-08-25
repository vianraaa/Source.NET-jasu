using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Source.MaterialSystem;

public unsafe class VertexBuffer
{
	internal VertexFormat VertexBufferFormat;
	internal int Position;
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

	public int NextLockOffset() {
		int nextOffset = (Position + VertexSize - 1) / VertexSize;
		nextOffset *= VertexSize;
		return nextOffset;
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

	internal bool HasEnoughRoom(int numVertices) {
		return (NextLockOffset() + (numVertices * VertexSize)) <= BufferSize;
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

	internal bool HasEnoughRoom(int indexCount) {
		throw new NotImplementedException();
	}
}

public unsafe class BufferedMesh : Mesh
{
	Mesh? Mesh;
	ushort LastIndex;
	ushort ExtraIndices;
	bool IsFlushing;
	bool WasRendered;
	bool FlushNeeded;

	public void ResetRendered() {
		WasRendered = false;
	}
	public bool WasNotRendered() => !WasRendered;

	public void Flush() {
		if (Mesh != null && !IsFlushing && FlushNeeded) {
			IsFlushing = true;
			((IMesh)Mesh!)!.Draw();
			IsFlushing = false;
			FlushNeeded = false;
		}
	}

	public void SetMesh(Mesh? mesh) {
		if (Mesh != mesh) {
			ShaderAPI.FlushBufferedPrimitives();
			Mesh = mesh;
		}
	}

	public override void SetMaterial(IMaterialInternal matInternal) {
		Assert(Mesh != null);
		Mesh.SetMaterial(matInternal);
	}
	public override void SetVertexFormat(VertexFormat fmt) {
		Assert(Mesh != null);
		if (Mesh.NeedsVertexFormatReset(fmt)) {
			ShaderAPI.FlushBufferedPrimitives();
			Mesh.SetVertexFormat(fmt);
		}
	}
}
public unsafe class DynamicMesh : Mesh
{
	bool HasDrawn;
	bool VertexOverride;
	bool IndexOverride;

	internal void OverrideVertexBuffer(VertexBuffer vertexBuffer) {
		throw new NotImplementedException();
	}

	internal void OverrideIndexBuffer(IndexBuffer indexBuffer) {
		throw new NotImplementedException();
	}
	public override bool NeedsVertexFormatReset(VertexFormat fmt) {
		return VertexOverride || IndexOverride || base.NeedsVertexFormatReset(fmt);
	}
	public override bool HasEnoughRoom(int vertexCount, int indexCount) {
		if (ShaderDevice.IsDeactivated())
			return false;
		Assert(VertexBuffer != null);
		return VertexBuffer.HasEnoughRoom(vertexCount) && IndexBuffer.HasEnoughRoom(indexCount);
	}
}

public unsafe class Mesh : IMesh
{
	public IShaderAPI ShaderAPI;
	public IShaderUtil ShaderUtil;
	public MeshMgr MeshMgr;
	public IShaderDevice ShaderDevice;

	protected VertexBuffer VertexBuffer;
	protected IndexBuffer IndexBuffer;

	VertexFormat VertexFormat;
	IMaterialInternal Material;
	MaterialPrimitiveType Type;

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
		Material = matInternal;
	}

	public virtual void SetVertexFormat(VertexFormat fmt) {
		VertexFormat = fmt;
	}

	public virtual void RenderPass() {
	}

	internal bool HasColorMesh() {
		return false;
	}

	internal bool HasFlexMesh() {
		return false;
	}

	public virtual bool NeedsVertexFormatReset(VertexFormat fmt) {
		return VertexFormat != fmt;
	}

	public virtual bool HasEnoughRoom(int vertexCount, int indexCount) => true;
}
