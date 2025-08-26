using Raylib_cs;

using Source.Common.Engine;
using Source.Common.MaterialSystem;
using Source.Common.ShaderAPI;

using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Source.MaterialSystem;

public unsafe class VertexBuffer : IDisposable
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

	internal uint VAO() => vao > 0 ? (uint)vao : throw new NullReferenceException("Vertex Array Object was null");
	internal uint VBO() => vbo > 0 ? (uint)vbo : throw new NullReferenceException("Vertex Buffer Object was null");

	public VertexBuffer() {
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
			glVertexArrayAttribFormat(vao, elementAttribute, count, (int)type, false, (uint)sizeof1vertex);

			bindings[bindingsPtr++] = elementAttribute;
			sizeof1vertex += elementSize;
		}

		// Bind the VBO to the VAO here
		glVertexArrayVertexBuffer(vao, 0, (uint)vbo, 0, sizeof1vertex);

		Assert(bindingsPtr < bindings.Length);
		for (int i = 0; i < bindingsPtr; i++) {
			// Bind every enabled element to the 0th buffer (we don't use other buffers)
			glVertexArrayAttribBinding(vao, bindings[i], 0);
		}

	}

	public int NextLockOffset() {
		int nextOffset = (Position + VertexSize - 1) / VertexSize;
		nextOffset *= VertexSize;
		return nextOffset;
	}

	internal void ChangeConfiguration(VertexFormat format, int vertexSize, int totalSize) {
		VertexBufferFormat = format;
		VertexSize = vertexSize;
		VertexCount = BufferSize / vertexSize;
		RecomputeVBO();
	}

	public unsafe void RecomputeVBO() {
		Dispose();

		vbo = (int)glCreateBuffer();
		glNamedBufferStorage((uint)vbo, BufferSize, null, GL_MAP_WRITE_BIT | GL_MAP_PERSISTENT_BIT | GL_MAP_COHERENT_BIT);
		SysmemBuffer = glMapNamedBufferRange((uint)vbo, 0, BufferSize, GL_MAP_WRITE_BIT | GL_MAP_PERSISTENT_BIT | GL_MAP_COHERENT_BIT);
		if (SysmemBuffer == null) {
			Warning("WARNING: RecomputeVBO failure (OpenGL's not happy...)\n");
			Warning($"    OpenGL error code    : {glGetErrorName()}\n");
			Warning($"    Vertex buffer object : {vbo}\n");
			Warning($"    Attempted alloc size : {BufferSize}\n");
		}
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

	public void Unlock(int vertexCount) {
		if (!Locked)
			return;

		int lockOffset = NextLockOffset();
		int bufferSize = vertexCount * VertexSize;

		Position = lockOffset + BufferSize;

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

	public void Dispose() {
		if (vbo != -1) {
			Assert(SysmemBuffer != null);
			fixed (int* ugh = &vbo)
				glDeleteBuffers(1, (uint*)ugh);
			vbo = -1;
			SysmemBuffer = null;
		}
	}

	internal void HandleLateCreation() {

	}
}

public unsafe class IndexBuffer : IDisposable
{
	internal MaterialIndexFormat IndexFormat;
	internal int IndexCount;
	internal int Position;
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
	public unsafe void RecomputeIBO() {
		Dispose();
		ibo = (int)glCreateBuffer();
		glNamedBufferStorage((uint)ibo, BufferSize, null, GL_MAP_WRITE_BIT | GL_MAP_PERSISTENT_BIT | GL_MAP_COHERENT_BIT);
		SysmemBuffer = glMapNamedBufferRange((uint)ibo, 0, BufferSize, GL_MAP_WRITE_BIT | GL_MAP_PERSISTENT_BIT | GL_MAP_COHERENT_BIT);

		if (SysmemBuffer == null) {
			Warning("WARNING: RecomputeIBO failure (OpenGL's not happy...)\n");
			Warning($"    OpenGL error code    : {glGetErrorName()}\n");
			Warning($"    Vertex buffer object : {ibo}\n");
			Warning($"    Attempted alloc size : {BufferSize}\n");
		}
	}

	public short* Lock(bool readOnly, int indexCount, out int startIndex, int firstIndex) {
		Assert(!Locked);
		startIndex = 0;
		if (SysmemBuffer == null) {
			RecomputeIBO();
		}
		Locked = true;
		return (short*)SysmemBuffer;
	}

	public void Unlock(int indexCount) {
		if (!Locked)
			return;

		Position += indexCount;
		Locked = false;
	}

	internal bool HasEnoughRoom(int indices) {
		return (indices + Position) <= IndexCount;
	}

	public void Dispose() {
		if (ibo != -1) {
			Assert(SysmemBuffer != null);
			fixed (int* ugh = &ibo)
				glDeleteBuffers(1, (uint*)ugh);
			ibo = -1;
			SysmemBuffer = null;
		}
	}

	public IndexBuffer(int count, bool dynamic = false) {
		Position = 0;
		Locked = false;
		Flush = true;
		Dynamic = dynamic;
		ExternalMemory = false;
		LateCreateShouldDiscard = false;

		count += (count % 2);
		IndexCount = count;

		BufferSize = sizeof(ushort) * IndexCount;

		RecomputeIBO();
	}

	internal void HandleLateCreation() {

	}

	internal uint IBO() => ibo > 0 ? (uint)ibo : throw new NullReferenceException("Index Buffer Object was null");
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

	public override void UnlockMesh(int vertexCount, int indexCount, ref MeshDesc desc) {
		if ((Mesh!.GetPrimitiveType() == MaterialPrimitiveType.TriangleStrip) && desc.Index.IndexSize > 0) {
			if (ExtraIndices > 0) 
				*(desc.Index.Indices - 1) = *desc.Index.Indices;

			LastIndex = desc.Index.Indices[indexCount - 1];
			indexCount += ExtraIndices;
		}

		Mesh.UnlockMesh(vertexCount, indexCount, ref desc);
	}

	public override void Draw(int firstIndex = -1, int indexCount = 0) {
		if(!ShaderUtil.OnDrawMesh(this, firstIndex, indexCount)) {
			WasRendered = true;
			MarkAsDrawn();
			return;
		}

		Assert(!IsFlushing && !WasRendered);
		Assert(firstIndex == -1 && indexCount == 0);

		WasRendered = true;
		FlushNeeded = true;
		// ShaderAPI.FlushBufferedPrimitives(); is sometimes called here... figure out why later?
	}
}
public unsafe class DynamicMesh : Mesh
{
	bool HasDrawn;
	bool VertexOverride;
	bool IndexOverride;

	int BufferId;
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
		int firstIndex = Lock(false, -1, indexCount, ref desc.Index);
		if (FirstIndex < 0)
			FirstIndex = firstIndex; // ???????????????
	}

	public void Init(int bufferId) {
		BufferId = bufferId;
	}

	public override void SetVertexFormat(VertexFormat format) {
		if (ShaderDevice.IsDeactivated())
			return;

		if (format != VertexFormat || VertexOverride || IndexOverride) {
			VertexFormat = format;
			UseVertexBuffer(MeshMgr.FindOrCreateVertexBuffer(BufferId, format));

			if (BufferId == 0)
				UseIndexBuffer(MeshMgr.GetDynamicIndexBuffer());

			VertexOverride = IndexOverride = false;
		}
	}
	public override void UnlockMesh(int vertexCount, int indexCount, ref MeshDesc desc) {
		TotalVertices += vertexCount;
		TotalIndices += indexCount;
		base.UnlockMesh(vertexCount, indexCount, ref desc);
	}
	public override void Draw(int firstIndex = -1, int indexCount = 0) {
		if(!ShaderUtil.OnDrawMesh(this, firstIndex, indexCount)) {
			MarkAsDrawn();
			return;
		}

		HasDrawn = true;

		if (IndexOverride || VertexOverride || ((TotalVertices > 0) && (TotalIndices > 0 || Type == MaterialPrimitiveType.Points || Type == MaterialPrimitiveType.InstancedQuads))) {
			Assert(!IsDrawing);

			HandleLateCreation();

			// only have a non-zero first vertex when we are using static indices
			int nFirstVertex = VertexOverride ? 0 : FirstVertex;
			int actualFirstVertex = IndexOverride ? nFirstVertex : 0;
			int nVertexOffsetInBytes = HasFlexMesh() ? nFirstVertex * MeshMgr.VertexFormatSize(GetVertexFormat()) : 0;
			int baseIndex = IndexOverride ? 0 : FirstIndex;

			// Overriding with the dynamic index buffer, preserve state!
			if (IndexOverride && IndexBuffer == MeshMgr.GetDynamicIndexBuffer()) {
				baseIndex = FirstIndex;
			}

			VertexFormat fmt = VertexOverride ? GetVertexFormat() : VertexFormat.Invalid;
			if (!SetRenderState(nVertexOffsetInBytes, actualFirstVertex, fmt))
				return;

			// Draws a portion of the mesh
			int numVertices = VertexOverride ? VertexBuffer.VertexCount : TotalVertices;
			if ((firstIndex != -1) && (indexCount != 0)) {
				firstIndex += baseIndex;
			}
			else {
				firstIndex = baseIndex;
				if (IndexOverride) {
					indexCount = IndexBuffer.IndexCount;
					Assert(indexCount != 0);
				}
				else {
					indexCount = TotalIndices;

					if ((Type == MaterialPrimitiveType.Points) || (Type == MaterialPrimitiveType.InstancedQuads)) 
						indexCount = TotalVertices;
					
					Assert(indexCount != 0);
				}
			}

			// Fix up nFirstVertex to indicate the first vertex used in the data
			if (!HasFlexMesh()) {
				actualFirstVertex = nFirstVertex - actualFirstVertex;
			}

			s_FirstVertex = (uint)actualFirstVertex;
			s_NumVertices = (uint)numVertices;

			PrimList* prim = stackalloc PrimList[1];
			prim->FirstIndex = firstIndex;
			prim->NumIndices = indexCount;
			Assert(indexCount != 0);
			s_Prims = prim;
			s_PrimsCount = 1;

			DrawMesh();

			s_Prims = null;
		}
	}
}

