using Source.Common.MaterialSystem;

using System.Runtime.InteropServices;

namespace Source.MaterialSystem.Meshes;

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

	public VertexBuffer(bool dynamic) {
		Dynamic = dynamic;
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

	public void FlushASAP() => Flush = true;

	public void RecomputeVAO() {
		// Unlike the VBO, we do not need to destroy everything when the state changes
		if (this.vao == -1) {
			this.vao = (int)glCreateVertexArray();
			glObjectLabel(GL_VERTEX_ARRAY, (uint)this.vao, "MaterialSystem VertexBuffer");
		}

		// But we need a VBO first
		if (vbo == -1)
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
			// TODO: normalization ternary is kinda gross but acceptable for now...
			glVertexArrayAttribFormat(vao, elementAttribute, count, (int)type, i == VertexElement.Color ? true : false, (uint)sizeof1vertex);


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
		SysmemBuffer = NativeMemory.AllocZeroed((nuint)BufferSize);
		glNamedBufferData((uint)vbo, BufferSize, null, GL_DYNAMIC_DRAW);
		RecomputeVAO();
	}

	public byte* Lock(int numVerts, out int baseVertexIndex) {
		Assert(!Locked);

		if(numVerts > VertexCount) {
			baseVertexIndex = 0;
			return null;
		}
		if (Dynamic) {
			if(Flush || !HasEnoughRoom(numVerts)) {
				if (SysmemBuffer != null)
					LateCreateShouldDiscard = true;

				Flush = false;
				Position = 0;
			}
		}
		else {
			Position = 0;
		}
		baseVertexIndex = VertexSize == 0 ? 0 : (Position / VertexSize);
		if (SysmemBuffer == null) {
			RecomputeVBO();
		}
		Locked = true;
		return (byte*)((nint)SysmemBuffer + Position);
	}

	public void Unlock(int vertexCount) {
		if (!Locked)
			return;

		int lockOffset = NextLockOffset();
		int bufferSize = vertexCount * VertexSize;

		glNamedBufferSubData((uint)vbo, Position, bufferSize, (void*)((nint)SysmemBuffer + Position));
		Position = lockOffset + bufferSize;
		Locked = false;
	}

	internal bool HasEnoughRoom(int numVertices) {
		return NextLockOffset() + (numVertices * VertexSize) <= BufferSize;
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
