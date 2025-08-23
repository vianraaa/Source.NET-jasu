using Source.Common.MaterialSystem;

namespace Source.MaterialSystem;

public unsafe class MeshGl46 : BaseMeshGl46 {
	protected static PrimList* s_pPrims;
	protected static nint s_nPrims;
	public static Span<PrimList> Primitives => new(s_pPrims, (int)s_nPrims);

	public override void RenderPass() {
		Warning("Cannot renderpass\n");
	}

	public MaterialPrimitiveType Type;
	public uint Mode;

	public override MaterialPrimitiveType GetPrimitiveType() {
		return Type;
	}

	public override void LockMesh(int vertexCount, int indexCount, ref MeshDesc desc) {
		ShaderUtil.SyncMatrices();
	}

	public override void SetPrimitiveType(MaterialPrimitiveType type) {
		if (!ShaderUtil.OnSetPrimitiveType(this, type))
			return;

		Type = type;
		Mode = ToGLPrimitive(type);
	}

	private uint ToGLPrimitive(MaterialPrimitiveType type) {
		switch (type) {
			case MaterialPrimitiveType.Points: return GL_POINTS;
			case MaterialPrimitiveType.Lines: return GL_LINES;
			case MaterialPrimitiveType.Triangles: return GL_TRIANGLES;
			case MaterialPrimitiveType.TriangleStrip: return GL_TRIANGLE_STRIP;
			case MaterialPrimitiveType.LineStrip: return GL_LINE_STRIP;
			case MaterialPrimitiveType.LineLoop: return GL_LINE_LOOP;
			case MaterialPrimitiveType.Quads: return GL_QUADS;
			case MaterialPrimitiveType.InstancedQuads: return GL_QUADS; // instancing handled elsewhere
			default: Assert(false); return GL_TRIANGLES;
		}
	}
}
