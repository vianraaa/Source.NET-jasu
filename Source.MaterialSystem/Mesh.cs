using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Source.MaterialSystem;

public unsafe class VertexBuffer
{
	VertexFormat VertexBufferFormat;
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

	int vao = -1;
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

	public void RecomputeVAO() {
		// Unlike the VBO, we do not need to destroy everything when the state changes
		if (this.vao == -1)
			this.vao = (int)glCreateVertexArray();

		// But we need a VBO first
		if (this.vbo == -1)
			RecomputeVBO();

		uint vao = (uint)this.vao;
		int sizeof1vertex = 0;

		Span<uint> bindings = stackalloc uint[64];
		int bindingsPtr = 0;

		for (VertexElement i = 0; i < VertexElement.Count; i++) {
			uint elementAttribute = (uint)i;
			VertexFormat bitmask = (VertexFormat)(1 << (int)elementAttribute);
			bool enabled = (VertexBufferFormat & bitmask) == bitmask;
			if (!enabled) {
				glDisableVertexArrayAttrib(vao, elementAttribute);
				continue;
			}

			i.GetInformation(out int count, out VertexAttributeType type);
			int elementSize = count * (int)type.SizeOf();
			glEnableVertexArrayAttrib(vao, elementAttribute);
			// type is relative to OpenGL's enumeration
			glVertexArrayAttribFormat(vao, elementAttribute, count, (int)type, false, (uint)elementSize);

			bindings[bindingsPtr++] = elementAttribute;
			sizeof1vertex += elementSize;
		}

		// Bind the VBO to the VAO here
		glVertexArrayVertexBuffer(vao, 0, (uint)vbo, 0, sizeof1vertex);

		Assert(bindingsPtr < bindings.Length);
		for (int i = 0; i < bindings.Length; i++) {
			// Bind every enabled element to the 0th buffer (we don't use other buffers)
			glVertexArrayAttribBinding(vao, bindings[i], 0);
		}

	}

	public int NextLockOffset() {
		int nextOffset = (Position + VertexSize - 1) / VertexSize;
		nextOffset *= VertexSize;
		return nextOffset;
	}

	internal void ChangeConfiguration(int vertexSize, int totalSize) {
		VertexSize = vertexSize;
		VertexCount = BufferSize / vertexSize;
		RecomputeVBO();
	}

	private unsafe void RecomputeVBO() {
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

		RecomputeVAO();
	}

