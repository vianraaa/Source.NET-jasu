namespace Source.MaterialSystem;

public unsafe class MeshGl46 : BaseMeshGl46 {
	protected static PrimList* s_pPrims;
	protected static nint s_nPrims;
	public static Span<PrimList> Primitives => new(s_pPrims, (int)s_nPrims);
}
