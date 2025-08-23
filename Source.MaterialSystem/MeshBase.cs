using Source.Common.MaterialSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.MaterialSystem;

public abstract class MeshBase : IMesh
{
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

	public virtual MaterialPrimitiveType GetPrimitiveType() {
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

	public virtual void RenderPass() {
		throw new NotImplementedException();
	}

	internal bool HasColorMesh() {
		return false;
	}

	internal bool HasFlexMesh() {
		return false;
	}

	internal void BeginPass() {

	}
}