	public byte* Lock(int numVerts, out int baseVertexIndex) {
		Assert(!Locked);
		baseVertexIndex = 0;
		if (SysmemBuffer == null) {
			RecomputeVBO();
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

	unsafe static nint dummyData = (nint)NativeMemory.AlignedAlloc(512, 16);

	public static unsafe void ComputeVertexDescription(byte* vertexMemory, VertexFormat vertexFormat, ref VertexDesc desc) {
		fixed (VertexDesc* descPtr = &desc) {
			nint offset = 0;
			nint baseptr = (nint)vertexMemory;
			int** vertexSizesToSet = stackalloc int*[64];
			int vertexSizesToSetPtr = 0;

			for (VertexElement element = 0; element < VertexElement.Count; element++) {
				VertexFormat formatMask = (VertexFormat)(1 << (int)element);
				bool enabled = (vertexFormat & formatMask) == formatMask;
				nint elementSize = element.GetSize();
				switch (element) {
					case VertexElement.Position:
						if (enabled) {
							descPtr->Position = (float*)(baseptr + offset);
							vertexSizesToSet[vertexSizesToSetPtr++] = &descPtr->PositionSize;
						}
						else {
							descPtr->Position = (float*)dummyData;
							descPtr->PositionSize = 0;
						}
						break;
					case VertexElement.Normal:
						if (enabled) {
							descPtr->Normal = (float*)(baseptr + offset);
							vertexSizesToSet[vertexSizesToSetPtr++] = &descPtr->NormalSize;
						}
						else {
							descPtr->Normal = (float*)dummyData;
							descPtr->NormalSize = 0;
						}
						break;
					case VertexElement.Color:
						if (enabled) {
							descPtr->Color = (byte*)(baseptr + offset);
							vertexSizesToSet[vertexSizesToSetPtr++] = &descPtr->ColorSize;
						}
						else {
							descPtr->Color = (byte*)dummyData;
							descPtr->ColorSize = 0;
						}
						break;
					case VertexElement.Specular:
						if (enabled) {
							descPtr->Specular = (byte*)(baseptr + offset);
							vertexSizesToSet[vertexSizesToSetPtr++] = &descPtr->SpecularSize;
						}
						else {
							descPtr->Specular = (byte*)dummyData;
							descPtr->SpecularSize = 0;
						}
						break;
					case VertexElement.BoneIndex:
						if (enabled) {
							descPtr->BoneIndex = (byte*)(baseptr + offset);
							vertexSizesToSet[vertexSizesToSetPtr++] = &descPtr->BoneIndexSize;
						}
						else {
							descPtr->BoneIndex = (byte*)dummyData;
							descPtr->BoneIndexSize = 0;
						}
						break;
					case VertexElement.BoneWeights:
						if (enabled) {
							descPtr->BoneWeight = (float*)(baseptr + offset);
							vertexSizesToSet[vertexSizesToSetPtr++] = &descPtr->BoneWeightSize;
						}
						else {
							descPtr->BoneWeight = (float*)dummyData;
							descPtr->BoneWeightSize = 0;
						}
						break;
					case VertexElement.TexCoord:
						if (enabled) {
							descPtr->TexCoord0 = (float*)(baseptr + offset);
							vertexSizesToSet[vertexSizesToSetPtr++] = &descPtr->TexCoordSizePtr[0];
						}
						else {
							descPtr->TexCoord0 = (float*)dummyData;
							descPtr->TexCoordSize[0] = 0;
						}
						break;
				}

				if (enabled)
					offset += elementSize;
			}
			desc.ActualVertexSize = (int)offset;
			for (int i = 0; i < vertexSizesToSetPtr; i++) {
				*vertexSizesToSet[i] = (int)offset;
			}
		}
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

	public override MaterialPrimitiveType GetPrimitiveType() {
		return Mesh!.GetPrimitiveType();
	}
	public override void SetPrimitiveType(MaterialPrimitiveType type) {
		if (type != GetPrimitiveType()) {
			ShaderAPI.FlushBufferedPrimitives();
			Mesh!.SetPrimitiveType(type);
		}
	}

	public override void LockMesh(int vertexCount, int indexCount, ref MeshDesc desc) {
		ShaderUtil.SyncMatrices();

		Assert(Mesh != null);
		Assert(WasRendered);

		Mesh.PreLock();

		if (!Mesh.HasEnoughRoom(vertexCount, indexCount))
			ShaderAPI.FlushBufferedPrimitives();

		Mesh.LockMesh(vertexCount, indexCount, ref desc);
	}
}
public unsafe class DynamicMesh : Mesh
{
	bool HasDrawn;
	bool VertexOverride;
	bool IndexOverride;

	int TotalVertices;
	int TotalIndices;
	int FirstVertex;
	int FirstIndex;

	public void ResetVertexAndIndexCounts() {
		TotalVertices = TotalIndices = 0;
		FirstIndex = FirstVertex = -1;
		HasDrawn = false;
	}

	public override void PreLock() {
		if (HasDrawn) {
			ResetVertexAndIndexCounts();
		}
	}

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
	public override void LockMesh(int vertexCount, int indexCount, ref MeshDesc desc) {
		if (VertexOverride) {
			vertexCount = 0;
		}

		if (IndexOverride) {
			indexCount = 0;
		}

		Lock(vertexCount, false, ref desc.Vertex);
		Lock(indexCount, false, ref desc.Index);
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
		if (VertexBuffer == null) {
			// todo:
			throw new NotImplementedException();
		}

		byte* vertexMemory = VertexBuffer.Lock(vertexCount, out desc.FirstVertex);
		VertexBuffer.ComputeVertexDescription(vertexMemory, VertexFormat, ref desc);

		return true;
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

	public virtual MaterialPrimitiveType GetPrimitiveType() {
		return Type;
	}

	public virtual void SetPrimitiveType(MaterialPrimitiveType type) {
		if (!ShaderUtil.OnSetPrimitiveType(this, type))
			return;

		Type = type;
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

	public virtual void PreLock() {
		throw new NotImplementedException();
	}
}