public struct PrimList {
	public int FirstIndex;
	public int NumIndices;
}

public unsafe class Mesh : IMesh
{
	public IShaderAPI ShaderAPI;
	public IShaderUtil ShaderUtil;
	public MeshMgr MeshMgr;
	public IShaderDevice ShaderDevice;

	protected VertexBuffer VertexBuffer;
	protected IndexBuffer IndexBuffer;

	protected VertexFormat LastVertexFormat;
	protected VertexFormat VertexFormat;
	protected IMaterialInternal Material;
	protected MaterialPrimitiveType Type = MaterialPrimitiveType.Triangles;
	protected bool IsDrawing;

	protected static PrimList* s_Prims;
	protected static int s_PrimsCount;
	protected static uint s_FirstVertex;
	protected static uint s_NumVertices;


	protected int Mode = ComputeMode(MaterialPrimitiveType.Triangles);
	protected int TotalVertices;
	protected int TotalIndices;
	protected int FirstVertex;
	protected int FirstIndex;

	public VertexBuffer GetVertexBuffer() => throw new Exception();
	public IndexBuffer GetIndexBuffer() => throw new Exception();

	public virtual void BeginCastBuffer(VertexFormat format) {
		throw new NotImplementedException();
	}
	public void DrawMesh() {
		Assert(!IsDrawing);
		IsDrawing = true;

		ShaderAPI.DrawMesh(this);

		IsDrawing = false;
	}

