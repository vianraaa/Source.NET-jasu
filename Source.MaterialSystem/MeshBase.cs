using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.MaterialSystem;

public class MeshBase : IMesh
{
	public void BeginCastBuffer(VertexFormat format) {
		throw new NotImplementedException();
	}

	public void BeginCastBuffer(MaterialIndexFormat format) {
		throw new NotImplementedException();
	}

	public void Draw(int firstIndex = -1, int indexCount = 0) {
		throw new NotImplementedException();
	}

	public void EndCastBuffer() {
		throw new NotImplementedException();
	}

	public int GetRoomRemaining() {
		throw new NotImplementedException();
	}

	public VertexFormat GetVertexFormat() {
		throw new NotImplementedException();
	}

	public int IndexCount() {
		throw new NotImplementedException();
	}

	public MaterialIndexFormat IndexFormat() {
		throw new NotImplementedException();
	}

	public bool IsDynamic() {
		throw new NotImplementedException();
	}

	public bool Lock(int vertexCount, bool append, ref VertexDesc desc) {
		throw new NotImplementedException();
	}

	public bool Lock(int maxIndexCount, bool append, ref IndexDesc desc) {
		throw new NotImplementedException();
	}

	public void LockMesh(int vertexCount, int indexCount, ref MeshDesc desc) {
		throw new NotImplementedException();
	}

	public void MarkAsDrawn() {
		throw new NotImplementedException();
	}

	public void ModifyBegin(int firstVertex, int vertexCount, int firstIndex, int indexCount, ref MeshDesc desc) {
		throw new NotImplementedException();
	}

	public void ModifyEnd(ref MeshDesc desc) {
		throw new NotImplementedException();
	}

	public void SetColorMesh(IMesh colorMesh, int vertexOffset) {
		throw new NotImplementedException();
	}

	public void SetPrimitiveType(MaterialPrimitiveType type) {
		throw new NotImplementedException();
	}

	public bool Unlock(int vertexCount, ref VertexDesc desc) {
		throw new NotImplementedException();
	}

	public bool Unlock(int writtenIndexCount, ref IndexDesc desc) {
		throw new NotImplementedException();
	}

	public void UnlockMesh(int vertexCount, int indexCount, ref MeshDesc desc) {
		throw new NotImplementedException();
	}

	public int VertexCount() {
		throw new NotImplementedException();
	}

	internal void RenderPass() {
		throw new NotImplementedException();
	}
}
