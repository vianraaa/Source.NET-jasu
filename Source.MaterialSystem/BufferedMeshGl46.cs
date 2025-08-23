using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public class BufferedMeshGl46 : BaseMeshGl46
{
	BaseMeshGl46? Mesh;
	ushort LastIndex;
	ushort ExtraIndices;
	bool IsFlushing;
	bool WasRendered;
	bool FlushNeeded;

	public void ResetRendered() { }
	public bool WasNotRendered() => throw new NotImplementedException();
	public void SetMesh(BaseMeshGl46? mesh) {
		if (Mesh != mesh) {
			ShaderAPI.FlushBufferedPrimitives();
			Mesh = mesh;
		}
	}
	public BaseMeshGl46? GetMesh() => Mesh;
	public virtual void Spew(int vertexCount, int indexCount, out MeshDesc spewDesc) {
		spewDesc = default;
	}
	public virtual void SetVertexFormat(VertexFormat format) {

	}
	public override void Draw(int firstIndex = -1, int indexCount = 0) {
		if (!ShaderUtil.OnDrawMesh(this, firstIndex, indexCount)) {
			WasRendered = true;
			MarkAsDrawn();
			return;
		}
		Assert(!IsFlushing && !WasRendered);
		Assert(firstIndex == -1 && indexCount == 0);
		WasRendered = true;
		FlushNeeded = true;
		if (Mesh!.HasFlexMesh())
			ShaderAPI.FlushBufferedPrimitives();
	}
	public override VertexFormat GetVertexFormat() {
		throw new NotImplementedException();
	}
	public override MaterialPrimitiveType GetPrimitiveType() {
		return Mesh!.GetPrimitiveType();
	}
	public override void SetPrimitiveType(MaterialPrimitiveType type) {
		Assert(type != MaterialPrimitiveType.InstancedQuads);
		Assert(type != MaterialPrimitiveType.Heterogenous);

		if (type != GetPrimitiveType()) {
			ShaderAPI.FlushBufferedPrimitives();
			Mesh!.SetPrimitiveType(type);
		}
	}
	public override void LockMesh(int vertexCount, int indexCount, ref MeshDesc desc) {
		ShaderUtil.SyncMatrices();

		Assert(Mesh);
		Assert(WasRendered);

		Mesh!.PreLock();

		ExtraIndices = 0;
		bool tristripFixup = (Mesh.IndexCount() != 0) &&
		(Mesh.GetPrimitiveType() == MaterialPrimitiveType.TriangleStrip);
		if (tristripFixup) {
			ExtraIndices = (ushort)((Mesh.IndexCount() & 0x1) != 0 ? 3 : 2);
			indexCount += ExtraIndices;
		}

		// Flush if we gotta
		if (!Mesh.HasEnoughRoom(vertexCount, indexCount)) {
			ShaderAPI.FlushBufferedPrimitives();
		}

		Mesh.LockMesh(vertexCount, indexCount, ref desc);

		//	This is taken care of in the function above.
		//	CBaseMeshDX8::m_bMeshLocked = true;

		// Deal with fixing up the tristrip..
		if (tristripFixup && desc.Index.IndexSize > 0) {
			Span<char> buf = stackalloc char[32];
			if (DebugTrace()) {
				if (ExtraIndices == 3)
					sprintf(buf, "Link Index: %d %d\n", LastIndex, LastIndex);
				else
					sprintf(buf, "Link Index: %d\n", LastIndex);
				Platform.DebugString(buf);
			}
			unsafe {
				*desc.Index.Indices++ = LastIndex;
				if (ExtraIndices == 3) {
					*desc.Index.Indices++ = LastIndex;
				}
				// Leave room for the last padding index
				++desc.Index.Indices;
			}
		}

		WasRendered = false;
	}

	private bool DebugTrace() {
		return false;
	}

	public override void HandleLateCreation() {
		Mesh?.HandleLateCreation();
	}

	public void Flush() {
		if (Mesh != null && !IsFlushing && FlushNeeded) {
			IsFlushing = true;
			((IMesh)Mesh!)!.Draw();
			IsFlushing = false;
			FlushNeeded = false;
		}
	}
}