	protected virtual bool SetRenderState(int vertexOffsetInBytes, int firstIndex, VertexFormat vertexFormat) {
		if (ShaderDevice.IsDeactivated()) {
			ResetMeshRenderState();
			return false;
		}

		LastVertexFormat = vertexFormat;

		SetVertexStreamState(vertexOffsetInBytes);
		SetIndexStreamState(firstIndex);

		return true;
	}

	// What do these do...
	private void SetVertexStreamState(int vertexOffsetInBytes) {

	}

	private void SetIndexStreamState(object firstIndex) {

	}

	private void ResetMeshRenderState() {
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
		return VertexFormat;
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

	public void HandleLateCreation() {
		VertexBuffer?.HandleLateCreation();
		IndexBuffer?.HandleLateCreation();
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

	public virtual int Lock(bool readOnly, int firstIndex, int indexCount, ref IndexDesc desc) {
		if (ShaderDevice.IsDeactivated() || indexCount == 0) {
			Assert(false);
			return 0;
		}

		desc.Indices = (ushort*)IndexBuffer.Lock(readOnly, indexCount, out int startIndex, firstIndex);
		if (desc.Indices == null) {
			desc.IndexSize = 0;
			Assert(false);
			Warning("Failed to lock index buffer...\n");
			return 0;
		}

		desc.IndexSize = 1;
		return startIndex;
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
		Mode = ComputeMode(type);
	}

	public virtual bool Unlock(int vertexCount, ref VertexDesc desc) {
		VertexBuffer.Unlock(vertexCount);
		return true;
	}

	public virtual bool Unlock(int indexCount, ref IndexDesc desc) {
		IndexBuffer.Unlock(indexCount);
		return true;
	}

	int NumVertices;
	int NumIndices;

	public virtual void UnlockMesh(int vertexCount, int indexCount, ref MeshDesc desc) {
		Unlock(vertexCount, ref desc.Vertex);
		if (Type != MaterialPrimitiveType.Points)
			Unlock(indexCount, ref desc.Index);

		NumVertices = vertexCount;
		NumIndices = indexCount;
	}

	public virtual int VertexCount() {
		return NumVertices;
	}

	public virtual void SetMaterial(IMaterialInternal matInternal) {
		Material = matInternal;
	}

	public virtual void SetVertexFormat(VertexFormat fmt) {
		VertexFormat = fmt;
	}

	public virtual unsafe void RenderPass() {
		HandleLateCreation();
		Assert(Type != MaterialPrimitiveType.Heterogenous);

		for (int iPrim = 0; iPrim < s_PrimsCount; iPrim++) {
			PrimList* pPrim = &s_Prims[iPrim];

			if (pPrim->NumIndices == 0)
				continue;

			if ((Type == MaterialPrimitiveType.Points) || (Type == MaterialPrimitiveType.InstancedQuads)) {
				throw new NotImplementedException();
			}
			else {
				int numPrimitives = NumPrimitives(s_NumVertices, pPrim->NumIndices);

				CheckIndices(pPrim, numPrimitives);
				uint vao = VertexBuffer!.VAO();
				uint ibo = IndexBuffer!.IBO();
				glVertexArrayElementBuffer(vao, ibo);
				glBindVertexArray(vao);
				glDrawElements(Mode, pPrim->NumIndices, GL_UNSIGNED_SHORT, (void*)pPrim->FirstIndex);
			}
		}
	}

	public static int ComputeMode(MaterialPrimitiveType type) {
		switch (type) {
			case MaterialPrimitiveType.Points: return GL_POINTS;
			case MaterialPrimitiveType.Lines: return GL_LINES;
			case MaterialPrimitiveType.Triangles: return GL_TRIANGLES;
			case MaterialPrimitiveType.TriangleStrip: return GL_TRIANGLE_STRIP;
			default: throw new Exception();
		}
	}

	private int NumPrimitives(uint vertexCount, int indexCount) {
		switch (Mode) {
			case GL_POINTS: return (int)vertexCount;
			case GL_LINES: return indexCount / 2;
			case GL_TRIANGLES: return indexCount / 3;
			case GL_TRIANGLE_STRIP: return indexCount - 2;
			default: Assert(0); return 0;
		}
	}

	private void CheckIndices(PrimList* pPrim, int numPrimitives) {
		int indexCount = 0;
		if (Mode == GL_TRIANGLES) {
			indexCount = numPrimitives * 3;
		}
		else if (Mode == GL_TRIANGLE_STRIP) {
			indexCount = numPrimitives + 2;
		}

		if (indexCount != 0) {
			// TODO: Should index buffer be global? Why the hell was it global before??
			Assert(pPrim->FirstIndex >= 0 && pPrim->FirstIndex < IndexBuffer.IndexCount);

			for (int j = 0; j < indexCount; j++) { // TODO
				//uint index = IndexBuffer.GetShadowIndex(j + pPrim->FirstIndex);

				//if (index >= s_FirstVertex && index < s_FirstVertex + s_NumVertices) {
				//	continue;
				//}

				//Assert(false);
			}
		}
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

	public virtual void UseVertexBuffer(VertexBuffer vertexBuffer) {
		VertexBuffer = vertexBuffer;
	}

	public virtual void UseIndexBuffer(IndexBuffer indexBuffer) {
		IndexBuffer = indexBuffer;
	}
}
