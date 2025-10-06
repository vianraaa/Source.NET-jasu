namespace Source.Common;

public class RecvPropExtra_UtlVector 
{
	public int MaxElements;
	public ResizeVectorFn ResizeFn;
	public EnsureCapacityFn EnsureCapacityFn;
	public DataTableRecvVarProxyFn DataTableProxyFn;
	public RecvVarProxyFn ProxyFn;
	public IFieldAccessor FieldInfo;

	public int Index;

	public RecvPropExtra_UtlVector Clone() => (RecvPropExtra_UtlVector)MemberwiseClone();
}
